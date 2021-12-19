using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Tire:BaseEntity
    {
        public string Brand { get; set; } // Markasi
        public string Model { get; set; } // Modeli
        public string Figure { get; set; } // Deseni
        public string SerialNumber { get; set; } // Seri Numarasi
        // Bu ozellikler de olabilir.
        public int RimDiameter { get; set; } // Jant Capi
        public int Width { get; set; } // Genislik
        public int Height { get; set; } // Yukseklik
        public int MadeYear { get; set; } // üretim yılı
        public int MadeWeek { get; set; } // Üretim Haftası
        public int MaxMileage { get; set; } //Max kilometre

    }
}
