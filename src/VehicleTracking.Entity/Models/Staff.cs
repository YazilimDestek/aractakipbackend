using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Staff : BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string AbysCode { get; set; }
        public bool IsTrackingEnable { get; set; }


        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User User { get; set; }
    }
}