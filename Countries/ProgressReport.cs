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
    }
}
