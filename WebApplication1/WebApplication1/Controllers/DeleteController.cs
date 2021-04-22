using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DeleteController : Controller
    {
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Home");
        }
        public ActionResult TwojePrzelewyCykliczne()
        {
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
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
                


                List<string> numery = db.Kontas.Where(p => p.id_klienta == kl.id_klienta).Select(p => p.numer_konta).ToList();
                List<infoCykliczne> list = db.Przelewy_Cykliczne.Where(p => numery.Contains(p.od_kogo))
                    .Select(p => new infoCykliczne { co_ile = p.co_ile_dni, dane_nadawcy = p.dane_osobowe_nadawca, dane_odbiorcy = p.dane_osobowe, data = p.data, id = p.id_przelewu, kwota = p.kwota, numer_konta_nadawca = p.od_kogo, numer_konta_odbiorca = p.do_kogo, tytul = p.tytul }).ToList();



                TempData["prz"] = list;




                return View(code);
            }
        }
        [HttpPost]
        public ActionResult UsunPrzelew(string kod, string check_code,string id_przelewu)
        {

            int id = Int32.Parse(id_przelewu);
            DB_Entities db = new DB_Entities();
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("TwojePrzelewyCykliczne", "Delete");
            }
            using (MD5 md5Hash = MD5.Create())
            {
                Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(check_code) && p.aktywny == true);
                if (!check_code.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)))
                {

                    TempData["title"] = "Błąd";
                    TempData["message"] = "Podałeś niepoprawne dane autoryzacyjne";
                    TempData["type"] = "error";

                    return RedirectToAction("TwojePrzelewyCykliczne", "Delete");
                }
                k.aktywny = false;
                db.SaveChanges();
            }
                Przelewy_Cykliczne pc = db.Przelewy_Cykliczne.FirstOrDefault(p => p.id_przelewu == id);
            if(pc == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Wybrany przelew cykliczny już nie istnieje w bazie danych.";
                TempData["type"] = "error";
                return RedirectToAction("TwojePrzelewyCykliczne", "Delete");
            }
           
            db.Przelewy_Cykliczne.Remove(pc);
            db.SaveChanges();
            TempData["title"] = "Sukces";
            TempData["message"] = "Wybrany przelew cykliczny został usunięty.";
            TempData["type"] = "success";
            return RedirectToAction("TwojePrzelewyCykliczne", "Delete");
        }


        [HttpPost]
        public ActionResult UsunKontoConf(string id_konta,string password,string code,string check_code)
        {
            DB_Entities db = new DB_Entities();
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
            Klient kl = (Klient)Session["Klient"];
            int id_k = Int32.Parse(id_konta);
            Konta k = db.Kontas.FirstOrDefault(p => p.id_konta.Equals(id_k));
            if (k == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Nie masz dostępu do tego konta";
                TempData["type"] = "error";
                return RedirectToAction("TwojeRachunki", "Home");
            }
            using (MD5 md5Hash = MD5.Create())
            {
                Kody_Autoryzujace kod = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(check_code) && p.aktywny == true);
                if (!check_code.Equals(new RegisterController().GetMd5Hash(md5Hash, code)) || !kl.haslo.Equals(new RegisterController().GetMd5Hash(md5Hash, password)))
                {

                    TempData["title"] = "Błąd";
                    TempData["message"] = "Podałeś niepoprawne dane";
                    TempData["type"] = "error";

                    return RedirectToAction("TwojeRachunki", "Home");
                }
                db.Kontas.Remove(k);
                db.SaveChanges();
                kod.aktywny = false;
                db.SaveChanges();

                TempData["title"] = "Sukces";
                TempData["message"] = "Konto zostało usunięte";
                TempData["type"] = "success";
            }
            return RedirectToAction("TwojeRachunki", "Home");
        }
        [HttpPost]
        public ActionResult UsunKonto(string id_konta)
        {
            if(id_konta== null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Brak dostępu";
                TempData["type"] = "error";
            }
            if(Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do tej strony należy zalogować się do serwisu";
                TempData["type"] = "error";
            }
            DB_Entities db = new DB_Entities();
            Klient kl = (Klient)Session["Klient"];
            int id_k = Int32.Parse(id_konta);

            Konta k = db.Kontas.FirstOrDefault(p => p.id_konta==id_k && p.id_klienta==kl.id_klienta);
            if(k == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Nie masz dostępu do tego konta";
                TempData["type"] = "error";
                return RedirectToAction("TwojeRachunki", "Home");
            }
            if(k.stan_konta<0)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Zanim usuniesz konto musisz uregulować debet";
                TempData["type"] = "error";
                return RedirectToAction("TwojeRachunki", "Home");
            }
            Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(kl.id_klienta) && p.aktywny == true)
            .Select(p => new Kod { numer_kodu = p.numer_kodu, kod = p.kod })
            .OrderBy(p => Guid.NewGuid())
            .FirstOrDefault();
            if (code == null)
            {
                TempData["title"] = "Brak kodów";
                TempData["message"] = "Nie posiadasz żadnych aktywnych kodów autoryzacyjnych. Aby uzyskać dostęp do tej strony, wygeneruj nowe";
                TempData["type"] = "error";
                return RedirectToAction("Kody", "Edit");
            }
            TempData["kod"] = code;
            TempData["id_k"] = id_konta;
            return View();
        }
    }
}