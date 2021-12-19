using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class VehicleModel
    {
        public int? BrandId { get; set; } // Markası
        public int? ModelId { get; set; } // Modeli
        public int? EngineId { get; set; } // Motoru 
        public int? FuelTankId { get; set; } // Yakit Tanki
        public int? HydraulicTankId { get; set; } // Hidrolik Tanki
        public int ModelYear { get; set; } // Model Yılı
        public int NumberOfPeople { get; set; } // Kisi Satyisi
        public DateTime WarrantyEndDate { get; set; } // Garanti bitis tarihi
        public long InstantMileage { get; set; } // Anlık Kilometre veya Kullanım Saati
        public bool IsGpsTracking { get; set; } // Gps Takibi Yapiliyor mu
        public int Tonnage { get; set; } // Tonnaji
        public string UsageType { get; set; } // Kullanim Tipi
        public string OwnershipType { get; set; } // Sahiplik Tipi 
        public int? VehicleTypeId { get; set; } // Arac Tipi
        public string PlateNumber { get; set; } // plaka numarası
        public DateTime? FirstRegisterDateTime { get; set; } // ilk tescil / trafiğe çıkış tarihi
        public DateTime? RegisterDateTime { get; set; } // tescil tarihi
        public string RegisterPaperSerialNumber { get; set; } // ruhsat seri numarası
        public string RegisterPaperOrderNumber { get; set; } // ruhsat sıra numarası
        public string CaseNumber { get; set; } // şase numarası
        public string EngineNumber { get; set; } // motor numarası
        public int TireCount { get; set; } // araca takılı lastik sayısı
    }
}
