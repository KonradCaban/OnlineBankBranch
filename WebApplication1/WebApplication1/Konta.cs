//------------------------------------------------------------------------------
// <auto-generated>
//    Ten kod źródłowy został wygenerowany na podstawie szablonu.
//
//    Ręczne modyfikacje tego pliku mogą spowodować nieoczekiwane zachowanie aplikacji.
//    Ręczne modyfikacje tego pliku zostaną zastąpione w przypadku ponownego wygenerowania kodu.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    
    public partial class Konta
    {
        public int id_konta { get; set; }
        public string numer_konta { get; set; }
        public int typ_konta { get; set; }
        public Nullable<decimal> stan_konta { get; set; }
        public Nullable<decimal> limit_d { get; set; }
        public Nullable<decimal> limit_m { get; set; }
        public System.DateTime data_otwarcia { get; set; }
        public Nullable<int> id_klienta { get; set; }
    
        public virtual Klienci Klienci { get; set; }
        public virtual Typy_Konta Typy_Konta { get; set; }
    }
}