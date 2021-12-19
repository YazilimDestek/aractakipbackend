using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleTracking.Entity.Models
{
  public  class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public int? UpdatedUserId { get; set; }
    }
}
