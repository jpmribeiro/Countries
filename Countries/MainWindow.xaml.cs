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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Countrie> ListOfCountries;
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        private readonly DialogService dialogService;
        //private readonly DataService dataService;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            //dataService = new DataService();

            LoadCountries();
        }

        private async void LoadCountries()
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
            }

            if (ListOfCountries.Count == 0)
            {
                labelReport.Content = "There Is no Internet Connection at the moment" + Environment.NewLine +
                   "And there is not a local repository" + Environment.NewLine +
                   "Try Again Later!";

                MessageBox.Show("Before using the App for the first time make sure there is a Internet Connection");

                return;
            }

            listBoxCountries.ItemsSource = ListOfCountries;
            this.listBoxCountries.DataContext = ListOfCountries;
            
            if (load)
            {
                labelReport.Content = string.Format("Data uploaded from the Internet in {0:F} .", DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-EN")));
            }
            
            else
            {
                labelReport.Content = string.Format("Data uploaded from a local repository.");
            }

            ProgressBarReport.Value = 100;

        }
        private async Task LoadApiCountries()
        {
            ProgressBarReport.Value = 0;
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");
            ListOfCountries = (List<Countrie>)response.Result;

            ////dataService.DeleteData(); 
            //dataService.SaveData(ListOfCountries);
        }

        private void ProgressBarReport_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private void LoadLocalCountries()
        //{
        //    ListOfCountries = dataService.GetData(); //Retorna uma Lista
        //}



    }
}
