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
        public async Task<Response> GetCountries(string urlBase, string controller)
        {
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



    }
}

