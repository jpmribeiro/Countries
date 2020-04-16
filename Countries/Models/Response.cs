namespace Countries.Models
{
    public class Response
    {
        public bool IsSucess { get; set; }
        public string Message { get; set; }
        public object Result { get; set; } //Meaning a Countrie, a successful connection or a list of countries
    }
}
