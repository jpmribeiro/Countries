namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Holiday
    {
        public string name { get; set; }
        public string date { get; set; }
        public override string ToString()
        {
            return $"{name}, Celebrated at {date};";
        }

    }
}
