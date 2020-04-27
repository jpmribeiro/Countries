namespace Countries.Services
{
    using Countries.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class DataService
    {
        private SQLiteConnection connection;

        private SQLiteCommand commandCountry;

        private DialogService dialogService;

        public DataService() //Construtor que verifica que se existe a pasta com a Base de Dados
        {
            dialogService = new DialogService();

            var pointCulture = new CultureInfo("en")
            {
                NumberFormat = { NumberDecimalSeparator = "." }
            };

            Thread.CurrentThread.CurrentCulture = pointCulture;

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data"); //Se a pasta não existir, é criada
            }

            var path = @"Data\Countries.sqlite";

            try
            {
                connection = new SQLiteConnection("DataSource=" + path); //Caminho para fazer a conexação À Base de Dados
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
               "Iso639_2 char(3)," +
               "Alpha3Code char(3)," +
               "PRIMARY KEY (Alpha3Code, Iso639_2))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();
                
                //Language
                sqlCommand = "Create table if not exists Languages " +
                    "(Iso639_1 char(3)," +
                    "Iso639_2 char(3) PRIMARY KEY," + 
                    "LanguageName string," +
                    "LanguageNativeName string, " +
                    "FOREIGN KEY (Iso639_2) REFERENCES CountryLanguage(Iso639_2))";

                commandCountry = new SQLiteCommand(sqlCommand, connection);
                commandCountry.ExecuteNonQuery();

                //Holiday
                //sqlCommand = "Create table if not exists Holidays " +
                //    "(name string," +
                //    "date string");

                //commandCountry = new SQLiteCommand(sqlCommand, connection);
                //commandCountry.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Create DataService", e.Message);
            }

        }
        public async Task SaveDataCountry(List<Country> Countries)
        {
            try
            {
                List<string> ListOfLanguages = new List<string>();

                foreach (var c in Countries) //Foreach Country in the List ListOfCountries
                {
                    CheckSaveDataCountry(c);
                    
                    string sql = string.Format("insert into Countries(Name,Native_Name, Alpha3Code, Capital,Region,Subregion,Area,Gini,Population) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", c.Name, c.NativeName, c.Alpha3Code, c.Capital, c.Region, c.Subregion, c.Area, c.Gini, c.Population);
                    commandCountry = new SQLiteCommand(sql, connection);
                    await Task.Run (() => commandCountry.ExecuteNonQuery());

                    foreach(var language in c.Languages)
                    {
                        if (!ListOfLanguages.Contains(language.Iso639_2))
                        {
                            CheckSaveDataCountryLanguage(language);

                            string sql2 = string.Format("insert into Languages(Iso639_1,Iso639_2,LanguageName,LanguageNativeName) values ('{0}', '{1}', '{2}', '{3}')", language.Iso639_1, language.Iso639_2, language.Name, language.NativeName);
                            commandCountry = new SQLiteCommand(sql2, connection);
                            await Task.Run(() => commandCountry.ExecuteNonQuery());

                            string sql3 = string.Format("insert into CountryLanguage(Iso639_2,Alpha3Code) values ('{0}', '{1}')", language.Iso639_2, c.Alpha3Code);
                            commandCountry = new SQLiteCommand(sql3, connection);
                            await Task.Run(() => commandCountry.ExecuteNonQuery());

                            ListOfLanguages.Add(language.Iso639_2);
                        }
                    }
                }

                connection.Close();
                
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Unable to Save Countries Data", e.Message);
            }
        }

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


        public List<Country> GetLocalDataCountry()
        {
            List<Country> Countries = new List<Country>();

            try
            {
                string sql = "select Name, Native_Name, Alpha3Code, Capital, Region, Subregion, Area, Gini, Population from Countries";
                commandCountry = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = commandCountry.ExecuteReader(); //Lê cada registo

                while (reader.Read())
                {
                    Countries.Add(new Country
                    {
                        Name = (string)reader["Name"],
                        NativeName = (string)reader["Native_Name"],
                        Alpha3Code = (string)reader["Alpha3Code"],
                        Capital = (string)reader["Capital"],
                        Region = (string)reader["Region"],
                        Subregion = (string)reader["Subregion"],
                        Area = (double)reader["Area"],
                        Gini = (double)reader["Gini"],
                        Population = (int)reader["Population"],
                    });
                }

                connection.Close();
                return Countries;

            }
            catch (Exception e)

            {
                dialogService.ShowMessage("Unable to Read ", e.Message);
                return null;
            }
        }

    }
}
