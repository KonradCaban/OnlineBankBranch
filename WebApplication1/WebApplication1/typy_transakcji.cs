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
    
    public partial class typy_transakcji
    {
        public typy_transakcji()
        {
            this.Transakcjes = new HashSet<Transakcje>();
            this.Przelewy_Cykliczne = new HashSet<Przelewy_Cykliczne>();
        }
    
        public int id_typu { get; set; }
        public string nazwa_typu { get; set; }
    
        public virtual ICollection<Transakcje> Transakcjes { get; set; }
        public virtual ICollection<Przelewy_Cykliczne> Przelewy_Cykliczne { get; set; }
    }
}