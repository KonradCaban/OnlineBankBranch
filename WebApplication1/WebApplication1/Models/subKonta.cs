using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class subKonta
    {
        public int id_konta { get; set; }
        public string numer_konta { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal? stan_konta { get; set; }

        public decimal? limit_d { get; set; }

        public decimal? limit_m { get; set; }

        public string nazwa_typu { get; set; }

        public List<subKonta> get_sub(int id)
        {
            using (DB_Entities db = new DB_Entities())
            {
                List<subKonta> roles = db.Kontas.Join(db.Typy_Konta, k => k.typ_konta, typ => typ.id_typu, (k, typ) => new { Konta = k, Typy = typ })
                                        .Where(r => r.Konta.id_klienta == id)
                                        .Select(r => new subKonta { id_konta = r.Konta.id_konta, nazwa_typu = r.Typy.nazwa_typu, numer_konta = r.Konta.numer_konta, stan_konta = r.Konta.stan_konta, limit_d = r.Konta.limit_d, limit_m = r.Konta.limit_m })
                                        .ToList();
                return roles;
            }

        }


    }


}