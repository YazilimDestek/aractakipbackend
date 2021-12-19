using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class VehicleTireHistory:BaseEntity
    {
        [ForeignKey("Tire")]
        public int? TireId { get; set; }
        public Tire Tire { get; set; }
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } 
        public int? HoleOrder { get; set; } // kaçıncı lastik yerine takıldı? sol önden başlayarak saat yönünde sıra numarası.
        public  DateTime? RemovedDate { get; set; } // sokuldugu tarih (null ise sökülmemiş demektir)
        public DateTime InstalledDate { get; set; } // takildigi tarih
        public  string RemovedReason { get; set; } // sokulme nedeni
    }
}
