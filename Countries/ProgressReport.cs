namespace Countries
{
    using Countries.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProgressReport
    {
        public int Percentagem { get; set; } = 0;
        public List<Country> SaveCountries { get; set; } = new List<Country>();
        public List<CountryHoliday> SaveHolidays { get; set; } = new List<CountryHoliday>();
        public List<Rates> SaveRates { get; set; } = new List<Rates>();
    }
}
