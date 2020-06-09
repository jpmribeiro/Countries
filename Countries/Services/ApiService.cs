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
        public async Task <Response> GetHolidays(string urlBase, string controller)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase); //API Adress
                var response = await  client.GetAsync(controller); //API Controller
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

        /// <summary>
        /// Gets Data from cambiosrafa API
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public async Task<Response> GetRates(string urlBase, string controller, IProgress<ProgressReport> progress) 
        {
            ProgressReport report3 = new ProgressReport();

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase);
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSucess = false,
                        Message = result
                    };
                }

                var rates = JsonConvert.DeserializeObject<List<Rates>>(result);

                report3.SaveRates = rates;
                report3.Percentagem = (report3.SaveRates.Count * 100) / rates.Count;
                progress.Report(report3);

                return new Response
                {
                    IsSucess = true,
                    Result = rates,
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

    }
}

