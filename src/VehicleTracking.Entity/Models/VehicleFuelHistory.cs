using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class VehicleFuelHistory : BaseEntity
    {
        [ForeignKey("Vehicle ")]
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public string FuelType { get; set; } // Yakıt türü
        public  int Liter { get; set; } // Litre 
        public  DateTime TakenDate { get; set; } // alindigi tarih
        public long Mileage { get; set; } // Alindigi andaki arac kilometresi

    }

}
