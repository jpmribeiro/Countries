//namespace Countries.Services
//{
//    using Countries.Models;
//    using Newtonsoft.Json;
//    using System;
//    using System.Collections.Generic;
//    using System.Net.Http;
//    using System.Threading.Tasks;

//    public class ApiService
//    {
//        public async Task <Response> GetCountries(string urlBase, string controller) 
//        {
//            try
//            {
//                var client = new HttpClient();
//                client.BaseAddress = new Uri(urlBase); //API Adress
//                var response = await client.GetAsync(controller); //API Controller
//                var result = await response.Content.ReadAsStringAsync();//Uploads the results in the form of String into (object) Result

//                if (!response.IsSuccessStatusCode)
//                {
//                    return new Response
//                    {
//                        IsSucess = false,
//                        Message = result
//                    };
//                }

//                var Countries = JsonConvert.DeserializeObject<List<Country>>(result);//Moves the results (JSON) to a list

//                return new Response
//                {
//                    IsSucess = true,
//                    Result = Countries,
//                };

//            }
//            catch(Exception ex)
//            {
//                return new Response
//                {
//                    IsSucess = false,
//                    Message = ex.Message,
//                };
//            }
//        }
//    }
//}
namespace Countries.Services
{
    using Countries.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ApiService
    {
        /// <summary>
        /// Gets Data from restcountries.eu API
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Response> GetCountries(string urlBase, string controller, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase); //API Adress
                var response = await client.GetAsync(controller); //API Controller
                var result = await response.Content.ReadAsStringAsync();//Uploads the results in the form of String into (object) Result

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSucess = false,
                        Message = result
                    };
                }

                var Countries = JsonConvert.DeserializeObject<List<Country>>(result);//Moves the results (JSON) to a list

                report.SaveCountries = Countries;
                report.Percentagem = (report.SaveCountries.Count * 100) / Countries.Count;
                progress.Report(report);

                return new Response
                {
                    IsSucess = true,
                    Result = Countries,
                };

            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucess = false,
                    Message = ex.Message,
                };
            }
        }

        /// <summary>
        /// Gets Data From holidayapi.com API
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public async Task<Response> GetHolidays(string urlBase, string controller)
        {
            
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase); //API Adress
                var response = await client.GetAsync(controller); //API Controller
                var result2 = await response.Content.ReadAsStringAsync();//Uploads the results in the form of String into (object) Result

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSucess = false,
                        Message = result2
                    };
                }

                var Holidays = JsonConvert.DeserializeObject<CountryHoliday>(result2);//Moves the results (JSON) to a list

                return new Response
                {
                    IsSucess = true,
                    Result = Holidays,
                };

            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucess = false,
                    Message = ex.Message,
                };
            }
        }

        //public async Task<Response> GetRates(string urlBase, string controller) //Vai Buscar as taxas
        //{
        //    try
        //    {
        //        var client = new HttpClient();//Fazer a Ligação Http 
        //        client.BaseAddress = new Uri(urlBase); //Endereço onde se encontra a API
        //        var response = await client.GetAsync(controller); //Controlador da API
        //        var result = await response.Content.ReadAsStringAsync();//Carrega os resultados sob a forma de string para o objecto result

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new Response
        //            {
        //                IsSucess = false,
        //                Message = result
        //            };
        //        }

        //        var rates = JsonConvert.DeserializeObject<List<Rate>>(result);//Passa os resultados (JSON) para uma lista que guarda dados do tipo Rate

        //        return new Response
        //        {
        //            IsSucess = true,
        //            Result = rates,

        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        return new Response
        //        {
        //            IsSucess = false,
        //            Message = ex.Message,
        //        };
        //    }
        //}
    }
}

