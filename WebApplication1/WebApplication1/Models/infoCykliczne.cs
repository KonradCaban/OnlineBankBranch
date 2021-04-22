using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class infoCykliczne
    {
        [Key]
        public int id { get; set; }
        public DateTime? data { get; set; }
        public string tytul { get; set; }
        public string numer_konta_odbiorca { get; set; }
        public string numer_konta_nadawca { get; set; }
        public decimal kwota { get; set; }

        public string dane_odbiorcy { get; set; }

        public string dane_nadawcy { get; set; }

        public int co_ile { get; set; }

    }
}