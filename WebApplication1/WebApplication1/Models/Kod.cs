using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Kod
    {
        public string kod { get; set; }
        public int id_klienta { get; set; }
        public int numer_kodu { get; set; }
        public Nullable<bool> aktywny { get; set; }


    }
}