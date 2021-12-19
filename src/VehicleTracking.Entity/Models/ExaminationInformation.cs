using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
   public class ExaminationInformation:BaseEntity
    {
        [ForeignKey("Vehicle")]
        public int ? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime ExaminationDate { get; set; } // Muayene Tarihi
        public string ExaminationResult { get; set; } // Muayene sonucu 
        public string ExaminationResultDocument { get;set; }// Muayene sonucu evrak id uretebiliriz. file ismi gibi.
    }
}
