namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Currency:Country
    {
        public string Code { get; set; }
        public string CurrencyName { get; set; }
        public string Symbol { get; set; }
    }
}
