using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RegisterController : Controller
    {
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Home");
        }
        // GET: Register
 

        public ActionResult Reset()
        {
            return View();
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Reset(Klient kl)
        {
            DB_Entities db = new DB_Entities();
            var tmp = db.Kliencis.FirstOrDefault(p => p.email.Equals(kl.email));
            if (tmp == null)
            {
                TempData["title"] = "Błąd";
                TempData["message"] = "W naszym serwisie nie istnieje konto dla podanego adresu e-mail";
                TempData["type"] = "error";
                return View();
            }
            Guid g = Guid.NewGuid();

            var body = "<p>Witaj!</p>";
            body += "<br /><br />Kliknij poniższy link aby zresetować swoje hasło.";
            body += "<br /><a href ='https://localhost:44314/Register/ResetConf/reset{0}'>Kliknij tutaj</a>";
            var message = new MailMessage();
            message.To.Add(new MailAddress(tmp.email));
            message.From = new MailAddress("bot@bankswinka.com");
            message.Subject = "Reset hasła";
            message.IsBodyHtml = true;
            message.Body = string.Format(body, g.ToString());
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

            tokeny t = new tokeny { email = tmp.email, kod = "reset" + g.ToString() };
            db.tokenies.Add(t);
            db.SaveChanges();
            TempData["title"] = "Sprawdź e-mail";
            TempData["message"] = "Na adres e-mail została wysłana wiadomość z linkiem resetującym hasło";
            TempData["type"] = "success";

            return RedirectToAction("Login", "Home");
        }



        public ActionResult ResetConf()
        {

            if (RouteData.Values["id"] != null)
            {
                string resetCode = RouteData.Values["id"].ToString();
                DB_Entities db = new DB_Entities();
                var tmp = db.tokenies.FirstOrDefault(p => p.kod.Equals(resetCode));
                if (tmp != null)
                {
                    var cl = db.Kliencis.FirstOrDefault(p => p.email.Equals(tmp.email));
                    ViewBag.id = cl.id_klienta;
                    db.tokenies.Remove(tmp);
                    db.SaveChanges();
                    return View();
                }
                else
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Błędny token resetujący";
                    TempData["type"] = "error";
                    return RedirectToAction("Login", "Home");
                }
            }
            TempData["title"] = "Błąd";
            TempData["message"] = "Brak dostępu";
            TempData["type"] = "error";
            return RedirectToAction("Login", "Home");
        }

        public ActionResult ChangePassword(Klienci kl,string password_repeat,string id)
        {

            using (MD5 md5Hash = MD5.Create())
            {
                if (kl.haslo.Equals(password_repeat))
                {
                    using (DB_Entities db = new DB_Entities())
                    {
                        int tmp_id = Int32.Parse(id);
                        var client = db.Kliencis.FirstOrDefault(p => p.id_klienta == tmp_id);
                        if (client == null)
                        {
                            TempData["title"] = "Błąd";
                            TempData["message"] = "Użytkownik nie istnieje w naszej bazie danych";
                            TempData["type"] = "error";
                            return HttpNotFound();
                        }
                        client.haslo = new RegisterController().GetMd5Hash(md5Hash, kl.haslo);
                        db.SaveChanges();
                        TempData["title"] = "Ustawiono nowe hasło";
                        TempData["message"] = "Możesz teraz zalogować się do serwisu używając nowych danych";
                        TempData["type"] = "success";
                        return RedirectToAction("Login","Home");

                    }

                }
                else
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Podane nowe hasła nie pasują do siebie";
                    TempData["type"] = "error";
                }
            }

            return RedirectToAction("Login", "Home");
        }


        public async System.Threading.Tasks.Task<ActionResult> Activation()
        {
            TempData["title"] = "Błąd";
            TempData["message"] = "Zły kod aktywujący";
            TempData["type"] = "error";
            if (RouteData.Values["id"] != null)
            {
                string activationCode = RouteData.Values["id"].ToString();
                DB_Entities db = new DB_Entities();
                var tmp = db.tokenies.FirstOrDefault(p => p.kod.Equals(activationCode));
                if(tmp != null)
                {
                    var cl = db.Kliencis.FirstOrDefault(p => p.email.Equals(tmp.email));
                    cl.blokada = false;
                    db.SaveChanges();



                    String[] kody = create_codes(cl.id_klienta);
                    Session["kody"] = kody;
                    byte[] b = generatePDF(kody);

                    var body = "<p>Witaj!:</p>";
                    body += "<br /><br />Twoje konto zostało aktywowane. Możesz teraz zalogować się do serwisu,";
                    body += "<br />Twój login: {0}<br />W załączniku zostały przesłane również kody potwierdzające działania na koncie. Zatrzymaj je w bezpiecznym miejscu!<br/><br/>Bank Świnka";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(cl.email));  // replace with valid value 
                    message.From = new MailAddress("bot@bankswinka.com");  // replace with valid value
                    message.Subject = "Potwierdzenie rejestracji";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body, cl.login);
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






                        db.tokenies.Remove(tmp);
                        db.SaveChanges();
                        TempData["title"] = "Konto zostało aktywowane";
                        TempData["message"] = "Możesz teraz zalogować się do serwisu";
                        TempData["type"] = "success";
                    }
                }
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> RegisterAsync(Klienci obj)
        {
            Session.Clear();
            using (MD5 md5Hash = MD5.Create())
            {
                obj.haslo = GetMd5Hash(md5Hash, obj.haslo);
                obj.login = get_login();
            }
            obj.blokada = true;
            Debug.WriteLine(obj.login.ToString());
        
                if (!check_pesel(obj.pesel, obj.data_ur, obj.plec))
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Błędny numer PESEL";
                    TempData["type"] = "error";
                    return Redirect(Request.UrlReferrer.ToString());
                }
                if(!check_idnum(obj.numer_dowodu))
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "Błędny numer dowodu";
                    TempData["type"] = "error";
                    return Redirect(Request.UrlReferrer.ToString());
                }
                DB_Entities db = new DB_Entities();
                var tmp = db.Kliencis.Where(p => p.email.Equals(obj.email) || p.pesel.Equals(obj.pesel) || p.numer_dowodu.Equals(obj.numer_dowodu) || p.telefon.Equals(obj.telefon))
                    .FirstOrDefault();
                if(tmp != null)
                {
                    TempData["title"] = "Błąd";
                    TempData["message"] = "W naszej bazie danych istnieje już konto dla podanych danych";
                    TempData["type"] = "error";
                    return Redirect(Request.UrlReferrer.ToString());
                }
                db.Kliencis.Add(obj);
                db.SaveChanges();
                var id = obj.id_klienta;
                Guid g = Guid.NewGuid();



                    var body = "<p>Witaj!:</p>";
                body += "<br /><br />Kliknij poniższy link aby aktywować swoje konto.";
                body+= "<br /><a href ='https://localhost:44314/Register/Activation/active{0}'>Kliknij tutaj</a>";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(obj.email)); 
                    message.From = new MailAddress("bot@bankswinka.com");
                    message.Subject = "Aktywacja konta bankowego";
                    message.IsBodyHtml = true;
                    message.Body = string.Format(body,g.ToString());
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
                tokeny t = new tokeny { email = obj.email, kod = "active"+g.ToString() };
                db.tokenies.Add(t);
                db.SaveChanges();
                TempData["title"] = "Sprawdź e-mail";
                TempData["message"] = "Na adres e-mail została wysłana wiadomość z linkiem potwierdzającym rejestrację";
                TempData["type"] = "success";

            

            return RedirectToAction("Login");
        }
        
        private bool check_idnum(String chk)
        {
            int sum = 0;
            int i = 1;
            int[] wages = { 7, 3, 1, 9, 7, 3, 1, 7, 3,0};
            foreach (var item in chk.Zip(wages, (a, b) => new { A = a, B = b }))
            {
                if (i < 4)
                    sum = ((int)item.A - 55) * item.B + sum;
                else
                    sum = (int)Char.GetNumericValue(item.A) * item.B + sum;
                i++;
                
            }
            if (sum % 10 == 0)
                return true;
            else
            {
                Session["msgidnum"] = "Numer dowodu jest nieprawidłowy.";
                    return false ;
            }
        }
        private bool check_pesel(String pesel,DateTime dt,int plec)
        {
            var i = 0;
            int sum = 0;
            String h_y;
            int[] wages = { 9,7,3,1,9,7,3,1,9,7,0};
            var count = pesel.Count();
            string pesel_sub = pesel.Substring(0, 6);
            if (!((int)pesel[9] % 2 == 0 && plec == 2) && !((int)pesel[9] % 2 == 1 && plec == 1))
                {
                Session["msgpesel"] = "Numer PESEL jest nieprawidłowy.";
                return false;
            }

            if (dt.Year < 2000)
                h_y = dt.ToString("yy") + dt.ToString("MM") + dt.ToString("dd");
            else
                h_y = dt.ToString("yy") + (dt.Month+20).ToString() + dt.ToString("dd");
            Debug.WriteLine(pesel_sub + " " + h_y);
            if (dt.Year < 1901 || dt.Year > 2007)
            {
                Session["msgpesel"] = "Rok urodzenia powinien być z zakresu 1901 - 2007";
                return false;
            }
           
            if(!String.Equals(pesel_sub,h_y))
            {
                Session["msgpesel"] = "Numer PESEL jest nieprawidłowy.";
                return false;
            } 
            foreach (var item in pesel.Zip(wages, (a, b) => new { A = a, B = b }))
            {
                
                if (++i == count)
                {
                    if (sum % 10 == (int)Char.GetNumericValue(item.A))
                        return true;
                    else
                    {
                        Session["msgpesel"] = "Numer PESEL jest nieprawidłowy.";
                        return false;
                    }
                }
                else
                {
                    sum += (int)Char.GetNumericValue(item.A) * item.B;
                }
            }
            Session["msgpesel"] = "Numer PESEL jest nieprawidłowy.";
            return false;
        }

        private int get_login()
        {
            int login = 0;
            string cnn = @"Data Source=KOMPUTER;Initial Catalog=bank;User ID=sa;Password=praktyka";

            var con = new System.Data.SqlClient.SqlConnection(cnn);
            var cmd = new System.Data.SqlClient.SqlCommand("select dbo.generate_login()", con);
            
            con.Open();

            login =Convert.ToInt32( cmd.ExecuteScalar());
            Debug.WriteLine(login.ToString());
            con.Close();       
            return login;
        }

        public String[] create_codes(int id)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                Kody_Autoryzujace[] kody = new Kody_Autoryzujace[20];
                String[] codes_array = new String[20];
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random rnd = new Random();
                for (int i = 0; i < 20; i++)
                {
                    string code = new string(Enumerable.Repeat(chars, 6).Select(s => s[rnd.Next(s.Length)]).ToArray());

                    codes_array[i] = code;
                    string hashedcode = GetMd5Hash(md5Hash, code);
                    kody[i] = new Kody_Autoryzujace { kod = hashedcode, id_klienta = id, numer_kodu = i+1, aktywny = true };
                }
                DB_Entities db = new DB_Entities();
                for (int j = 0; j < 20; j++)
                {
                     db.Kody_Autoryzujace.Add(kody[j]);
                }
                  db.SaveChanges();

                return codes_array;          
             }
        }


         public string GetMd5Hash(MD5 md5Hash, string input)
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

        public byte[] generatePDF(string[] codes)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            Document document = new Document(PageSize.A4, 10, 10, 10, 10);

            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();
            Paragraph paragraph = new Paragraph("Twoje Kody Autoryzacyjne:");
            document.Add(paragraph);
            for (int i = 1; i <= codes.Length; i++)
            {
                paragraph = new Paragraph(i.ToString() + " " + codes[i - 1]);
                document.Add(paragraph);
            }

            document.Close();
            return memoryStream.ToArray();
            
        }

    }
}