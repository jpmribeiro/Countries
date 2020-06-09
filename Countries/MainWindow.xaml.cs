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
    using System.Text.RegularExpressions;
    using System.Net.Http;
    using System.Linq;

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

                load = false;

            }
            else  //If there is Internet Connection
            {
                await LoadApiCountries();
                await LoadApiRates();
                await LoadApiWikipedia();
                await LoadApiHoliday();

                List<Country> listBoxList = ListOfCountries;
                listBoxCountries.ItemsSource = listBoxList;

                load = true;
            }

            if ((ListOfCountries == null) || (ListOfCountries.Count == 0))
            {
                labelReport.Content = "Please make sure you have a Internet Connection when you use the application for the first time. Try Again Later!";

                MessageBox.Show("Before using the App for the first time make sure there is a Internet Connection");

                return;
            }
           
            if (load)
            {

                await SaveCountryRates();
                await SaveCountryData();

                labelReport.Content = string.Format("(Data uploaded from the Internet, ({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }
            else
            {
                labelReport.Content = string.Format("(Data uploaded from local Database, ({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
                
                if ((ListOfCountries.Count > 0) || (ListOfCountries.Count < 250))
                {
                    MessageBox.Show("Your Database is incomplete. To prevent this, run your program while connected to the Internet. Try Again Later!");
                }

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
            labelReport.Content = "Downloading Countries Information.";

            await dataService.DeleteData();

            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all", progress);
            ListOfCountries = (List<Country>)response.Result;


            //Images 

            labelReport.Content = "Downloading Countries Flags Information.";

            if (!Directory.Exists("FlagsPNG")) //If directory doesn't exist
            {

                await dataService.SaveFlag(ListOfCountries, progress);

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

        }

        /// <summary>
        /// Calls holidays API
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private async Task LoadApiHoliday()
        {
            if(ListOfCountries!=null)
            {
                if (!Directory.Exists("HolidaysTexts"))
                {
                    Directory.CreateDirectory("HolidaysTexts");
                }

                try
                {
                    foreach (Country c in ListOfCountries)
                    {
                        string path = Environment.CurrentDirectory + "/HolidaysTexts" + $"/{c.Alpha3Code}.txt";
                        string BackupPath = Environment.CurrentDirectory + "/BackupHolidaysTexts" + $"/{c.Alpha3Code}.txt";

                        FileInfo textFile = new FileInfo(path);

                        if (!File.Exists(path))
                        {
                            int auxcount = 0;
                            labelReport.Content = "Updating Holidays Information.";

                            var response2 = await apiService.GetHolidays("https://holidayapi.com", $"/v1/holidays?pretty&key=f007d74d-60a8-448f-8974-b3805fea0463&country={c.Alpha2Code}&year=2019");
                            var ListOfCountryHolidays = (CountryHoliday)response2.Result;

                            List<Holiday> listaux = new List<Holiday>();

                            if (ListOfCountryHolidays != null)
                            {
                                foreach (var holiday in ListOfCountryHolidays.holidays)
                                {
                                    listaux.Add(holiday);
                                    auxcount++;
                                }

                                StreamWriter sw = File.CreateText(path);

                                foreach (Holiday holiday in listaux)
                                {
                                    sw.WriteLine(holiday.ToString());
                                }

                                sw.Close();
                                listaux.Clear();

                            }
                            else
                            {
                                countHolidays.Content = $"Unable to Show this Country Holidays due to the lack of Internet Connection.";
                                txtCountriesHolidays.Text = string.Empty;
                            }
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
                catch
                {
                    MessageBox.Show("Error While loading Countries Holidays Info");
                }
            }
           
        }

        /// <summary>
        /// Calls WikipediaAPI
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiWikipedia()
        {
            labelReport.Content = "Downloading Countries Texts Information.";

            if (!Directory.Exists("CountriesTexts"))
            {
                Directory.CreateDirectory("CountriesTexts");
            }

            foreach (var c in ListOfCountries)
            {
                string path = Environment.CurrentDirectory + "/CountriesTexts" + $"/{c.Alpha3Code}.txt";
                string BackupPath = Environment.CurrentDirectory + "/BackupCountriesTexts" + $"/{c.Alpha3Code}.txt";

                FileInfo textFile;

                try
                {
                    if (!File.Exists(path)) //If there isn't a txt file containing a text about the specified country
                    {
                        var response = await GetText("https://en.wikipedia.org/w/api.php", $"?format=xml&action=query&prop=extracts&titles={c.Name.Replace((' '), ('_')).Replace(("'"), (""))}&redirects=true", c);
                        string output = (string)response.Result;
                        
                        if (!string.IsNullOrEmpty(output) && output.Contains("\n\n"))
                            output = output.Replace("\n\n", "");
                        else if (!string.IsNullOrEmpty(output) && output.Contains("\n"))
                            output = output.Replace("\n", "");

                        if ((c.Alpha3Code == "COG") || (c.Alpha3Code == "GEO")) //Republic of Congo and Georgia Alpha Codes are ambiguous in wikipedia
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
                            StreamWriter sw = File.CreateText(path);

                            sw.WriteLine(output);
                            sw.Close();
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
                output = Regex.Replace(output, @"\t|\n|\r", string.Empty); //Remove Tabs or White Space

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
            labelReport.Content = "Saving Countries Information into Database.";

            await dataService.SaveDataCountry(ListOfCountries, progress);
        }

        public async Task SaveCountryRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            labelReport.Content = "Saving Rates Information into Database.";

            await dataService.SaveDataRates(ListOfApiRates, progress);

        }



        //Load Data locally

        /// <summary>
        /// Access DB in case there is no Internet Connection
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private async Task LoadLocalDataCountries()
        {
            await LoadLocalCountry();
            await LoadLocalRates();
        }

        private async Task LoadLocalCountry()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            labelReport.Content = "Loading Countries Data from Local Database";

            ListOfCountries = await dataService.GetLocalDataCountry(); //Returns a Local Repository containing The various API's Data

            List<Country> listBoxList = ListOfCountries;
            listBoxCountries.ItemsSource = listBoxList;
        }

        private async Task LoadLocalRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            labelReport.Content = "Loading Rates Data from Local Database";

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
        private void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country selectedCountry = (Country)listBoxCountries.SelectedItem;

            if (selectedCountry != null)
            {
                string pathCountryWiki = Environment.CurrentDirectory + @"/CountriesTexts" + $@"/{selectedCountry.Alpha3Code}.txt";
                string backupPathCountryWiki = Environment.CurrentDirectory + @"/BackupCountriesTexts" + $@"/{selectedCountry.Alpha3Code}.txt";
                string pathCountryHoliday = Environment.CurrentDirectory + @"/HolidaysTexts" + $@"/{selectedCountry.Alpha3Code}.txt";
                string backupPathCountryHoliday = Environment.CurrentDirectory + @"/BackupHolidaysTexts" + $@"/{selectedCountry.Alpha3Code}.txt";

                try
                {
                    if (selectedCountry != null)
                    {
                        List<Language> selectedCountryLanguage = (selectedCountry.Languages);
                        ListBoxLanguages.ItemsSource = selectedCountryLanguage;
                    }

                    TxtBlockName.Text = selectedCountry.Name;
                    TxtBlockNativeName.Text = selectedCountry.NativeName;
                    TxtBlockCapital.Text = selectedCountry.Capital;
                    TxtBlockRegion.Text = selectedCountry.Region;
                    TxtBlockSubRegion.Text = selectedCountry.Subregion;
                    TxtBlockArea.Text = selectedCountry.Area.GetValueOrDefault().ToString() + " Km2";
                    TxtBlockGini.Text = selectedCountry.Gini.GetValueOrDefault().ToString();
                    TxtBlockPopulation.Text = selectedCountry.Population/*.ToString("#,#", CultureInfo.InvariantCulture)*/ + " Inhabitants";
                    LblDescription.Content = selectedCountry.Name + $"({selectedCountry.NativeName})";
                    txtRegion.Text = selectedCountry.Region;

                    //CheckData(selectedCountry);

                    if (!File.Exists(pathCountryWiki))
                    {
                        TxtDescription.Text = File.ReadAllText(backupPathCountryWiki);
                    }
                    else
                    {
                        TxtDescription.Text = File.ReadAllText(pathCountryWiki);
                    }

                    if (!File.Exists(pathCountryHoliday))
                    {
                        txtCountriesHolidays.Text = File.ReadAllText(backupPathCountryHoliday);
                        countHolidays.Content = $"{File.ReadAllLines(backupPathCountryHoliday).Count()} Holidays:";
                    }
                    else
                    {
                        txtCountriesHolidays.Text = File.ReadAllText(pathCountryHoliday);
                        countHolidays.Content = $"{File.ReadAllLines(pathCountryHoliday).Count()} Holidays:";
                    }


                    listBoxCountries.ItemsSource = ListOfCountries;

                }
                catch                                                                                                                                                                         
                {
                    MessageBox.Show("Error presenting this Country Wiki Text");
                }

                try //FlagImage
                {

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();

                    if (File.Exists(Environment.CurrentDirectory + "/FlagsPNG" + $"/{selectedCountry.Name}.png"))
                    {
                        img.UriSource = new Uri(Environment.CurrentDirectory + "/FlagsPNG" + $"/{selectedCountry.Name}.png");
                    }
                    else if (!File.Exists(Environment.CurrentDirectory + "/FlagsPNG" + $"/{selectedCountry.Name}.png")) //If the image doesn't exist in the folder of flags
                    {
                        img.UriSource = new Uri(Environment.CurrentDirectory + "/BackupFlagsPNG" + $"/{selectedCountry.Name}.png");
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
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(Environment.CurrentDirectory + "/NoImageAvailable.png");
                    FlagImage.Source = img;
                    img.EndInit();
                }

                try // Region Image 
                {
                    string Region = selectedCountry.Region.ToString();
                    BitmapImage imgregion = new BitmapImage();
                    imgregion.BeginInit();

                    if (selectedCountry.Region == "Europe")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/europe.png");
                        RegionImage.Source = imgregion;
                    }
                    if (selectedCountry.Region == "Americas")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/americas.png");
                        RegionImage.Source = imgregion;
                    }
                    if (selectedCountry.Region == "Oceania")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/oceania.png");
                        RegionImage.Source = imgregion;
                    }
                    if (selectedCountry.Region == "Africa")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/africa.png");
                        RegionImage.Source = imgregion;
                    }
                    if (selectedCountry.Region == "Polar")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/antarctida.png");
                        RegionImage.Source = imgregion;
                    }
                    if (selectedCountry.Region == "Asia")
                    {
                        imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/Regions" + "/asia.png");
                        RegionImage.Source = imgregion;
                    }
                    

                    imgregion.EndInit();
                }
                catch
                {
                    BitmapImage imgregion = new BitmapImage();
                    imgregion.BeginInit();
                    imgregion.UriSource = new Uri(Environment.CurrentDirectory + "/NoImageAvailable.png");
                    RegionImage.Source = imgregion;
                    imgregion.EndInit();

                }

                try //Rates 
                {
                    OriginValue.ItemsSource = null;
                    DestinationValue.ItemsSource = null;

                    txtValue.Text = string.Empty;
                    TxtResult.Text = string.Empty;

                    List<Rates> listauxrate = new List<Rates>();
                    List<Currency> selectedCountryCurrency = (selectedCountry.Currencies);

                    ListBoxCountryCurrency.ItemsSource = selectedCountryCurrency;

                    if (ListOfApiRates.Count > 0)
                    {
                        foreach (Rates r in ListOfApiRates)
                        {
                            foreach (Currency c in selectedCountryCurrency)
                            {
                                if (c.Code == r.Code)
                                {
                                    listauxrate.Add(r);
                                }
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
            if (((string) labelReport.Content == "Loading Countries Data from Local Database") || ((string)labelReport.Content == "Loading Rates Data from Local Database") || ((string)labelReport.Content == "Downloading Countries Texts Information.") || ((string)labelReport.Content == "Updating Holidays Information."))     
            {
                
                MessageBoxResult result = MessageBox.Show("Your Program is still downloading Information. Are you sure you want to exit?",
                        "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    Environment.Exit(0);
                }
                
            }

            else if (((string)labelReport.Content == "Saving Countries Information into Database.") || ((string)labelReport.Content == "Saving Rates Information into Database."))
            {
                MessageBoxResult result = MessageBox.Show("Your Program is still saving Information. Are you sure you want to exit?",
                        "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    Environment.Exit(0);
                }
            }

            else
            {
                Environment.Exit(0);
            }
            
            

            


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
                txtValue.Text = string.Empty;
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
