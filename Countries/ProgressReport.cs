namespace Countries
{
    using Countries.Models;
    using System.Collections.Generic;

    public class ProgressReport
    {
        public int Percentagem { get; set; } = 0;
        public List<Country> SaveCountries { get; set; } = new List<Country>();
        public List<Rates> SaveRates { get; set; } = new List<Rates>();
    }
}
