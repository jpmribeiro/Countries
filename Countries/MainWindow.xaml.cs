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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> ListOfCountries;
        //private List<CountryHoliday> ListOfCountryHolidays;
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
            LoadCountries();

            //WebClient wc = new WebClient();
            //string webData = wc.DownloadString("https://en.wikipedia.org/wiki/Portugal");
            //TxtDescription.Text = StripHTML(webData, true);
        }

        /// <summary>
        /// Removes HTML labels
        /// </summary>
        /// <param name="HTMLText"></param>
        /// <param name="decode"></param>
        /// <returns></returns>
        public static string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }

        /// <summary>
        /// Determines if the Program loads information from the API or the DB
        /// </summary>
        private async void LoadCountries()//Tests Internet Connection
        {
            bool load;

            labelReport.Content = "Updating Countries Data...";

            var connection = networkService.CheckConnection();

            if (!connection.IsSucess) //If no Internet Connection is Available
            {
                //LoadLocalCountries();

                load = false;

            }
            else
            {
                await LoadApiRates();
                await LoadApiCountries(); //If there is Internet Connection
                load = true;

                if (!Directory.Exists("Flags")) //If directory doesn't exist
                {
                    GetCountriesFlags(ListOfCountries);
                }

            }

            if (ListOfCountries.Count == 0)
            {
                labelReport.Content = "There Is no Internet Connection at the moment" + Environment.NewLine +
                   "And there is not a local repository" + Environment.NewLine +
                   "Try Again Later!";

                MessageBox.Show("Before using the App for the first time make sure there is a Internet Connection");

                return;
            }

            List<Country> listBoxList = ListOfCountries;

            listBoxCountries.ItemsSource = listBoxList;
            //this.listBoxCountries.DataContext = ListOfCountries;

            if (load)
            {
                labelReport.Content = string.Format("(Data uploaded from the Internet, ({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }

            else
            {
                labelReport.Content = string.Format("(Data uploaded from a local repository({0:F})).", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
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
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all", progress);
            ListOfCountries = (List<Country>)response.Result;
            ////dataService.DeleteData();
            await dataService.SaveDataCountry(ListOfCountries);

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
            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/Rates");
            ListOfApiRates = (List<Rates>)response.Result;

            //dataService.DeleteData(); 
            //dataService.SaveData(Rates); 
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

        /// <summary>
        /// Calls holidaysapi
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private async Task LoadHoliday(string c) //FAZER COM QUE SO ENTRE AQUI SE HOUVER LIGACAO A NET
        {
            int auxcount = 0;

            var response2 = await apiService.GetHolidays("https://holidayapi.com", $"/v1/holidays?pretty&key=dd888656-3e3a-4e83-a1e9-85823f145c04&country={c}&year=2019");
            var ListOfCountryHolidays = (CountryHoliday)response2.Result;

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
                Country C = new Country();
                C.holidays = listaux;
            }
            else
            {
                countHolidays.Content = $"There is no Information Available regarding this Country's Holidays";
                listBoxCountriesHolidays.ItemsSource = null;
            }

        }

        /// <summary>
        /// Updates the Tab Menu with the selected object of type country properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var connection = networkService.CheckConnection();

            ListBoxLanguages.ItemsSource = null;

            Country selectedCountry = (Country)listBoxCountries.SelectedItem;

            if (selectedCountry != null)
            {
                List<Language> selectedCountryLanguage = (selectedCountry.Languages);
                ListBoxLanguages.ItemsSource = selectedCountryLanguage;
            }

            this.TxtBlockName.Text = selectedCountry.Name;
            this.TxtBlockNativeName.Text = selectedCountry.NativeName;
            this.TxtBlockCapital.Text = selectedCountry.Capital;
            this.TxtBlockRegion.Text = selectedCountry.Region;
            this.TxtBlockSubRegion.Text = selectedCountry.Subregion;
            this.TxtBlockArea.Text = selectedCountry.Area.GetValueOrDefault().ToString() + " Km2";
            this.TxtBlockGini.Text = selectedCountry.Gini.GetValueOrDefault().ToString();
            this.TxtBlockPopulation.Text = selectedCountry.Population.ToString("#,#", CultureInfo.InvariantCulture) + " Inhabitants";

            CheckData();

            string flagName = selectedCountry.Alpha3Code;
            await LoadHoliday(selectedCountry.Alpha2Code);

            try
            {

                //FlagImage
                BitmapImage img = new BitmapImage();
                img.BeginInit();

                if (File.Exists(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg"))
                {
                    img.UriSource = new Uri(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg");
                }
                else if (!File.Exists(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg"))
                {
                    img.UriSource = new Uri(Environment.CurrentDirectory + "/Resources/NoImageAvailable.jpg");
                    //FlagImage.Stretch = Stretch.None;
                }

                //RegionImage:

                img.EndInit();
                FlagImage.Source = img;
                FlagImage.Stretch = Stretch.Fill;

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

                listBoxCountries.ItemsSource = ListOfCountries;

            }
            catch
            {
                MessageBox.Show("Failed When Presenting Selected Country Flag and Regional Bloc Location Image");
            }

            int auxcountrates = 0;

            List<Rates> listauxrate = new List<Rates>();

            List<Currency> selectedCountryCurrency = (selectedCountry.Currencies);

            ListBoxCountryCurrency.ItemsSource = selectedCountryCurrency;

            foreach (Rates r in ListOfApiRates)
            {
                foreach (Currency c in selectedCountryCurrency)
                {
                    if (c.Code == r.Code)
                    {
                        listauxrate.Add(r);
                        auxcountrates += 1;
                    }
                }
            }

            OriginValue.ItemsSource = listauxrate;
            DestinationValue.ItemsSource = ListOfApiRates;

            if (auxcountrates == 1)
            {
                txtCountryCurrency.Text = $"{selectedCountry.Name} has {auxcountrates} Currency";
            }
            if (auxcountrates > 1)
            {
                txtCountryCurrency.Text = $"{selectedCountry.Name} has {auxcountrates} Currencies";
            }

        }

        /// <summary>
        /// Display Info Conventions
        /// </summary>
        private void CheckData()
        {
            if (this.TxtBlockCapital.Text == string.Empty)
            {
                this.TxtBlockCapital.Text = "(Unknown)";
            }
            if (this.TxtBlockPopulation.Text == string.Empty)
            {
                this.TxtBlockPopulation.Text = "(Unknown)";
            }
            if (this.TxtBlockArea.Text == string.Empty)
            {
                this.TxtBlockArea.Text = "(Unknown)";
            }
            if (this.TxtBlockSubRegion.Text == string.Empty)
            {
                this.TxtBlockSubRegion.Text = "(Unknown)";
            }
            if ((this.TxtBlockArea.Text == " Km2") || (this.TxtBlockArea.Text == "0"))
            {
                this.TxtBlockArea.Text = "(Unknown)";
            }
            if ((this.TxtBlockGini.Text == string.Empty) || (this.TxtBlockGini.Text == "0"))
            {
                this.TxtBlockGini.Text = "(Unknown)";
            }
        }

        /// <summary>
        /// Countries 'Search Engine'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSearchCountry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListOfCountries != null)
            {
                var aux = ListOfCountries.FindAll(x => x.Name.ToLower().Contains(TxtSearchCountry.Text.ToLower()));

                listBoxCountries.ItemsSource = aux;

                if (aux.Count == 0)
                {
                    MessageBox.Show("Make sure your typing the country name correctly");
                    TxtSearchCountry.Text = string.Empty;
                }
            }

        }

        /// <summary>
        /// Creates a folder for countries flags images
        /// </summary>
        /// <param name="ListOfCountries"></param>
        private void GetCountriesFlags(List<Country> ListOfCountries)
        {
            if (!Directory.Exists("Flags"))
            {
                Directory.CreateDirectory("Flags");

            }

            foreach (Country country in ListOfCountries)
            {
                try
                {
                    string flagName = country.Flag.Split('/')[4].Split('.')[0]; ;
                    var path = @"Flags\" + $"{flagName}.svg";

                    string svgFile = "http://restcountries.eu" + $"/data/{flagName}.svg";

                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFile(svgFile, path);
                    }

                    string flag = flagName;
                    var pathFlag = @"Flags\" + $"{flagName}.jpg"; //Save the Image as a jpg file

                    var svgDoc = SvgDocument.Open(path);
                    var bitmap = svgDoc.Draw(100, 100);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    if (!File.Exists(pathFlag))
                    {
                        bitmap.Save(pathFlag, ImageFormat.Jpeg);
                    }

                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Access DB in case there is no Internet Connection
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private void LoadLocalCountries()
        {
            /*ListOfCountries = dataService.GetLocalDataCountry();*/ //Returns a Local Repository containing The various API's Data
            //dataService.DeleteData();
            //var stopwatch = Stopwatch.StartNew();
            //stopwatch.Stop();
            //var elapsped = stopwatch.Elapsed;
            //MessageBox.Show(elapsped.ToString());
        }

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
        /// Opens Credits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            About form = new About();
            form.Show();
        }

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
        /// Converts a Int Value from a given Currency to another Currency
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
