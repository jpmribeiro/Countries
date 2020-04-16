namespace Countries.Services
{
    using Models;
    using System.Net;

    public class NetworkService //Disponibiliza uma ligaçõa à Internet
    {
        public Response CheckConnection()
        {
            var client = new WebClient();

            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSucess = true,
                    };
                }
            }
            catch
            {
                return new Response
                {
                    IsSucess = false,
                    Message = "There is no Internet Connection",
                };
            }

        }
    }
}
