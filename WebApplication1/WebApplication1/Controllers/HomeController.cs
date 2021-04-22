using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult TwojeLokaty()
        {
            if (Session["Klient"] == null)
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
            }
            Klient kl = (Klient)Session["Klient"];
            using (DB_Entities db = new DB_Entities())
            {

                List<lokatyinfo> l_i = db.Lokaties.Where(p => p.id_klienta == kl.id_klienta)
                    .Select(p => new lokatyinfo { data = p.data_rozpoczecia, kwota = p.kwota, okres = p.okres, oprocentowanie = p.oprocentowanie })
                    .ToList();
                Session["lokaty"] = l_i;
            }


            return View();
        }
        public ActionResult TwojeRachunki()
        {
            if(Session["Klient"]==null)
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
            }
            Klient kl = (Klient)Session["Klient"];
            using (DB_Entities db = new DB_Entities())
            {

                List<subKonta> l_k = new subKonta().get_sub(kl.id_klienta);
                int[] count_tr = new int[l_k.Count];
                int i = 0;
                foreach(var item in l_k)
                {
                    count_tr[i] = db.Transakcjes.Where(p => p.od_kogo.Equals(item.numer_konta) || p.do_kogo.Equals(item.numer_konta)).Count();
                    i++;
                }
                TempData["konta"] = l_k;
                TempData["ile"] = count_tr;
            }


                return View();
        }
        public ActionResult Historia()
        {
            if (Session["klient"] == null)
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
            }
            transakcje_rozszerzone tmp = new transakcje_rozszerzone();
            subKonta tmp2 = new subKonta();
            List<subKonta> acc = tmp2.get_sub((Session["klient"] as Klient).id_klienta);
            Session["konta"] = acc;
            Session["transakcje"] = tmp.get_transactions((Session["klient"] as Klient).id_klienta,acc,true);
            return View();
        }


        public ActionResult Glowna()
        {
            using (DB_Entities db = new DB_Entities())
            {
                var info =db.Typy_Konta.Select(p => new Acc_Info {nazwa_typu= p.nazwa_typu, opis=p.opis}).ToList();
                ViewData["info"] = info;
            }
                return View();
        }
        public ActionResult Login()
        {
            if (Session["klient"] != null)
                return RedirectToAction("PanelKlienta");
            else
                return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Klienci kl)
        {
            if(ModelState.IsValid)
            {
                using(DB_Entities db = new DB_Entities())
                {
                    using (MD5 md5Hash = MD5.Create())
                    {
                        kl.haslo = GetMd5Hash(md5Hash, kl.haslo);
                        var o = db.Kliencis.Where(a => a.login.Equals(kl.login) && a.haslo.Equals(kl.haslo) && a.blokada == false).FirstOrDefault();
                        if (o != null)
                        {

                            transakcje_rozszerzone tmp = new transakcje_rozszerzone();
                            subKonta tmp2 = new subKonta();
                            Klient cl = new Klient(o.id_klienta, o.login.ToString(), o.haslo.ToString(), o.imie.ToString(), o.nazwisko.ToString(), o.pesel.ToString(), o.numer_dowodu.ToString(), o.telefon.ToString(), o.email.ToString(), o.data_ur, (bool)o.blokada, o.plec);
                            Session["Klient"] = cl;
                            List<subKonta> acc = tmp2.get_sub(o.id_klienta);
                            Session["konta"] = acc;
                            Session["transakcje"] = tmp.get_transactions(o.id_klienta, acc, false);
                               
                            return RedirectToAction("PanelKlienta");
                        }
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędne dane logowania";
                        TempData["type"] = "error";
                    }
                }
            }
            return View(kl);
        }
        public ActionResult PanelKredytow()
        {
            Klient kl = (Klient)Session["Klient"];
            if (Session["klient"] == null)
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
            }
            using (DB_Entities db = new DB_Entities())
            {
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

                List<infoKredyt> kr_list = db.Kredyties.Where(p => p.id_klienta == kl.id_klienta)
                    .Select(p => new infoKredyt { id_kr = p.id_kredytu, data = p.data_zawarcia, kwota = p.kwota, okres = p.okres_kredytowania, oprocentowanie = p.oprocentowanie, prowizja = p.prowizja, rrso = p.rrso, kwota_najbliszej_raty = 0, raty_pozostale = 0 })
                    .ToList();
                List<int> idlist = new List<int>();
                foreach (infoKredyt item in kr_list)
                {
                    idlist.Add(item.id_kr);
                }
                var raty_info = db.Raty_kredytu.Where(a => idlist.Contains(a.id_kredytu) && a.czy_splacona == false).OrderByDescending(a => a.id_raty);
                foreach (var item in raty_info)
                {
                    kr_list.Where(w => w.id_kr == item.id_kredytu).ToList().ForEach(s => { s.kwota_najbliszej_raty = item.kwota_raty; s.termin_splaty = item.termin_splaty; });
                }
                var ilosc_poz = db.Raty_kredytu.Where(a => a.czy_splacona == false)
                    .GroupBy(a => a.id_kredytu)
                    .Select(a => new { id = a.Key, ilosc = a.Count() });
                foreach (var item in ilosc_poz)
                {
                    kr_list.Where(w => w.id_kr == item.id).ToList().ForEach(s => { s.raty_pozostale = item.ilosc; });
                }
                List<subKonta> acc = new subKonta().get_sub(kl.id_klienta);
                TempData["l_w_kredytow"] = kr_list;
                Session["konta"] = acc;



                return View(code);
            }
        }
        public ActionResult PanelKlienta()
        {
            if (Session["Klient"] != null)
            {

                Klient kl = (Klient)Session["Klient"];
                List<subKonta> acc = new subKonta().get_sub(kl.id_klienta);
                List<String> list2 = new List<String>();
                foreach (var item in acc)
                {
                    list2.Add(item.numer_konta);
                }
                using (DB_Entities db = new DB_Entities())
                {
                    List<lokatyinfo> l = db.Lokaties.Where(p => p.id_klienta == kl.id_klienta && p.kwota > 0)
                        .Select(p => new lokatyinfo { kwota = p.kwota, data = p.data_rozpoczecia, okres = p.okres, oprocentowanie = p.oprocentowanie })
                        .ToList();
                    List<String> pow = db.powiadomienias.Where(p => list2.Contains(p.numer_konta))
                            .Select(p => p.tresc).Take(1).ToList();
                   TempData["powiadomienia"] = pow;
                    List<powiadomienia> pow_del = db.powiadomienias.Where(p => pow.Contains(p.tresc))
                        .Take(1).ToList();

                    foreach (powiadomienia item in pow_del)
                    {
                        db.powiadomienias.Remove(item);
                        db.SaveChanges();
                    }
                    List<infoKredyt> info = db.Kredyties.Join(db.Raty_kredytu, t => t.id_kredytu, k => k.id_kredytu, (t, k) => new { l = t, r = k })
                     .Where(a => a.r.czy_splacona == false)
                    .GroupBy(a => new { a.l.id_kredytu, a.l.data_zawarcia, a.l.kwota, a.l.okres_kredytowania, a.l.oprocentowanie, a.l.prowizja, a.l.rrso })
                    .Select(a => new infoKredyt { id_kr = a.Key.id_kredytu, data = a.Key.data_zawarcia, kwota = a.Key.kwota, okres = a.Key.okres_kredytowania, oprocentowanie = a.Key.oprocentowanie, raty_pozostale = a.Count(), prowizja = a.Key.prowizja, rrso = a.Key.rrso })
                    .ToList();

                    List<int> idlist = new List<int>();
                    foreach (infoKredyt item in info)
                    {
                        idlist.Add(item.id_kr);
                    }
                    var raty_info = db.Raty_kredytu.Where(a => idlist.Contains(a.id_kredytu) && a.czy_splacona == false).OrderByDescending(a => a.id_raty);
                    foreach (var item in raty_info)
                    {
                        info.Where(w => w.id_kr == item.id_kredytu).ToList().ForEach(s => { s.kwota_najbliszej_raty = item.kwota_raty; s.termin_splaty = item.termin_splaty; });
                    }

                    Session["kredyty"] = info;
                    Session["lokaty"] = l;
                }
                
                Session["konta"] = acc;
                Session["transakcje"] = new transakcje_rozszerzone().get_transactions(kl.id_klienta, acc,false);
                return View();
            }   
            else
            {
                TempData["title"] = "Brak dostępu";
                TempData["message"] = "Aby przejść do tej strony musisz się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login");
            }
        }
        public ActionResult Wyloguj()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult Registration()
        {
            if (Session["klient"] != null)
                return RedirectToAction("PanelKlienta");
            else
                return View();
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }




       
    }
}