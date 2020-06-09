namespace Countries.Services
{
    using Countries.Models;
    using ImageMagick;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class DataService
    {
        private SQLiteConnection connection;
        private SQLiteCommand commandCountry;
        private DialogService dialogService;


        /// <summary>
        /// Creates the DB Tables
        /// </summary>
        public DataService() 
        {
            dialogService = new DialogService();

            var pointCulture = new CultureInfo("en")
            {
                NumberFormat = { NumberDecimalSeparator = "." }
            };

            Thread.CurrentThread.CurrentCulture = pointCulture;

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = @"Data\Countries.sqlite";

            try
            {
                connection = new SQLiteConnection("DataSource=" + path);
                connection.Open();

                //Country
                string sqlCommand = "Create table if not exists Countries " +
                    "(Name varchar(200)," +
                    "Native_Name varchar(200)," +
                    "Alpha3Code char(3) PRIMARY KEY," +
                    "Capital varchar(200)," +
                    "Region varchar(200)," +
                    "SubRegion varchar(200)," +
                    "Area real," +
                    "Gini real," +
                    "Population int)";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //CountryLanguage
                sqlCommand = "create table if not exists CountryLanguage(" +
               "CodLanguage char(3)," +
               "Alpha3Code char(3)," +
               "PRIMARY KEY (Alpha3Code, CodLanguage)," +
               "FOREIGN KEY(CodLanguage) REFERENCES Language(Iso639_2)," +
               "FOREIGN KEY (Alpha3Code) REFERENCES Country(Alpha3Code))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();
                
                //Language
                sqlCommand = "Create table if not exists Languages " +
                    "(Iso639_1 char(3)," +
                    "Iso639_2 char(3) PRIMARY KEY," + 
                    "Name Varchar(100)," +
                    "NativeName Varchar(100))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //Holiday
                sqlCommand = "Create table if not exists Holidays " +
                    "(Name varchar(100)," +
                    "date varchar(100))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //CountryHoliday
                sqlCommand = "Create table if not exists CountryHoliday " +
                    "(Alpha3Code char(3), " +
                    "Name varchar(100)," +
                    "FOREIGN KEY(Alpha3Code) REFERENCES Country(Alpha3Code)," +
                    "FOREIGN KEY(Name) REFERENCES Holidays(Name))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //Currency

                sqlCommand = "Create table if not exists Currency " +
                    "(Code char(3)," +
                    "CurrencyName varchar(100)," +
                    "Symbol char(3))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //CountryCurrency
                sqlCommand = "Create table if not exists CountryCurrency " +
                   "(Alpha3Code char(3)," +
                   "CurrencyCode char(3)," +
                   "FOREIGN KEY(Alpha3Code) REFERENCES Country(Alpha3Code)," +
                   "FOREIGN KEY(CurrencyCode) REFERENCES Currency(Code))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //Rate
                sqlCommand = "Create table if not exists Rates " +
                  "(RateId Int," +
                  "Code varchar(3)," +
                  "TaxRate double," +
                  "Name Varchar(100))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Create DataService", e.Message);
            }

        }


        /// <summary>
        /// Saves the information from countries API in the DB
        /// </summary>
        /// <param name="Countries"></param>
        /// <returns></returns>
        public async Task SaveDataCountry(List<Country> Countries, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();

            try
            {
                List<string> ListOfLanguages = new List<string>();
                List<string> ListOfCurrencies = new List<string>();

                foreach (var c in Countries) 
                {
                    CheckSaveDataCountry(c);
                    
                    string sql = string.Format("insert into Countries(Name,Native_Name, Alpha3Code, Capital,Region,Subregion,Area,Gini,Population) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", c.Name, c.NativeName, c.Alpha3Code, c.Capital, c.Region, c.Subregion, c.Area, c.Gini, c.Population);
                    commandCountry = new SQLiteCommand(sql, connection);
                    await Task.Run (() => commandCountry.ExecuteNonQuery());

                    foreach(var language in c.Languages)
                    {
                        string sql3 = string.Format("insert into CountryLanguage(CodLanguage,Alpha3Code) values ('{0}', '{1}')", language.Iso639_2, c.Alpha3Code);
                        commandCountry = new SQLiteCommand(sql3, connection);
                        await Task.Run(() => commandCountry.ExecuteNonQuery());

                        if (!ListOfLanguages.Contains(language.Iso639_2))
                        {
                            CheckSaveDataCountryLanguage(language);

                            string sql2 = string.Format("insert into Languages(Iso639_1,Iso639_2,Name,NativeName) values ('{0}', '{1}', '{2}', '{3}')", language.Iso639_1, language.Iso639_2, language.Name, language.NativeName);
                            commandCountry = new SQLiteCommand(sql2, connection);
                            await Task.Run(() => commandCountry.ExecuteNonQuery());

                            ListOfLanguages.Add(language.Iso639_2);
                        }
                    }

                    foreach (var currency in c.Currencies)
                    {
                        string sql5 = string.Format("insert into CountryCurrency(Alpha3Code, CurrencyCode) values ('{0}', '{1}')", c.Alpha3Code, currency.Code);
                        commandCountry = new SQLiteCommand(sql5, connection);
                        await Task.Run(() => commandCountry.ExecuteNonQuery());

                        if (!ListOfCurrencies.Contains(currency.Code))
                        {
                            //CheckSaveDataCountryCurrency(currency);

                            string sql4 = string.Format("insert into Currency(Code,CurrencyName,Symbol) values ('{0}', '{1}', '{2}')", currency.Code, currency.CurrencyName, currency.Symbol);
                            commandCountry = new SQLiteCommand(sql4, connection);
                            await Task.Run(() => commandCountry.ExecuteNonQuery());
                            ListOfCurrencies.Add(currency.Code);
                        }

                    }

                    report.SaveCountries.Add(c);
                    report.Percentagem = (report.SaveCountries.Count * 100) / Countries.Count;
                    progress.Report(report);

                }

                connection.Close();

            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Save Countries Data", e.Message);
            }

        }

        /// <summary>
        /// Saves the information from rates API in the DB
        /// </summary>
        /// <param name="Rates"></param>
        /// <returns></returns>
        public async Task SaveDataRates(List<Rates> Rates, IProgress<ProgressReport> progress)
        {
            ProgressReport report3 = new ProgressReport();

            try
            {
                foreach (var rate in Rates)
                {
                    string sql = string.Format("insert into Rates (RateId,Code,TaxRate,Name) values('{0}', '{1}', '{2}', '{3}')", rate.RateId, rate.Code, rate.TaxRate, rate.Name);
                    commandCountry = new SQLiteCommand(sql, connection);
                    await Task.Run(() => commandCountry.ExecuteNonQuery());

                    report3.SaveRates.Add(rate);
                    report3.Percentagem = (report3.SaveRates.Count * 100) / Rates.Count;
                    progress.Report(report3);
                }

            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Save Rates Data", e.Message);
            }

        }

        /// <summary>
        /// Checks info before saving data in the Languages Table
        /// </summary>
        /// <param name="language"></param>
        private void CheckSaveDataCountryLanguage(Language language)
        {
            if (language.Name.Contains("'"))
            {
                language.Name = language.Name.Replace("'", " ");
            }
            if (language.NativeName.Contains("'"))
            {
                language.NativeName = language.NativeName.Replace("'", " ");
            }
        }

        /// <summary>
        /// Filters info before saving data in the Countries Table 
        /// </summary>
        /// <param name="c"></param>
        private void CheckSaveDataCountry(Country c)
        {
            if (c.Name.Contains("'"))
            {
                c.Name = c.Name.Replace("'", " ");
            }
            if (c.NativeName.Contains("'"))
            {
                c.NativeName = c.NativeName.Replace("'", " ");
            }
            if (c.Alpha3Code.Contains("'"))
            {
                c.Alpha3Code = c.Alpha3Code.Replace("'", " ");
            }
            if (c.Capital.Contains("'"))
            {
                c.Capital = c.Capital.Replace("'", " ");
            }
            if (c.Region.Contains("'"))
            {
                c.Region = c.Region.Replace("'", " ");
            }
            if (c.Subregion.Contains("'"))
            {
                c.Subregion = c.Subregion.Replace("'", " ");
            }
            if (c.Gini==null)
            {
                c.Gini = 0;
            }

        }

        /// <summary>
        /// Gets Table Country from DB
        /// </summary>
        /// <returns></returns>
        public async Task<List<Country>> GetLocalDataCountry()
        {
            List<Country> Countries = new List<Country>();

            try
            {
                string sqlCountries = "select Name, Native_Name, Alpha3Code, Capital, Region, Subregion, Area, Gini, Population from Countries";
                commandCountry = new SQLiteCommand(sqlCountries, connection);
                SQLiteDataReader readerCountries = commandCountry.ExecuteReader(); //Lê cada registo

                await Task.Run(() =>
                {
                    while (readerCountries.Read())
                    {
                        Countries.Add(new Country
                        {
                            Name = (string)readerCountries["Name"],
                            NativeName = (string)readerCountries["Native_Name"],
                            Alpha3Code = (string)readerCountries["Alpha3Code"],
                            Capital = (string)readerCountries["Capital"],
                            Region = (string)readerCountries["Region"],
                            Subregion = (string)readerCountries["Subregion"],
                            Area = (double)readerCountries["Area"],
                            Gini = (double)readerCountries["Gini"],
                            Population = (int)readerCountries["Population"],
                            Languages = new List<Language>(),
                            Currencies = new List<Currency>()
                        });
                    }
                });

                readerCountries.Close();

                foreach(Country c in Countries)
                {
                    commandCountry.Parameters.AddWithValue("@alpha3Code", c.Alpha3Code);
                    commandCountry.CommandText = "SELECT Iso639_1, Iso639_2, Name, NativeName FROM Languages INNER JOIN CountryLanguage ON Languages.Iso639_2 = CountryLanguage.CodLanguage WHERE Alpha3Code =@alpha3Code";
                    commandCountry.Connection = connection;

                    SQLiteDataReader readerLanguages = commandCountry.ExecuteReader();

                    await Task.Run(() =>
                    {
                        while (readerLanguages.Read())
                        {
                            c.Languages.Add(new Language
                            {
                                Iso639_1 = (string)readerLanguages["Iso639_1"],
                                Iso639_2 = (string)readerLanguages["Iso639_2"],
                                Name = (string)readerLanguages["Name"],
                                NativeName = (string)readerLanguages["NativeName"],
                            });
                        }
                    });

                    readerLanguages.Close();
                    
                    commandCountry.Parameters.AddWithValue("@alpha3Code", c.Alpha3Code);
                    commandCountry.CommandText = "SELECT Code,CurrencyName,Symbol FROM Currency INNER JOIN CountryCurrency ON Currency.Code=CountryCurrency.CurrencyCode WHERE Alpha3Code=@alpha3code";
                    commandCountry.Connection = connection;

                    SQLiteDataReader readerCurrencies= commandCountry.ExecuteReader();

                    await Task.Run(() =>
                    {
                        while (readerCurrencies.Read())
                        {
                            c.Currencies.Add(new Currency
                            {
                                Code = (string)readerCurrencies["Code"],
                                CurrencyName = (string)readerCurrencies["CurrencyName"],
                                Symbol = (string)readerCurrencies["Symbol"]
                            });
                        }
                    });

                    readerCurrencies.Close();

                }

                return Countries;

            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Read Data Base", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets Rates Table from DB
        /// </summary>
        /// <returns></returns>
        public async Task<List<Rates>> GetLocalDataRates()
        { 
            List<Rates> Rates = new List<Rates>();

            try
            {
                string sql = "select RateId, Code, TaxRate, Name from Rates";
                commandCountry = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = commandCountry.ExecuteReader(); //Lê cada registo

                while (await Task.Run(() => reader.Read()))
                {
                    await Task.Run(() => Rates.Add(new Rates
                    {
                        RateId = (int)reader["RateId"],
                        Code = (string)reader["Code"],
                        Name = (string)reader["Name"],
                        TaxRate = (double)reader["TaxRate"],
                    }));
                }

                connection.Close();
                return Rates;

            }
            catch (Exception e)

            {
                dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }

        /// <summary>
        /// Drops every Table in the DB
        /// </summary>
        public async Task DeleteData()
        {
            await Task.Run(() =>
            {
                try
                {
                    string sql1 = "delete from Countries";
                    commandCountry = new SQLiteCommand(sql1, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql2 = "delete from CountryLanguage";
                    commandCountry = new SQLiteCommand(sql2, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql3 = "delete from Languages";
                    commandCountry = new SQLiteCommand(sql3, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql4 = "delete from CountryHoliday";
                    commandCountry = new SQLiteCommand(sql4, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql5 = "delete from Holidays";
                    commandCountry = new SQLiteCommand(sql5, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql6 = "delete from Holidays";
                    commandCountry = new SQLiteCommand(sql6, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql7 = "delete from Currency";
                    commandCountry = new SQLiteCommand(sql7, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql8 = "delete from CountryCurrency";
                    commandCountry = new SQLiteCommand(sql8, connection);
                    commandCountry.ExecuteNonQuery();

                    string sql9 = "delete from Rates";
                    commandCountry = new SQLiteCommand(sql9, connection);
                    commandCountry.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    dialogService.ShowMessage("Unable to delete Local Repository", e.Message);
                }
            });
        }

        /// <summary>
        /// Saves Flags images in a Folder inside the Project
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filename"></param>
        public async Task SaveFlag(List<Country> ListCountry, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();

            foreach (var item in ListCountry)
            {
                if (!File.Exists(Environment.CurrentDirectory + "/FlagsPNG" + $"/{item.Name}.png"))
                {
                    await Task.Run(() =>
                    {
                        if (!Directory.Exists("Images"))
                        {
                            Directory.CreateDirectory("Images");
                        }

                        try
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                webClient.DownloadFile(item.Flag, @"Images\" + item.Name + ".svg");
                            }
                        }
                        catch (Exception e)
                        {
                            dialogService.ShowMessage("Error While Creating SVG Images Folder", e.Message);
                        }

                        if (!Directory.Exists("FlagsPNG"))
                        {
                            Directory.CreateDirectory("FlagsPNG");
                        }

                        try
                        {
                            using (MagickImage image = new MagickImage(@"Images\" + item.Name + ".svg"))
                            {
                                image.Write(@"FlagsPNG\" + item.Name + ".png");
                            }
                        }
                        catch (Exception e)
                        {
                            dialogService.ShowMessage("Error While Creating PNG Images Folder", e.Message);
                        }

                        report.SaveCountries.Add(item);
                        report.Percentagem = (report.SaveCountries.Count * 100) / ListCountry.Count;
                        progress.Report(report);

                    });
                }
            }

        }

    }
}
