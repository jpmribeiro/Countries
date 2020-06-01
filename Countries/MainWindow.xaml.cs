namespace Countries
{
    using Services;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.IO;
    using Svg;
    using System.Globalization;
    using System.Net;
    using System.Drawing.Imaging;
    using System.Text.RegularExpressions;
    using System.Web;
    using NPOI.HSSF.Record;
    using System.Net.Http;
    using WpfAnimatedGif;
    using System.Drawing;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> ListOfCountries;
        private List<Rates> ListOfApiRates;
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        private readonly DialogService dialogService;
        private readonly DataService dataService;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCountries();
        }

        /// <summary>
        /// Determines if the Program loads information from the API or the DB
        /// </summary>
        private async Task LoadCountries()
        {
            bool load;

            labelReport.Content = "Please wait while data is being loaded...";

            var connection = networkService.CheckConnection();

            if (!connection.IsSucess) //If there is no Internet Connection Available
            {
                await LoadLocalDataCountries();

                List<Country> listBoxList = ListOfCountries;
                listBoxCountries.ItemsSource = listBoxList;

                load = false;

            }
            else  //If there is Internet Connection
            {
                await LoadApiCountries();
                await LoadApiRates();
                await LoadApiWikipedia();

                List<Country> listBoxList = ListOfCountries;
                listBoxCountries.ItemsSource = listBoxList;

                load = true;

                await SaveCountryData();
                await SaveCountryRates();

            }

            if (load)
            {
                labelReport.Content = string.Format("(Data uploaded from the Internet, ({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }
            else
            {
                labelReport.Content = string.Format("(Data uploaded from a local repository({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }

            if ((ListOfCountries == null) || (ListOfCountries.Count == 0))
            {
                labelReport.Content = "Plese make sure you have a Internet Connection when you use the application for the first time. Try Again Later!";

                MessageBox.Show("Before using the App for the first time make sure there is a Internet Connection");

                return;
            }

        }




        /// <summary>
        /// Calls restcountires.eu API, deletes the DB, if it exists, and Saves a updated DB
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiCountries()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            await dataService.DeleteData();

            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all", progress);
            ListOfCountries = (List<Country>)response.Result;

            //await dataService.SaveDataCountry(ListOfCountries, progress);
            //labelReport.Content = "Saving Countries Data into DB.";

            //Images 

            if (!Directory.Exists("ImagesPNG")) //If directory doesn't exist
            {
                foreach (var item in ListOfCountries)
                {
                    if (!File.Exists(Environment.CurrentDirectory + "/ImagesPNG" + $"/{item.Name}.png"))
                    {
                        await dataService.SaveFlag(item.Flag, item.Name);
                    }
                }

                Directory.Delete("Images", true);
            }

        }

        /// <summary>
        ///  /// <summary>
        /// Calls cambiosrafa API
        /// </summary>
        /// <returns></returns>
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            labelReport.Content = "Downloading Rates Information.";

            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/Rates", progress);
            ListOfApiRates = (List<Rates>)response.Result;

            //await dataService.SaveDataRates(ListOfApiRates, progress);
            //labelReport.Content = "Saving Rates Data into DB.";
        }

        /// <summary>
        /// Calls holidays API
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private async Task LoadApiHoliday(string c) //FAZER COM QUE SO ENTRE AQUI SE HOUVER LIGACAO A NET
        {
            try
            {
                int auxcount = 0;
                labelReport.Content = "Updating Holidays Information.";

                var response2 = await apiService.GetHolidays("https://holidayapi.com", $"/v1/holidays?pretty&key=f007d74d-60a8-448f-8974-b3805fea0463&country={c}&year=2019");
                var ListOfCountryHolidays = (CountryHoliday)response2.Result;

                //await dataService.SaveDataHolidays(ListOfCountryHolidays);

                List<Holiday> listaux = new List<Holiday>();

                if (ListOfCountryHolidays != null)
                {
                    foreach (var holiday in ListOfCountryHolidays.holidays)
                    {
                        listaux.Add(holiday);
                        auxcount++;
                    }

                    listBoxCountriesHolidays.ItemsSource = listaux;
                    countHolidays.Content = $"{auxcount} Holidays:";
                }
                else
                {
                    countHolidays.Content = $"There is no Information Available regarding this Country's Holidays";
                    listBoxCountriesHolidays.ItemsSource = null;
                }
            }
            catch
            {
                MessageBox.Show("Error While loading Countries Holidays Info");
            }
        }

        /// <summary>
        /// Calls WikipediaAPI
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiWikipedia()
        {
            List<string> RegionList = new List<string>();

            if (!Directory.Exists("CountriesTexts"))
            {
                Directory.CreateDirectory("CountriesTexts");
            }

            foreach (var c in ListOfCountries)
            {
                string path = Environment.CurrentDirectory + "/CountriesTexts" + $"/{c.Alpha2Code}.txt";
                string BackupPath = Environment.CurrentDirectory + "/BackupCountriesTexts" + $"/{c.Alpha2Code}.txt";

                FileInfo textFile = new FileInfo(path);

                try
                {
                    if (!File.Exists(path)) //If there isn't a txt file containing a text about the specified country
                    {
                        var response = await GetText("https://en.wikipedia.org/w/api.php", $"?format=xml&action=query&prop=extracts&titles={c.Name.Replace((' '), ('_')).Replace(("'"), (""))}&redirects=true", c);
                        string output = (string)response.Result;

                        using (StreamWriter sw = File.CreateText(path))
                        {
                            if ((c.Alpha2Code == "CG") || (c.Alpha2Code == "GE")) //Republic of Congo and Georgia Alpha Codes are ambiguous in wikipedia
                            {
                                if (File.Exists(BackupPath)) //If there is a file in the BackUps Folder with those Alpha Codes
                                {
                                    textFile = new FileInfo(BackupPath);
                                    File.Delete(path);
                                    textFile.CopyTo(path);
                                }
                            }
                            else if (!string.IsNullOrEmpty(output))
                            {
                                sw.WriteLine(output);
                            }
                            else
                            {
                                if (File.Exists(BackupPath))
                                {
                                    textFile = new FileInfo(BackupPath);
                                    File.Delete(path);
                                    textFile.CopyTo(path);
                                }
                            }
                        }
                    }
                }
                catch //If there's a problem downloading 
                {
                    if (File.Exists(BackupPath))
                    {
                        textFile = new FileInfo(BackupPath);
                        File.Delete(path);
                        textFile.CopyTo(path);
                    }

                    continue;
                }
            }

        }

        /// <summary>
        /// Converts the WikipediaAPI result
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public async Task<Response> GetText(string urlBase, string controller, Country c)
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase)
                };

                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                //Splits the XML result string by paragraphs - </p> (closing paragraph tag)
                string[] parts = result.Split(new string[] { "&lt;/p&gt;" /*<p>*/}, StringSplitOptions.None);

                var output = string.Empty;

                if (parts[1].Contains(c.Name))
                {
                    output = parts[1]; //If the second paragraph contains the Country's name 
                }
                else
                {
                    output = parts[2];//If the third paragraph contains the Country's name 
                }

                //Clears any white-space (/s) character AND non-white-space character (/S) ---  @"(<[\s\S]+?>)"
                output = Regex.Replace(output, @"(&lt;[\s\S]+?&gt;)", string.Empty); //Remove Tags from the XML

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSucess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSucess = true,
                    Result = output
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucess = false,
                    Message = ex.Message
                };
            }
        }



        //Save Into DB

        public async Task SaveCountryData()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            await dataService.SaveDataCountry(ListOfCountries, progress);
            labelReport.Content = "Saving Countries Data into Database.";
        }

        public async Task SaveCountryRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            await dataService.SaveDataRates(ListOfApiRates, progress);
            labelReport.Content = "Saving Rates Data into Database.";

        }




        /// <summary>
        /// Access DB in case there is no Internet Connection
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private async Task LoadLocalDataCountries()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            ListOfCountries = await dataService.GetLocalDataCountry(); //Returns a Local Repository containing The various API's Data
            ListOfApiRates = await dataService.GetLocalDataRates();
        }




        //Events

        /// <summary>
        /// Countries 'Search Engine'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSearchCountry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (ListOfCountries != null)
                {
                    listBoxCountries.ItemsSource = null;

                    var aux = ListOfCountries.FindAll(x => x.Name.ToLower().Contains(TxtSearchCountry.Text.ToLower()));

                    listBoxCountries.ItemsSource = aux;


                    if (aux.Count == 0)
                    {
                        MessageBox.Show("Make sure your typing the country name correctly");
                        TxtSearchCountry.Text = string.Empty;
                        listBoxCountries.ItemsSource = ListOfCountries;
                    }
                }
            }
            catch
            {
                MessageBox.Show("An Error Ocurred. Try Again");
                ListBoxLanguages.ItemsSource = ListOfCountries;
            }

        }

        /// <summary>
        /// Updates the Tab Menu with the selected object of type country properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxLanguages.ItemsSource = ListOfCountries;
            Country selectedCountry = (Country)listBoxCountries.SelectedItem;

            if (selectedCountry != null)
            {
                try
                {
                    string Path = Environment.CurrentDirectory + "/CountriesTexts" + $"/{selectedCountry.Alpha2Code}.txt";
                  
                    var connection = networkService.CheckConnection();

                    if (selectedCountry != null)
                    {
                        List<Language> selectedCountryLanguage = (selectedCountry.Languages);
                        ListBoxLanguages.ItemsSource = selectedCountryLanguage;
                    }

                    TxtDescription.Text = File.ReadAllText(Path);

                    this.TxtBlockName.Text = selectedCountry.Name;
                    this.TxtBlockNativeName.Text = selectedCountry.NativeName;
                    this.TxtBlockCapital.Text = selectedCountry.Capital;
                    this.TxtBlockRegion.Text = selectedCountry.Region;
                    this.TxtBlockSubRegion.Text = selectedCountry.Subregion;
                    this.TxtBlockArea.Text = selectedCountry.Area.GetValueOrDefault().ToString() + " Km2";
                    this.TxtBlockGini.Text = selectedCountry.Gini.GetValueOrDefault().ToString();
                    this.TxtBlockPopulation.Text = selectedCountry.Population/*.ToString("#,#", CultureInfo.InvariantCulture)*/ + " Inhabitants";
                    this.LblDescription.Content = selectedCountry.Name + $"({selectedCountry.NativeName})";
                    this.txtRegion.Text = selectedCountry.Region;

                    CheckData(selectedCountry);

                    if (connection.IsSucess)
                    {
                        await LoadApiHoliday(selectedCountry.Alpha2Code);
                    }
                    else
                    {
                        countHolidays.Content = $"In order to list this country holidays, please check your Internet Connection.";
                    }

                    listBoxCountries.ItemsSource = ListOfCountries;

                }
                catch
                {
                    MessageBox.Show("Error");
                    ListBoxLanguages.ItemsSource = ListOfCountries;
                }

                try //FlagImage
                {

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();

                    if (File.Exists(Environment.CurrentDirectory + "/ImagesPNG" + $"/{selectedCountry.Name}.png"))
                    {
                        img.UriSource = new Uri(Environment.CurrentDirectory + "/ImagesPNG" + $"/{selectedCountry.Name}.png");
                    }
                    else if (!File.Exists(Environment.CurrentDirectory + "/ImagesPNG" + $"/{selectedCountry.Name}.png"))
                    {
                        ////img.UriSource = new Uri(Environment.CurrentDirectory + "/Resources/NoImageAvailable.png");
                        //FlagImage.Stretch = Stretch.fill;
                    }

                    img.EndInit();

                    FlagImage.Source = img;
                    FlagImage.Height = 130;
                    FlagImage.Width = 200;
                    BorderFlag.Width = FlagImage.Width;
                    BorderFlag.Height = FlagImage.Height;
                    FlagImage.Stretch = Stretch.Fill;

                }
                catch
                {
                    BitmapImage imgregion = new BitmapImage();
                    imgregion.BeginInit();
                    if (selectedCountry.Region == null)
                    {
                        //    imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Resources/NoImageAvailable.png");
                    }

                    MessageBox.Show("Failed When Presenting Selected Country Flag Image");
                }

                try // Region Image 
                {
                    string Region = selectedCountry.Region.ToString();
                    BitmapImage imgregion = new BitmapImage();
                    imgregion.BeginInit();

                    if (selectedCountry.Region == "Europe")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/europe.png");
                    }
                    if (selectedCountry.Region == "Americas")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/americas.png");
                    }
                    if (selectedCountry.Region == "Oceania")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/oceania.png");
                    }
                    if (selectedCountry.Region == "Africa")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/africa.png");
                    }
                    if (selectedCountry.Region == "Polar")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/antarctida.png");
                    }
                    if (selectedCountry.Region == "Asia")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/asia.png");
                    }

                    imgregion.EndInit();
                    RegionImage.Source = imgregion;
                }
                catch
                {
                    BitmapImage imgregion = new BitmapImage();
                    imgregion.BeginInit();
                    if (selectedCountry.Region == null)
                    {
                        //imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Resources/NoImageAvailable.png");
                    }

                    MessageBox.Show("Error while presenting Selected Country Regional Bloc Image");
                }

                try //Rates 
                {
                    List<Rates> listauxrate = new List<Rates>();

                    List<Currency> selectedCountryCurrency = (selectedCountry.Currencies);

                    ListBoxCountryCurrency.ItemsSource = selectedCountryCurrency;

                    if (ListOfApiRates.Count > 0)
                        foreach (Rates r in ListOfApiRates)  // meter try
                        {
                            foreach (Currency c in selectedCountryCurrency)
                            {
                                if (c.Code == r.Code)
                                {
                                    listauxrate.Add(r);
                                }
                                //else if (c.CurrencyName.ToLower() == r.Name.ToLower())
                                //{
                                //    listauxrate.Add(r);
                                //}
                            }
                        }

                    OriginValue.ItemsSource = listauxrate;
                    DestinationValue.ItemsSource = ListOfApiRates;

                    if (selectedCountryCurrency.Count == 1)
                    {
                        txtCountryCurrency.Text = $"{selectedCountry.Name} has {selectedCountryCurrency.Count} Currency";
                    }
                    if (selectedCountryCurrency.Count > 1)
                    {
                        txtCountryCurrency.Text = $"{selectedCountry.Name} has {selectedCountryCurrency.Count} Currencies";
                    }
                }
                catch
                {
                    MessageBox.Show("Error while loading Countries Rates Information");
                }
            }

            listBoxCountries.ItemsSource = ListOfCountries;
        }

        /// <summary>
        /// Display Info 
        /// </summary>
        private void CheckData(Country c)
        {
            if ((c.Capital == null) || (c.Capital == string.Empty))
            {
                this.TxtBlockCapital.Text = "(Unknown)";
            }
            if ((c.Population == null) || (c.Population == 0))
            {
                this.TxtBlockPopulation.Text = "(Unknown)";
            }
            if ((c.Area == null) || (c.Capital == string.Empty))
            {
                this.TxtBlockArea.Text = "(Unknown)";
            }
            if ((c.Region == null) || (c.Region == string.Empty))
            {
                this.TxtBlockPopulation.Text = "(Unknown)";
            }
            if ((c.Subregion == null) || (c.Subregion == string.Empty))
            {
                this.TxtBlockSubRegion.Text = "(Unknown)";
            }
            if ((c.Area == null) || (c.Area == 0))
            {
                this.TxtBlockArea.Text = "(Unknown)";
            }
            if ((c.Gini ==null) || (c.Gini == 0))
            {
                this.TxtBlockGini.Text = "(Unknown)";
            }
        }

        /// <summary>
        /// ProgressBarRepor Value Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarReport.Value = e.Percentagem;
        }



        //Buttons

        /// <summary>
        /// Exit Program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Opens Credits Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            CreditsForm form = new CreditsForm();
            form.Show();
        }



        //Rates

        /// <summary>
        /// Converts the typed value to a destiny currency
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            Convert();
        }

        /// <summary>
        /// Convert one currency into another
        /// </summary>
        private void Convert()
        {
            if (string.IsNullOrEmpty(txtValue.Text))
            {
                dialogService.ShowMessage("Error", "Type a Value to Convert");
                return;
            }

            decimal value;

            if (!decimal.TryParse(txtValue.Text, out value))
            {
                dialogService.ShowMessage("Error", "Typed Value Must be a Intenger");
                return;
            }

            if (OriginValue.SelectedItem == null)
            {
                dialogService.ShowMessage("Error", "Choose a Currency of Origin to Convert");
                return;
            }

            if (DestinationValue.SelectedItem == null)
            {
                dialogService.ShowMessage("Error", "Choose a Currency of Destiny to Convert");
                return;
            }

            var taxaOrigem = (Rates)OriginValue.SelectedItem;
            var taxaDestino = (Rates)DestinationValue.SelectedItem;
            var valorConvertido = value / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;
            TxtResult.Text = string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, value, taxaDestino.Code, valorConvertido);

        }



       
    }
}
