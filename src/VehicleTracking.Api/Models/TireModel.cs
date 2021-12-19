using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.Entity.Models;

namespace VehicleTracking.Api.Models
{
    public class TireModel : BaseEntity
    {
        public string Brand { get; set; } // Markasi
        public string Model { get; set; } // Modeli
        public string Figure { get; set; } // Deseni
        public string SerialNumber { get; set; } // Seri Numarasi
        // Bu ozellikler de olabilir.
        public int RimDiameter { get; set; } // Jant Capi
        public int Width { get; set; } // Genislik
        public int Height { get; set; } // Yukseklik
        public int MadeYear { get; set; } //üretim yılı
        public int MadeWeek { get; set; } // Üretim Haftası
        public int MaxMileage { get; set; } // Max kilometre
    }
}
