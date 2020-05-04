//namespace Countries
//{
//    using Services;
//    using Models;
//    using System;
//    using System.Collections.Generic;
//    using System.Text;
//    using System.Threading.Tasks;
//    using System.Windows;
//    using System.Windows.Controls;
//    using System.Windows.Data;
//    using System.Windows.Documents;
//    using System.Windows.Input;
//    using System.Windows.Media;
//    using System.Windows.Media.Imaging;
//    using System.Windows.Navigation;
//    using System.Windows.Shapes;
//    using System.IO;
//    using Svg;
//    using System.Globalization;
//    using System.Net;
//    using System.Drawing.Imaging;

//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        private List<Country> ListOfCountries;
//        private readonly ApiService apiService;
//        private readonly NetworkService networkService;
//        private readonly DialogService dialogService;
//        //private readonly DataService dataService;

//        public MainWindow()
//        {
//            InitializeComponent();
//            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

//            networkService = new NetworkService();
//            apiService = new ApiService();
//            dialogService = new DialogService();
//            //dataService = new DataService();
//            LoadCountries();
//        }

//        private async void LoadCountries()//Tests Internet Connection
//        {
//            bool load;

//            labelReport.Content = "Updating Countries Data...";

//            var connection = networkService.CheckConnection();

//            if (!connection.IsSucess) //If no Internet Connection is Available
//            {
//                //LoadLocalCountries();
//                load = false;
//            }
//            else
//            {
//                await LoadApiCountries(); //If there is Internet Connection
//                GetCountriesFlags(ListOfCountries);
//                load = true;
//            }

//            if (ListOfCountries.Count == 0)
//            {
//                labelReport.Content = "There Is no Internet Connection at the moment" + Environment.NewLine +
//                   "And there is not a local repository" + Environment.NewLine +
//                   "Try Again Later!";

//                MessageBox.Show("Before using the App for the first time make sure there is a Internet Connection");

//                return;
//            }

//            List<Country> listBoxList = ListOfCountries;

//            listBoxCountries.ItemsSource = listBoxList;
//            //this.listBoxCountries.DataContext = ListOfCountries;

//            if (load)
//            {
//                labelReport.Content = string.Format("Data uploaded from the Internet in {0:F} .", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
//            }

//            else
//            {
//                labelReport.Content = string.Format("Data uploaded from a local repository.");
//            }

//            ProgressBarReport.Value = 100;

//        }

//        private async Task LoadApiCountries()
//        {
//            ProgressBarReport.Value = 0;
//            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");
//            ListOfCountries = (List<Country>)response.Result;
//            ////dataService.DeleteData(); 
//            //dataService.SaveData(ListOfCountries);
//        }

//        private void ProgressBarReport_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
//        {

//        }

//        private void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            Country selectedCountry = (Country)listBoxCountries.SelectedItem;

//            if (selectedCountry!=null)
//            {
//                this.TxtBlockName.Text = selectedCountry.Name;
//                this.TxtBlockNativeName.Text = selectedCountry.NativeName;
//                this.TxtBlockCapital.Text = selectedCountry.Capital;
//                this.TxtBlockRegion.Text = selectedCountry.Region;
//                this.TxtBlockSubRegion.Text = selectedCountry.Subregion;
//                this.TxtBlockArea.Text = selectedCountry.Area.ToString() + " Km2";
//                this.TxtBlockLanguages.Text = selectedCountry.Languages.ToString();
//                this.TxtBlockPopulation.Text = selectedCountry.Population.ToString("#,#", CultureInfo.InvariantCulture) + " Inhabitants";

//                string flagName = selectedCountry.Flag.Split('/')[4].Split('.')[0];

//                BitmapImage img = new BitmapImage();
//                img.BeginInit();

//                if (File.Exists(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg"))
//                {
//                    img.UriSource = new Uri(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg");
//                }
//                else
//                {
//                    img.UriSource = new Uri(Environment.CurrentDirectory + "/NoImageAvailable.jpg");
//                    FlagImage.Stretch = Stretch.None;
//                }

//                img.EndInit();
//                FlagImage.Source = img;
//                FlagImage.Stretch = Stretch.Fill;

//                listBoxCountries.ItemsSource = ListOfCountries;

//            }

//            if(this.TxtBlockCapital.Text == string.Empty)
//            {
//                this.TxtBlockCapital.Text = "(Unknown)";
//            }
//            if (this.TxtBlockPopulation.Text == string.Empty)
//            {
//                this.TxtBlockPopulation.Text = "(Unknown)";
//            }
//            if (this.TxtBlockArea.Text == string.Empty)
//            {
//                this.TxtBlockArea.Text = "(Unknown)";
//            }
//            if (this.TxtBlockSubRegion.Text == string.Empty)
//            {
//                this.TxtBlockSubRegion.Text = "(Unknown)";
//            }

//        }

//        private void TxtSearchCountry_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            if (ListOfCountries!=null)
//            {
//                var aux = ListOfCountries.FindAll(x => x.Name.ToLower().Contains(TxtSearchCountry.Text.ToLower()));

//                listBoxCountries.ItemsSource = aux;

//                if(aux.Count == 0)
//                {
//                    MessageBox.Show("Make sure your typing the country name correctly");
//                    TxtSearchCountry.Text = string.Empty;
//                }
//            }

//        }

//        private void GetCountriesFlags(List<Country> ListOfCountries )
//        {

//            if(!Directory.Exists("Flags"))
//            {
//                Directory.CreateDirectory("Flags");
//            }

//            foreach(Country country in ListOfCountries)
//            {
//                try
//                {
//                    string flagName = country.Flag.Split('/')[4].Split('.')[0]; ;
//                    var path = @"Flags\" + $"{flagName}.svg";

//                    string svgFile = "http://restcountries.eu" + $"/data/{flagName}.svg";

//                    using(WebClient webClient = new WebClient())
//                    {
//                        webClient.DownloadFile(svgFile, path);
//                    }

//                    string flag = flagName;
//                    var pathFlag = @"Flags\" + $"{flagName}.jpg"; //Save the Image as a jpg file

//                    var svgDoc = SvgDocument.Open(path);
//                    var bitmap = svgDoc.Draw(100,100);

//                    if (File.Exists(path))
//                    {
//                        File.Delete(path);
//                    }

//                    if (!File.Exists(pathFlag))
//                    {
//                        bitmap.Save(pathFlag, ImageFormat.Jpeg);
//                    }

//                }
//                catch
//                {
//                    continue;
//                }
//            }
//        }


//        //private void LoadLocalCountries()
//        //{
//        //    ListOfCountries = dataService.GetData(); //Retorna uma Lista
//        //}



//    }
//}
namespace Countries
{
    using Services;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.IO;
    using Svg;
    using System.Globalization;
    using System.Net;
    using System.Drawing.Imaging;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> ListOfCountries;
        //private List<CountryHoliday> ListOfCountryHolidays;
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
        }

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
                labelReport.Content = string.Format("Data uploaded from the Internet in {0:F} .", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }

            else
            {
                labelReport.Content = string.Format("Data uploaded from a local repository.");
            }


        }

        private async Task LoadApiCountries()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all", progress);
            ListOfCountries = (List<Country>)response.Result;
            ////dataService.DeleteData();
            //await dataService.SaveDataCountry(ListOfCountries);
            
        }

        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarReport.Value = e.Percentagem;
        }

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

        private void ProgressBarReport_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

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

            if (selectedCountry != null)
            {
                this.TxtBlockName.Text = selectedCountry.Name;
                this.TxtBlockNativeName.Text = selectedCountry.NativeName;
                this.TxtBlockCapital.Text = selectedCountry.Capital;
                this.TxtBlockRegion.Text = selectedCountry.Region;
                this.TxtBlockSubRegion.Text = selectedCountry.Subregion;
                this.TxtBlockArea.Text = selectedCountry.Area.GetValueOrDefault().ToString() + " Km2";
                this.TxtBlockGini.Text = selectedCountry.Gini.GetValueOrDefault().ToString();
                this.TxtBlockPopulation.Text = selectedCountry.Population.ToString("#,#", CultureInfo.InvariantCulture) + " Inhabitants";

                string flagName = selectedCountry.Alpha3Code;

                await LoadHoliday(selectedCountry.Alpha2Code);

                try
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();

                    if (File.Exists(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg"))
                    {
                        img.UriSource = new Uri(Environment.CurrentDirectory + "/Flags" + $"/{flagName}.jpg");
                    }
                    else
                    {
                        img.UriSource = new Uri(Environment.CurrentDirectory + "/ImageNotAvailable.jpg");
                        FlagImage.Stretch = Stretch.None;
                    }

                    img.EndInit();
                    FlagImage.Source = img;
                    FlagImage.Stretch = Stretch.Fill;

                }
                catch
                {

                }
                

                listBoxCountries.ItemsSource = ListOfCountries;

            }

            CheckData();

        }

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

        private void LoadLocalCountries()
        {
            ListOfCountries = dataService.GetLocalDataCountry(); //Returns a Local Repository containing The various API's Data
            //dataService.DeleteData();
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            var elapsped = stopwatch.Elapsed;
            MessageBox.Show(elapsped.ToString());
        }

    }
}
