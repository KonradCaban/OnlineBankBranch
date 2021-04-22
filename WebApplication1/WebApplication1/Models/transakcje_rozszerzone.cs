using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class transakcje_rozszerzone
    {
        [Key]
        public int id_transakcji { get; set; }
        public string nazwa_kategorii { get; set; }
        public DateTime? data { get; set; }
        public string tytul { get; set; }
        public string numer_konta_odbiorca { get; set; }
        public string numer_konta_nadawca { get; set; }
        public decimal kwota { get; set; }

        public string dane_odbiorcy { get; set; }

        public string dane_nadawcy { get; set; }


        public List<transakcje_rozszerzone> get_transactions (int id,List<subKonta> acc,bool all)
        {
            List<String> list2 = new List<String>();
            foreach (var item in acc)
            {
                list2.Add(item.numer_konta);
            }
            if (all == false)
            {
                using (DB_Entities db = new DB_Entities())
                {
                    List<transakcje_rozszerzone> tmp = db.Transakcjes.Join(db.typy_transakcji, t => t.typ_transakcji, k => k.id_typu, (t, k) => new { l = t, r = k })
                                    .Join(db.kategories, t => t.l.kategoria, k => k.id_kategorii, (t, k) => new { l = t, r = k })
                                    .Where(r => (list2.Contains(r.l.l.od_kogo) || list2.Contains(r.l.l.do_kogo)) && r.l.l.wykonane == true)
                                    .Select(r => new transakcje_rozszerzone { id_transakcji = r.l.l.id_transakcji, dane_nadawcy = r.l.l.dane_osobowe_nadawca, dane_odbiorcy = r.l.l.dane_osobowe, data = r.l.l.data_transakcji, kwota = r.l.l.kwota, nazwa_kategorii = r.r.nazwa_kategorii, numer_konta_nadawca = r.l.l.od_kogo, numer_konta_odbiorca = r.l.l.do_kogo, tytul = r.l.l.tytul })
                                    .OrderByDescending(r => r.data).ThenByDescending(r=>r.id_transakcji)
                                    .Take(10).ToList();

                    tmp.Reverse();
                    return tmp;
                }
            }
            else
            {
                using (DB_Entities db = new DB_Entities())
                {
                    List<transakcje_rozszerzone> tmp = db.Transakcjes.Join(db.typy_transakcji, t => t.typ_transakcji, k => k.id_typu, (t, k) => new { l = t, r = k })
                                    .Join(db.kategories, t => t.l.kategoria, k => k.id_kategorii, (t, k) => new { l = t, r = k })
                                    .Where(r => (list2.Contains(r.l.l.od_kogo) || list2.Contains(r.l.l.do_kogo)) && r.l.l.wykonane == true)
                                    .Select(r => new transakcje_rozszerzone { id_transakcji = r.l.l.id_transakcji, dane_nadawcy = r.l.l.dane_osobowe_nadawca, dane_odbiorcy = r.l.l.dane_osobowe, data = r.l.l.data_transakcji, kwota = r.l.l.kwota, nazwa_kategorii = r.r.nazwa_kategorii, numer_konta_nadawca = r.l.l.od_kogo, numer_konta_odbiorca = r.l.l.do_kogo, tytul = r.l.l.tytul })
                                    .OrderBy(r => r.data).ThenBy(r => r.id_transakcji)
                                    .ToList();
                    return tmp;
                }
            }
        }
        
    }
}