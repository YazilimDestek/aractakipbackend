using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class InsuranceModel
    {
        public int? VehicleId { get; set; }
        public string InsuranceType { get; set; } // Sigorta Tipi (Zorunlu Trafik, Kasko)
        public string InsuranceFirm { get; set; } // Sigoarta Firmasi
        public DateTime DateStart { get; set; } // Police Baslnagic
        public DateTime DateEnd { get; set; } // Police bitis
    }
}
