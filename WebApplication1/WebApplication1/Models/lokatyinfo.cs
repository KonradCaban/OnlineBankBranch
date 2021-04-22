using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class lokatyinfo
    {
        public DateTime? data
        {
            get; set;
        }

        public decimal kwota { get; set; }
        public double oprocentowanie { get; set; }
        public int okres { get; set; }

    }
}