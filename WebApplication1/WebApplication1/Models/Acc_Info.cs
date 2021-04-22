using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Acc_Info
    {
        public string nazwa_typu { get; set; }
        public string opis { get; set; }

        public int id_typu { get; set; }

        public decimal oplata_m { get; set; }
        
        public decimal prowizja { get; set; }

    }
}