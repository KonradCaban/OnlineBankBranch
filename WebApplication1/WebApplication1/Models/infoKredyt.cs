using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class infoKredyt
    {
        [Key]
        public int id_kr { get; set; }

        public DateTime? data { get; set; }

        public decimal kwota { get; set; }  

        public int okres { get; set; }

        public double oprocentowanie { get; set; }

        public double? prowizja { get; set; }

        public double? rrso { get; set; }

        public int raty_pozostale { get; set; }

        public decimal kwota_najbliszej_raty { get; set; }

        public DateTime? termin_splaty { get; set; }
    }
}