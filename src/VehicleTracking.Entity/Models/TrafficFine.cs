using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class TrafficFine:BaseEntity
    {
        [ForeignKey("VehicleId")]
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        [ForeignKey("StaffId")]
        public int? StaffId { get; set; } // Aracı Kullanan Personel Idsi
        public Staff Staff { get; set; }
   
        public double  Price { get; set; }
        public  string Description { get; set; }
        public DateTime IssueDate { get; set; } // Veriliş Tarihi
        public DateTime LastPaymentDate { get; set; } // Son ödeme tarihi
        public bool IsPayed { get; set; } // Ödendi mi
        public  bool IsReported { get; set; } // Muhasebeye bildirildi mi 


    }
}
