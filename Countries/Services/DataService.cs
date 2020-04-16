namespace Countries.Services
{
    using Countries.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;

    public class DataService
    {
        private SQLiteConnection connection;

        private SQLiteCommand command;

        private DialogService dialogService;

    //    public DataService() //Construtor que verifica que se existe a pasta com a Base de Dados
    //    {
    //        dialogService = new DialogService();

    //        if (!Directory.Exists("Data"))
    //        {
    //            Directory.CreateDirectory("Data"); //Se a pasta não existir, é criada
    //        }

    //        var path = @"Data\Countries.sqlite";

    //        //try
    //        //{
    //        //    connection = new SQLiteConnection("DataSource=" + path); //Caminho para fazer a conexação À Base de Dados
    //        //    connection.Open();
    //        //    //string sqlCommand = "Create table if not exists Countries(RateId int, Code varchar(5), TaxRate real, Name varchar(250))"; //Comando SQL que cria a Tabela
    //        //    //command = new SQLiteCommand(sqlCommand, connection);
    //        //    command.ExecuteNonQuery();
    //        //}
    //        //catch (Exception e)
    //        //{
    //        //    dialogService.ShowMessage("Error", e.Message);
    //        //}

    //    }
    //    public void SaveData(List<Countrie> countries)
    //    {
    //        try
    //        {
    //            foreach (var c in countries) //Para cada rate na lista de rates
    //            {
    //                //string sql = string.Format("insert into Rates (RateId,Code,TaxRate,Name) values({0}, '{1}', '{2}', '{3}')", c.RateId, rate.Code, rate.TaxRate, rate.Name);
    //                //command = new SQLiteCommand(sql, connection);
    //                command.ExecuteNonQuery();
    //            }
    //            connection.Close();
    //        }
    //        catch (Exception e)
    //        {
    //            dialogService.ShowMessage("Error", e.Message);
    //        }
    //    }
    //    public List<Countrie> GetData()
    //    {
    //        List<Countrie> Rates = new List<Countrie>();

    //        try
    //        {
    //            //string sql = "select RateId, Code, TaxRate, Name from Rates";
    //            //command = new SQLiteCommand(sql, connection);
    //            SQLiteDataReader reader = command.ExecuteReader(); //Lê cada registo
    //            while (reader.Read())
    //            {
    //                Rates.Add(new Countrie
    //                {
    //                    //RateId = (int)reader["RateId"],
    //                    //Code = (string)reader["Code"],
    //                    //Name = (string)reader["Name"],
    //                    //TaxRate = (double)reader["TaxRate"],
    //                });
    //            }

    //            connection.Close();
    //            return Rates;

    //        }
    //        catch (Exception e)

    //        {
    //            dialogService.ShowMessage("Error", e.Message);
    //            return null;
    //        }
    //    }
        
    }
}
