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
    
    public partial class Typy_Konta
    {
        public Typy_Konta()
        {
            this.Kontas = new HashSet<Konta>();
        }
    
        public int id_typu { get; set; }
        public string nazwa_typu { get; set; }
        public decimal oplata_za_prowadzenie { get; set; }
        public decimal prowizja { get; set; }
        public string opis { get; set; }
    
        public virtual ICollection<Konta> Kontas { get; set; }
    }
}
