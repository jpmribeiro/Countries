namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RegionalBloc:Country
    {
        public string Acronym { get; set; }
        public string RegionalBlocName { get; set; }
        public List<object> OtherAcronyms { get; set; }
        public List<object> OtherNames { get; set; }
    }
}
