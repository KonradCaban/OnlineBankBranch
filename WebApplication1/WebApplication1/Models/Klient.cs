using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Klient
    {

        [Display(Name = "ID")]
        [Key]
        public int id_klienta { get; set; }
        [Required(ErrorMessage = "Podaj login")]
        [Range(10000000, 99999999)]
        [Display(Name = "Login")]
        public String login { get; set; }
        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Musisz podać hasło")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Hasło powinno się składać z minimum 8 znaków")]
        public String haslo { get; set; }
        [Required(ErrorMessage = "Podaj swoje imię")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Imię powinno posiadać minimum 3 znaki")]
        [Display(Name = "Imię")]
        public  String imie { get; set; }
        [Required(ErrorMessage = "Podaj swoje nazwisko")]
        [StringLength(60, MinimumLength = 3, ErrorMessage = "Nazwisko powinno posiadać minimum 3 znaki")]
        [Display(Name = "Nazwisko")]
        public String nazwisko { get; set; }
        [Required(ErrorMessage = "Podaj swój PESEL")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi składać się z 11 cyfr")]
        [RegularExpression(@"([0-9]+)", ErrorMessage = "PESEL musi składać się z 11 cyfr.")]
        [Display(Name = "PESEL")]
        public String pesel { get; set; }
        [Required(ErrorMessage = "Podaj swój numer dowodu")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Numer dowodu składa się z 9 znaków")]
        [RegularExpression(@"([A-Z][A-Z][A-Z][0-9]+)", ErrorMessage = "Zły format numeru dowodu.")]
        [Display(Name = "Numer dowodu")]
        public String numer_dowodu { get; set; }
        [Required(ErrorMessage = "Podaj swój numer telefonu")]
        [StringLength(9, MinimumLength = 7, ErrorMessage = "Numer telefonu powinien składać się z minimum 7 cyfr")]
        [Range(1,999999999, ErrorMessage = "Numer telefonu powinien składać się z maksymalnie 9 cyfr")]
        [Display(Name = "Numer telefonu")]
        public String telefon { get; set; }
        [Required(ErrorMessage = "Podaj swój adres email")]
        [EmailAddress(ErrorMessage = "Zły format adresu email")]
        [StringLength(70, MinimumLength = 5, ErrorMessage = "Adres email jest za krótki")]
        [Display(Name = "Adres E-mail")]
        public String email { get; set; }

        [Required(ErrorMessage = "Wybierz swoją płeć")]
        [ForeignKey("Plec")]
        public int plec { get; set; }

        [Required(ErrorMessage = "Wybierz swoją datę urodzenia")]
        [DataType(DataType.Date)]
        [Display(Name = "Data urodzenia")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime data_ur { get; set; }

        [Required]
        public bool blokada { get; set; }


        public Klient(int id_klienta, string login, string haslo, string imie, string nazwisko, string pesel, string numer_dowodu, string telefon, string email, DateTime data_ur, bool blokada,int plec)
        {
            this.id_klienta = id_klienta;
            this.login = login;
            this.haslo = haslo;
            this.imie = imie;
            this.nazwisko = nazwisko;
            this.pesel = pesel;
            this.numer_dowodu = numer_dowodu;
            this.telefon = telefon;
            this.email = email;
            this.data_ur = data_ur;
            this.blokada = blokada;
            this.plec = plec;

        }

        public Klient()
        {
        }
    }
}