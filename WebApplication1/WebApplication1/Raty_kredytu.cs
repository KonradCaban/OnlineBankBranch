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
    
    public partial class Raty_kredytu
    {
        public int id_raty { get; set; }
        public int id_kredytu { get; set; }
        public decimal kwota_raty { get; set; }
        public Nullable<bool> czy_splacona { get; set; }
        public System.DateTime termin_splaty { get; set; }
    
        public virtual Kredyty Kredyty { get; set; }
    }
}