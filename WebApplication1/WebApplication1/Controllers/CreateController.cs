
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Security.Cryptography;

using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CreateController : Controller
    {

        public ActionResult UtworzPrzelewCykliczny(Przelewy_Cykliczne pr, string password, string kod, string check_code)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                if (Session["Klient"] == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Klient kl = (Klient)Session["Klient"];

                using (DB_Entities db = new DB_Entities())
                {
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(check_code) && p.aktywny == true);
                    if (!check_code.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) || !kl.haslo.Equals(new RegisterController().GetMd5Hash(md5Hash, password)) || k == null)
                    {

                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś niepoprawne dane";
                        TempData["type"] = "error";

                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    if (!ModelState.IsValid)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nieoczekiwany błąd";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }

                    else
                    {
                        Konta k2 = db.Kontas.FirstOrDefault(p => p.numer_konta.Equals(pr.od_kogo));
                        Typy_Konta tk = db.Typy_Konta.FirstOrDefault(p => p.id_typu == k2.typ_konta);
                        if (k2.stan_konta - tk.prowizja - pr.kwota < 0)
                        {
                            TempData["title"] = "Błąd";
                            TempData["message"] = "Niewystarczający stan konta";
                            TempData["type"] = "error";
                            return RedirectToAction("PanelKlienta", "Home");
                        }
                        else
                        {
                            TempData["title"] = "Sukces";
                            TempData["message"] = "Przelew został wykonany";
                            TempData["type"] = "success";
                        }
                        db.Przelewy_Cykliczne.Add(pr);
                        db.SaveChanges();
                        k.aktywny = false;
                        db.SaveChanges();

                        return RedirectToAction("PanelKlienta", "Home");
                    }


                    return RedirectToAction("PanelKlienta", "Home");
                }
            }
               
        }

        public ActionResult Przelew(Transakcje tr, string password, string kod, string check_code)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                DateTime dt = DateTime.Now;
                if (Session["Klient"] == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Klient kl = (Klient)Session["Klient"];

                using (DB_Entities db = new DB_Entities())
                {

                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(check_code) && p.aktywny == true);
                    if (!check_code.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) || !kl.haslo.Equals(new RegisterController().GetMd5Hash(md5Hash, password)) || k == null)
                    {

                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś niepoprawne dane";
                        TempData["type"] = "error";

                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Konta konto = db.Kontas.FirstOrDefault(p => p.numer_konta.Equals(tr.od_kogo));
                    decimal value2 = konto.limit_d ?? 0;
                    if (value2<tr.kwota)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Ustawiony limit na rachunku nie pozwala na zrealizowanie przelewu";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    DateTime d = DateTime.Today;
                    tr.kwota = (Decimal)tr.kwota;
                    if (!ModelState.IsValid)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nieoczekiwany błąd";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    else
                    {
                        Konta k2 = db.Kontas.FirstOrDefault(p => p.numer_konta.Equals(tr.od_kogo));
                        Typy_Konta tk = db.Typy_Konta.FirstOrDefault(p => p.id_typu == k2.typ_konta);
                        if (k2.stan_konta - tk.prowizja - tr.kwota < 0)
                        {
                            TempData["title"] = "Błąd";
                            TempData["message"] = "Niewystarczający stan konta";
                            TempData["type"] = "error";
                            return RedirectToAction("PanelKlienta", "Home");
                        }
                        tr.wykonane = true;
                        DateTime tmp = (DateTime)tr.data_transakcji;
                        if (DateTime.Compare(dt, tmp) < 0)
                        {
                            TempData["title"] = "Sukces";
                            TempData["message"] = "Przelew zostanie wykonany podanego dnia";
                            TempData["type"] = "success";
                            tr.wykonane = false;
                        }
                        else
                        {
                            TempData["title"] = "Sukces";
                            TempData["message"] = "Przelew został wykonany";
                            TempData["type"] = "success";
                            tr.data_transakcji = dt;
                        }
                        db.Transakcjes.Add(tr);
                        db.SaveChanges();
                        k.aktywny = false;
                        db.SaveChanges();

                        return RedirectToAction("PanelKlienta", "Home");
                    }
                }

            }


        }
        [HttpPost]
        public ActionResult SplataRaty(string password, string kod, string check_code, string rata, string id_kredytu, string konto_cel)
        {
           
            using (MD5 md5Hash = MD5.Create())
            {
                DateTime dt = DateTime.Now;
                if (Session["Klient"] == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Klient kl = (Klient)Session["Klient"];


                using (DB_Entities db = new DB_Entities())
                {

                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(check_code) && p.aktywny == true);
                    if (!check_code.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) || !kl.haslo.Equals(new RegisterController().GetMd5Hash(md5Hash, password)) || k == null)
                    {

                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś niepoprawne dane";
                        TempData["type"] = "error";

                        return RedirectToAction("PanelKlienta", "Home");
                    }
                     
                    Decimal r = Decimal.Parse(rata);
                    Konta konto = db.Kontas.FirstOrDefault(p => p.id_klienta == kl.id_klienta && p.stan_konta > r);
                   
                    Typy_Konta tk = db.Typy_Konta.FirstOrDefault(p => p.id_typu == konto.typ_konta);
                    
                    if (konto.stan_konta - tk.prowizja - r < 0)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Niewystarczający stan konta";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Debug.WriteLine(r);
                    Transakcje tr = new Transakcje { dane_osobowe = kl.imie + " " + kl.nazwisko, dane_osobowe_nadawca = "Bank Świnka", data_transakcji = dt, do_kogo = "04175012822459556296178942", kwota = Math.Round(r,2), od_kogo = konto.numer_konta, tytul = "Spłata raty kredytu", wykonane = true, kategoria = 5, typ_transakcji = 2 };
                    k.aktywny = false;
                 
                        db.SaveChanges();

                    try
                    {
                        db.Transakcjes.Add(tr);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                    

                    int id = Int32.Parse(id_kredytu);
                    Raty_kredytu rk = db.Raty_kredytu.FirstOrDefault(p => p.id_kredytu == id && p.czy_splacona==false);
                    rk.czy_splacona = true;
                    db.SaveChanges();
                    TempData["title"] = "Sukces";
                    TempData["message"] = "Rata została spłacona";
                    TempData["type"] = "success";
                    return RedirectToAction("PanelKlienta", "Home");

                }

            }
        }

        public ActionResult PrzelewCykliczny()
        {
            using (DB_Entities db = new DB_Entities())
            {


                Klient kl = (Klient)Session["Klient"];
                if (Session["klient"] == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
                .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefault();
                if (code == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby wykonywać działania na swoim koncie, należy wygenerować nowe";
                    TempData["type"] = "error";
                    return RedirectToAction("Kody", "Edit");
                }

                List<subKonta> ks = db.Kontas.Where(p => p.id_klienta == kl.id_klienta).Select(p => new subKonta { numer_konta = p.numer_konta, stan_konta = p.stan_konta }).ToList();
                if (ks.Count() == 0)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kont z ktorych możesz wykonać przelew";
                    TempData["type"] = "error";
                    return RedirectToAction("PanelKlienta", "Home");
                }
                var k = db.kategories.ToList();
                var tt = db.typy_transakcji.ToList();
                Session["kod"] = code;
                Dictionary<string, int> kategorie = new Dictionary<string, int>();
                Dictionary<string, int> typy_t = new Dictionary<string, int>();
                Dictionary<string, string> konta = new Dictionary<string, string>();
                foreach (var item in k)
                {
                    kategorie.Add(item.nazwa_kategorii, item.id_kategorii);
                }
                foreach (var item in tt)
                {
                    typy_t.Add(item.nazwa_typu, item.id_typu);
                }
                foreach (var item in ks)
                {
                    string kw = item.stan_konta.ToString().Substring(0, item.stan_konta.ToString().Length - 2);
                    konta.Add(item.numer_konta + " (" + kw + "zł )", item.numer_konta);
                }
                Session["kategorie"] = kategorie;
                Session["twoje_konta"] = konta;
                Session["typy"] = typy_t;
                return View();
            }
        }
        public ActionResult PrzelewJednorazowy()
        {
            using (DB_Entities db = new DB_Entities())
            {
               

                Klient kl = (Klient)Session["Klient"];
                if (Session["klient"] == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
                .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefault();
                if (code == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby wykonywać działania na swoim koncie, należy wygenerować nowe";
                    TempData["type"] = "error";
                    return RedirectToAction("Kody", "Edit");
                }

                List<subKonta> ks = db.Kontas.Where(p => p.id_klienta == kl.id_klienta).Select(p => new subKonta { numer_konta = p.numer_konta, stan_konta = p.stan_konta }).ToList();
                if (ks.Count() == 0)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kont z ktorych możesz wykonać przelew";
                    TempData["type"] = "error";
                    return RedirectToAction("PanelKlienta", "Home");
                }
                var k = db.kategories.ToList();
                var tt = db.typy_transakcji.ToList();
                Session["kod"] = code;
                Dictionary<string, int> kategorie = new Dictionary<string, int>();
                Dictionary<string, int> typy_t = new Dictionary<string, int>();
                Dictionary<string, string> konta = new Dictionary<string, string>();
                foreach (var item in k)
                {
                    kategorie.Add(item.nazwa_kategorii, item.id_kategorii);
                }
                foreach (var item in tt)
                {
                    typy_t.Add(item.nazwa_typu, item.id_typu);
                }
                foreach (var item in ks)
                {
                    string kw = item.stan_konta.ToString().Substring(0, item.stan_konta.ToString().Length - 2);
                    konta.Add(item.numer_konta + " (" + kw + "zł )", item.numer_konta);
                }
                Session["kategorie"] = kategorie;
                Session["twoje_konta"] = konta;
                Session["typy"] = typy_t;
                return View();
            }
        }
        // GET: Create
        public ActionResult Oferta()
        {
            using (DB_Entities db = new DB_Entities())
            {
                var info = db.Typy_Konta.Select(p => new Acc_Info { nazwa_typu = p.nazwa_typu, opis = p.opis, id_typu = p.id_typu }).ToList();
                ViewData["info"] = info;
                return View();
            }
        }

        public ActionResult OfertaKredytowa()
        {
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        public ActionResult OfertaLokat()
        {
            return View();
        }

        public ActionResult KalkulatorRat()
        {
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }

            return RedirectToAction("PanelKlienta", "Home");
        }

        public ActionResult KalkulatorLokat()
        {
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }

            return RedirectToAction("PanelKlienta", "Home");
        }

        [HttpPost]
        public ActionResult KalkulatorRat(double oprocentowanie, double prowizja, int min_kwota, int maks_kwota, int min_rat, int maks_rat)
        {
            List<subKonta> sk = (List<subKonta>)Session["konta"];
            List<infoKredyt> info_k = (List<infoKredyt>)Session["kredyty"];
            if (info_k.Count() >= 3)
            {
                TempData["title"] = "Odmowa dostępu";
                TempData["message"] = "W Naszym banku istnieje limit trzech otwartych kredytow na jedno konto bankowe. Aby zaciągnąć nowy kredyt, należy spłacić chociaż jeden z zaległych kredytow.";
                TempData["type"] = "error";
                return RedirectToAction("PanelKlienta", "Home");
            }
            if (sk.Count() == 0)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy posiadać otwarty rachunek w naszym banku";
                TempData["type"] = "error";
                return RedirectToAction("PanelKlienta", "Home");
            }

            if (Session["Klient"] == null) 
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
            using (DB_Entities db = new DB_Entities())
            {
                Klient kl = (Klient)Session["Klient"];
                Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
                .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefault();
                if (code == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby wykonywać działania na swoim koncie, należy wygenerować nowe";
                    TempData["type"] = "error";
                    return RedirectToAction("Kody", "Edit");
                }

                TempData["oprocentowanie"] = oprocentowanie;
                TempData["prowizja"] = prowizja;
                TempData["min_kwota"] = min_kwota;
                TempData["maks_kwota"] = maks_kwota;
                TempData["min_rat"] = min_rat;
                TempData["maks_rat"] = maks_rat;
                return View(code);
            }
        }

        [HttpPost]
        public ActionResult KalkulatorLokat(double oprocentowanie,int min_kwota, int maks_kwota, int min_m, int maks_m)
        {

            List<subKonta> sk = (List<subKonta>)Session["konta"];
            if (sk.Count() == 0)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy posiadać otwarty rachunek w naszym banku";
                TempData["type"] = "error";
                return RedirectToAction("PanelKlienta", "Home");
            }

            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
            using (DB_Entities db = new DB_Entities())
            {
                Klient kl = (Klient)Session["Klient"];
                Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
                .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefault();
                if (code == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby wykonywać działania na swoim koncie, należy wygenerować nowe";
                    TempData["type"] = "error";
                    return RedirectToAction("Kody", "Edit");
                }
                TempData["oprocentowanie"] = oprocentowanie;
                TempData["min_kwota"] = min_kwota;
                TempData["maks_kwota"] = maks_kwota;
                TempData["min_m"] = min_m;
                TempData["maks_m"] = maks_m;
                return View(code);
            }
        }




        [HttpPost]
        public ActionResult NowyRachunek(int id)
        {
            
            using (DB_Entities db = new DB_Entities())
            {
                
                if (Session["Klient"] == null )
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Aby uzyskać dostęp do strony należy zalogować się do serwisu";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
                Klient kl = (Klient)Session["Klient"];
                var info = db.Typy_Konta.FirstOrDefault(p => p.id_typu == id);
                Acc_Info acc = new Acc_Info { id_typu = info.id_typu, nazwa_typu = info.nazwa_typu, opis = info.opis, oplata_m = info.oplata_za_prowadzenie, prowizja = info.prowizja };
                Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
                    .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
                    .OrderBy(p => Guid.NewGuid())
                    .FirstOrDefault();
                if (code == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby wykonywać działania na swoim koncie, należy wygenerować nowe";
                    TempData["type"] = "error";
                    return RedirectToAction("Kody", "Edit");
                }
                ViewData["info"] = acc;
                return View(code);
            }
        }
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Home");
        }

        public ActionResult NowyKredyt()
        {
            return RedirectToAction("Login", "Home");
        }


        public ActionResult NowyRachunek()
        {
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> CreateNewAcc(string password, string kod, string check_code, string id_typu)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                using (DB_Entities db = new DB_Entities())
                {
                    int id = Int32.Parse(id_typu);
                    DateTime dla_ml = DateTime.Now.Date.AddYears(-13);
                    DateTime dla_st = DateTime.Now.Date.AddYears(-18);
                    DateTime gr = DateTime.Now.Date.AddYears(-26);
                    Klient kl = (Klient)Session["Klient"];
                    if ((DateTime.Compare(dla_ml, kl.data_ur) < 0 || DateTime.Compare(dla_st, kl.data_ur) > 0) && id == 3)
                    {
                        Debug.WriteLine(DateTime.Compare(dla_ml, kl.data_ur));
                        Debug.WriteLine(DateTime.Compare(dla_st, kl.data_ur));
                        Debug.WriteLine(dla_ml.ToString() + " " + kl.data_ur.ToString() + " " + dla_st.ToString());
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nie spełniasz warunków do założenia takiego konta";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }

                    if ((DateTime.Compare(dla_st, kl.data_ur) < 0 || DateTime.Compare(gr, kl.data_ur) > 0) && id == 4)
                    {
                        Debug.WriteLine(DateTime.Compare(dla_st, kl.data_ur));
                        Debug.WriteLine(DateTime.Compare(gr, kl.data_ur));
                        Debug.WriteLine(dla_st.ToString() + " " + kl.data_ur.ToString() + " " + gr.ToString());
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nie spełniasz warunków do założenia takiego konta";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }

                    string hashed_code = new RegisterController().GetMd5Hash(md5Hash, kod);
                    string hashed_pass = new RegisterController().GetMd5Hash(md5Hash, password);
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code && p.aktywny == true);
                    if (k == null)
                    {

                        TempData["title"] = "Błąd";
                        TempData["message"] = "Błędny kod autoryzujący";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Debug.WriteLine(hashed_code + " " + check_code);
                    Debug.WriteLine(hashed_pass + " " + kl.haslo);
                    if (!check_code.Equals(hashed_code) || !kl.haslo.Equals(hashed_pass))
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędne dane";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Kody_Autoryzujace tmp_kod = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code);
                    tmp_kod.aktywny = false;
                    db.SaveChanges();

                    string numer_konta_str = "115013160000";
                    Random r = new Random();
                    for (int i = 1; i <= 12; i++)
                        numer_konta_str += r.Next(0, 10).ToString();
                    numer_konta_str += "252100";
                    Debug.WriteLine(numer_konta_str);
                    BigInteger numer_konta = BigInteger.Parse(numer_konta_str);
                    BigInteger cyfry_kontrolne = BigInteger.ModPow(numer_konta, 1, 97);
                    cyfry_kontrolne = 98 - cyfry_kontrolne;
                    numer_konta_str = cyfry_kontrolne.ToString() + numer_konta_str.Substring(0, numer_konta_str.Length - 6);
                    if (numer_konta_str.Length != 26)
                        numer_konta_str = "0" + numer_konta_str;
                    Debug.WriteLine(numer_konta_str);
                    Konta new_acc = new Konta { data_otwarcia = DateTime.Now.Date, id_klienta = kl.id_klienta, limit_d = 0, limit_m = 0, numer_konta = numer_konta_str, stan_konta = 0, typ_konta = id };
                    db.Kontas.Add(new_acc);
                    db.SaveChanges();
                    TempData["title"] = "Sukces";
                    TempData["message"] = "Utworzono nowy rachunek!";
                    TempData["type"] = "success";
                    Session["konta"] = new subKonta().get_sub(kl.id_klienta);





                    var body = "<p>Witaj!:</p>";
                    body += "<br /><br />Utworzyliśmy nowy rachunek dla Twojego konta.<br>Chcesz uzyskać więcej informacji? Zaloguj się na swoje konto!<br>Kliknij w link poniżej aby przejść do strony logowania do naszego serwisu.";
                    body += "<br /><a href ='https://localhost:44314/Home/Login/'>Kliknij tutaj</a><br><br>Bank Świnka";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(kl.email));
                    message.From = new MailAddress("bot@bankswinka.com");
                    message.Subject = "Utworzono nowy rachunek dla twojego konta bankowego";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body);
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "botswinka@gmail.com",  // replace with valid value
                            Password = "zaq1@WSX"  // replace with valid value
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);

                    }








                    return RedirectToAction("PanelKlienta", "Home");




                }
            }

        }



        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> NowaLokata(string ilosc_m, string kwota_z, string procent, string password, string kod, string check_code, string konto_cel)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                using (DB_Entities db = new DB_Entities())
                {
                    int dl = Int32.Parse(ilosc_m);
                    Klient kl = (Klient)Session["Klient"];
                    DateTime dt = DateTime.Now;
                    Double pr = Double.Parse(procent);
                    string hashed_code = new RegisterController().GetMd5Hash(md5Hash, kod);
                    string hashed_pass = new RegisterController().GetMd5Hash(md5Hash, password);
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code && p.aktywny == true);
                    if (k == null)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Błędny kod autoryzujący";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Debug.WriteLine(hashed_code + " " + check_code);
                    Debug.WriteLine(hashed_pass + " " + kl.haslo);
                    if (!check_code.Equals(hashed_code) || !kl.haslo.Equals(hashed_pass))
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędne dane";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Kody_Autoryzujace tmp_kod = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code);
                    tmp_kod.aktywny = false;
                    db.SaveChanges();





                    Konta k2 = db.Kontas.FirstOrDefault(p => p.numer_konta.Equals(konto_cel));

                    Typy_Konta tk = db.Typy_Konta.FirstOrDefault(p => p.id_typu == k2.typ_konta);
                    Decimal r = Decimal.Parse(kwota_z);
                    if (k2.stan_konta - tk.prowizja - r < 0)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Niewystarczający stan konta";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Transakcje tr = new Transakcje { dane_osobowe = kl.imie + " " + kl.nazwisko, dane_osobowe_nadawca = "Bank Świnka", data_transakcji = dt, do_kogo = "04175012822459556296178942", kwota = r, od_kogo = konto_cel, tytul = "Przelew pieniędzy na lokatę", wykonane = true, kategoria = 6, typ_transakcji = 5 };
                    db.Transakcjes.Add(tr);
                    db.SaveChanges();
                    TempData["title"] = "Sukces";
                    TempData["message"] = "Utworzyłeś lokatę!";
                    TempData["type"] = "success";
                    Lokaty l = new Lokaty { data_rozpoczecia = dt, id_klienta = kl.id_klienta, kapitalizacja = "miesiac", kwota = r, okres = dl, oprocentowanie = pr };
                    db.Lokaties.Add(l);
                    db.SaveChanges();


                    var body = "<p>Witaj!:</p>";
                    body += "<br /><br />Utworzyliśmy nową lokatę dla Twojego konta.<br>Chcesz uzyskać więcej informacji? Zaloguj się na swoje konto!<br>Kliknij w link poniżej aby przejść do strony logowania do naszego serwisu.";
                    body += "<br /><a href ='https://localhost:44314/Home/Login/'>Kliknij tutaj</a><br><br>Bank Świnka";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(kl.email));
                    message.From = new MailAddress("bot@bankswinka.com");
                    message.Subject = "Utworzono nową lokatę dla twojego konta bankowego";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body);
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "botswinka@gmail.com",  // replace with valid value
                            Password = "zaq1@WSX"  // replace with valid value
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);

                    }

                    return RedirectToAction("PanelKlienta", "Home");


                }
            }
        }





        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> NowyKredyt( string rata_hid,string kwota_hid,string prow_hid, string proc_hid, string rrso_hid,string password, string kod, string check_code, string konto_cel)
        {

            using (MD5 md5Hash = MD5.Create())
            {
                using (DB_Entities db = new DB_Entities())
                {
                   
                    int rata = Int32.Parse(rata_hid);
                    Debug.WriteLine(rata_hid);
                    double pr = Double.Parse(prow_hid);
                    int kwota = Int32.Parse(kwota_hid);

  
                    double procent = Double.Parse(proc_hid);
                    Klient kl = (Klient)Session["Klient"];
                    string hashed_code = new RegisterController().GetMd5Hash(md5Hash, kod);
                    string hashed_pass = new RegisterController().GetMd5Hash(md5Hash, password);
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code && p.aktywny == true);
                    if (k == null)
                    {
                        Debug.WriteLine("2");
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Błędny kod autoryzujący";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Debug.WriteLine(hashed_code + " " + check_code);
                    Debug.WriteLine(hashed_pass + " " + kl.haslo);
                    if (!check_code.Equals(hashed_code) || !kl.haslo.Equals(hashed_pass))
                    {
                        Debug.WriteLine("3");
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędne dane";
                        TempData["type"] = "error";
                        return RedirectToAction("PanelKlienta", "Home");
                    }
                    Kody_Autoryzujace tmp_kod = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod == hashed_code);
                    tmp_kod.aktywny = false;
                    db.SaveChanges();
                    Kredyty nowy_kredyt = new Kredyty { id_klienta = kl.id_klienta, data_zawarcia = DateTime.Now.Date, kwota = kwota, okres_kredytowania = rata, oprocentowanie = procent, prowizja = pr, rrso = 3 };
                    db.Kredyties.Add(nowy_kredyt);
                    db.SaveChanges();
                    Klienci kl_tmp = db.Kliencis.FirstOrDefault(p => p.id_klienta == kl.id_klienta);
                    Transakcje tr = new Transakcje { dane_osobowe_nadawca = "Bank Świnka", dane_osobowe = kl_tmp.imie + " " + kl_tmp.nazwisko, data_transakcji = DateTime.Now.Date, kwota = kwota, tytul = "Kredyt", od_kogo = "04175012822459556296178942", do_kogo = konto_cel, wykonane = true, kategoria=4, typ_transakcji = 4 };
                    db.Transakcjes.Add(tr);
                    db.SaveChanges();
                    TempData["title"] = "Sukces";
                    TempData["message"] = "Otrzymałeś kredyt!";
                    TempData["type"] = "success";



                    var body = "<p>Witaj!:</p>";
                    body += "<br /><br />Utworzyliśmy nowy kredyt dla Twojego konta.<br>Chcesz uzyskać więcej informacji? Zaloguj się na swoje konto!<br>Kliknij w link poniżej aby przejść do strony logowania do naszego serwisu.";
                    body += "<br /><a href ='https://localhost:44314/Home/Login/'>Kliknij tutaj</a><br><br>Bank Świnka";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(kl.email));
                    message.From = new MailAddress("bot@bankswinka.com");
                    message.Subject = "Utworzono nowy kredyt dla twojego konta bankowego";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body);
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "botswinka@gmail.com",  // replace with valid value
                            Password = "zaq1@WSX"  // replace with valid value
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);

                    }

                    return RedirectToAction("PanelKlienta", "Home");



                }
            }
        }
    }
}