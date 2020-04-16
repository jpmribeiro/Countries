namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Language:Countrie
    {
        public string Iso639_1 { get; set; }
        public string Iso639_2 { get; set; }
        public string LanguageName { get; set; }
        public string LanguageNativeName { get; set; }
    }
}
