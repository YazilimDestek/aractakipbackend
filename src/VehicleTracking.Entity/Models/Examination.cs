using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
   public  class Examination:BaseEntity
    {
        [ForeignKey("VehicleType")]
        public  int? VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; }
        public long Mileage { get; set; }
    }
}
