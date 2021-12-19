using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Warning : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DaysLeft { get; set; } // kaç gün önce uyarı çıkacağı
        public string WarningMethod { get; set; } // uyarı tipi

        [ForeignKey("WarningType")]
        public int? WarningTypeId { get; set; }
        public WarningType WarningType { get; set; }

    }
}
