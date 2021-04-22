using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{


    public class EditController : Controller
    {

        public ActionResult Login()
        {
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public ActionResult Limity(int id_k, string old_limit_o, string old_limit_d,int? limit_o,int? limit_d,string kod, string kod_check)
        {
            using (DB_Entities db = new DB_Entities())
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(kod_check) && p.aktywny == true);
                    if (!kod_check.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) && k != null)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędny kod autoryzujący";
                        TempData["type"] = "error";
                        return RedirectToAction("DaneKlienta");
                    }

                    int i = Decimal.ToInt32(Convert.ToDecimal(old_limit_o));
                    int j = Decimal.ToInt32(Convert.ToDecimal(old_limit_d));

                    if (!limit_o.HasValue)
                        limit_o = i;


                    if (!limit_d.HasValue)
                        limit_d = j;

                    var konto = db.Kontas.FirstOrDefault(p => p.id_konta == id_k);
                    if (konto == null)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Konto nie istnieje";
                        TempData["type"] = "error";
                        return RedirectToAction("DaneKlienta");
                    }
                    konto.limit_d = limit_o;
                    konto.limit_m = limit_d;
                    k.aktywny = false;
                    db.SaveChanges();
                    TempData["title"] = "Zaaktualizowano limity";
                    TempData["message"] = "Pomyślnie zaaktualizowano Twoje limity";
                    TempData["type"] = "success";
                    return RedirectToAction("DaneKlienta");
                }
            }
        }
        public ActionResult DaneKlienta()
        {

            if (Session["Klient"] != null)
            {
                using (DB_Entities db = new DB_Entities())
                {
                    Klient tmp = (Klient)Session["Klient"];
                    Kod code = db.Kody_Autoryzujace.Where(p => p.id_klienta.Equals(tmp.id_klienta) && p.aktywny == true)
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



                    Session["kod"] = code;
                    Session["konta"] = new subKonta().get_sub(tmp.id_klienta);
                    return View(tmp);
                }
            }
            else
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do tej strony należy się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
        }
        [HttpPost]
        public ActionResult Haslo(string old_password, Klienci kl,string new_password_r, string kod, string kod_check)
        {

            using (MD5 md5Hash = MD5.Create())
            {
                using (DB_Entities db = new DB_Entities())
                {
                    Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(kod_check) && p.aktywny == true);
                    if (!kod_check.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) && k != null)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nie poprawny kod autoryzujący";
                        TempData["type"] = "error";
                        return RedirectToAction("DaneKlienta");
                    }
                    old_password = new RegisterController().GetMd5Hash(md5Hash, old_password);
                    Klient tmp = (Klient)Session["Klient"];
                    if (!tmp.haslo.Equals(old_password))
                    {

                        TempData["title"] = "Błąd";
                        TempData["message"] = "Podałeś błędne stare hasło";
                        TempData["type"] = "error";
                        return RedirectToAction("DaneKlienta");
                    }
                    if(!kl.haslo.Equals(new_password_r))
                    {


                        TempData["title"] = "Błąd";
                        TempData["message"] = "Nowe hasła nie zgadzają się";
                        TempData["type"] = "error";
                        return RedirectToAction("DaneKlienta");
                    }
              
                    var client = db.Kliencis.FirstOrDefault(p => p.id_klienta == tmp.id_klienta);
                    if (client == null)
                    {
                        TempData["title"] = "Błąd";
                        TempData["message"] = "Aby uzyskać dostęp do tej strony należy się zalogować";
                        TempData["type"] = "error";
                        return RedirectToAction("Login","Home");
                    }
                    client.haslo = new RegisterController().GetMd5Hash(md5Hash, kl.haslo);
                    k.aktywny = false;
                    db.SaveChanges();
                    load_client(tmp.id_klienta);
                    TempData["title"] = "Sukces";
                    TempData["message"] = "Pomyślnie zmieniono hasło";
                    TempData["type"] = "success";
                    return RedirectToAction("DaneKlienta");

                }
            }

           
        }

        public List<Kod> get_codes(int id_klienta)
        {
            using (DB_Entities db = new DB_Entities())
            {
                List<Kod> codes = new List<Kod>();
                codes = db.Kody_Autoryzujace.Select(r => new Kod { kod = r.kod, numer_kodu = r.numer_kodu, id_klienta = r.id_klienta, aktywny = r.aktywny })
                    .Where(r => r.id_klienta == id_klienta && r.aktywny == true)
                    .ToList();
                return codes;
            }

        }
        [HttpPost]
        public ActionResult DaneKlienta(Klienci kl,string kod,string kod_check)
        {
            try
            {
                MD5 md5Hash = MD5.Create();
                DB_Entities db = new DB_Entities();
                Kody_Autoryzujace k = db.Kody_Autoryzujace.FirstOrDefault(p => p.kod.Equals(kod_check) && p.aktywny == true);
                if (!kod_check.Equals(new RegisterController().GetMd5Hash(md5Hash, kod)) || k == null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Podałeś błędny kod autoryzacyjny";
                    TempData["type"] = "error";
                    return RedirectToAction("DaneKlienta");
                }
                if (db.Kliencis.FirstOrDefault(p=>p.email==kl.email && p.id_klienta!=kl.id_klienta) != null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Istnieje już konto o podanym adresie e-mail w naszej bazie danych";
                    TempData["type"] = "error";
                    return View();
                }
                if (db.Kliencis.FirstOrDefault(p => p.telefon == kl.telefon && p.id_klienta != kl.id_klienta) != null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Istnieje już konto z podanym numerem telefonu w naszej bazie danych";
                    TempData["type"] = "error";
                    return View();
                }
                db.Kliencis.Attach(kl);
                db.Entry(kl).Property(x => x.nazwisko).IsModified = true;
                db.Entry(kl).Property(x => x.email).IsModified = true;
                db.Entry(kl).Property(x => x.telefon).IsModified = true;
                k.aktywny = false;
                db.SaveChanges();
                Klient tmp = (Klient)Session["Klient"];
                load_client(tmp.id_klienta);
                TempData["title"] = "Sukces";
                TempData["message"] = "Pomyślnie zaktualizowano dane";
                TempData["type"] = "success";
                return View();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
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

        }
        public void load_client(int id)
        {
            DB_Entities db = new DB_Entities();
            var o = db.Kliencis.Where(a => a.id_klienta.Equals(id)).FirstOrDefault();
            if (o != null)
            {
                Klient cl = new Klient(o.id_klienta, o.login.ToString(), o.haslo.ToString(), o.imie.ToString(), o.nazwisko.ToString(), o.pesel.ToString(), o.numer_dowodu.ToString(), o.telefon.ToString(), o.email.ToString(), o.data_ur, (bool)o.blokada, o.plec);
                Session["Klient"] = cl;
            }
        }

        public ActionResult Kody()
        {
            if(Session["Klient"] == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "Aby uzyskać dostęp do podanej strony należy się zalogować";
                TempData["type"] = "error";
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Kody(Klienci kl)
        {
            Klient tmp = (Klient)Session["Klient"];

            using (MD5 md5Hash = MD5.Create())
            {
                DB_Entities db = new DB_Entities();
                string haslo = new RegisterController().GetMd5Hash(md5Hash, kl.haslo);
                if (tmp.haslo.Equals(haslo))
                {
                    var codes = db.Kody_Autoryzujace.Where(p => p.id_klienta == tmp.id_klienta).ToList();
                    codes.ForEach(p => p.aktywny = false);
                    db.SaveChanges();
                    String[] kody = new RegisterController().create_codes(tmp.id_klienta);
                    Session["kody"] = kody;
                    byte[] b = new RegisterController().generatePDF(kody);


                    var body = "<p>Witaj!:</p>";
                    body += "<br />Poprosiłeś o wygenerowanie nowych kodów autoryzujących. Przesyłamy je w załączniku. Zatrzymaj je w bezpiecznym miejscu!<br/><br/>Bank Świnka";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(tmp.email));  // replace with valid value 
                    message.From = new MailAddress("bot@bankswinka.com");  // replace with valid value
                    message.Subject = "Nowe kody autoryzacyjne";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body);
                    message.Attachments.Add(new Attachment(new System.IO.MemoryStream(b), "kody_autoryzacyjne.pdf"));
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
                        TempData["title"] = "Sukces";
                        TempData["message"] = "Kody zostały wysłane na twoj adres e-mail";
                        TempData["type"] = "success";
                        return View();



                    }
                }
                TempData["title"] = "Błąd";
                TempData["message"] = "Podałeś błędne hasło";
                TempData["type"] = "error";
                return View();
            }
        }
    }
}