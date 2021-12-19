using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Vehicle : BaseEntity
    {
        [ForeignKey("Brand")]
        public int? BrandId { get; set; } // Markası
        public Brand Brand { get; set; }

        [ForeignKey("Model")]
        public int? ModelId { get; set; } // Modeli
        public Model Model { get; set; }

        [ForeignKey("Engine")]
        public int? EngineId { get; set; } // Motoru 
        public Engine Engine { get; set; }

        [ForeignKey("FuelTank")]
        public int? FuelTankId { get; set; } // Yakit Tanki
        public FuelTank FuelTank { get; set; }

        [ForeignKey("HydraulicTank")]
        public int? HydraulicTankId { get; set; } // Hidrolik Tanki
        public HydraulicTank HydraulicTank { get; set; }

        [ForeignKey("VehicleType")]
        public int? VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; } // Arac Tipi

        public int ModelYear { get; set; } // Model Yılı
        public int NumberOfPeople { get; set; } // Kisi Satyisi
        public DateTime WarrantyEndDate { get; set; } // Garanti bitis tarihi
        public long InstantMileage { get; set; } // Anlık Kilometre veya Kullanım Saati
        public bool IsGpsTracking { get; set; } // Gps Takibi Yapiliyor mu
        public int Tonnage { get; set; } // Tonnaji
        public string UsageType { get; set; } // Kullanim Tipi
        public string OwnershipType { get; set; } // Sahiplik Tipi 
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
