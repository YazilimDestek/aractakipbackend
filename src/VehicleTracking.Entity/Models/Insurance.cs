using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Insurance:BaseEntity
    {
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public string InsuranceType { get; set; } // Sigorta Tipi (Zorunlu Trafik, Kasko, Makine Kırılması)
        public string InsuranceFirm { get; set; } // Sigoarta Firmasi
        public DateTime DateStart { get; set; } // Police Baslnagic
        public DateTime DateEnd { get; set; } // Police bitis
    }
}
