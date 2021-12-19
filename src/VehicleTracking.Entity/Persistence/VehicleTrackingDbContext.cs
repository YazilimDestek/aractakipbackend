using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VehicleTracking.Entity.Models;

namespace VehicleTracking.Entity.Persistence
{
    public class VehicleTrackingDbContext : DbContext
    {
        public IConfiguration _configuration;

        public VehicleTrackingDbContext(DbContextOptions<VehicleTrackingDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(300);
        }

        public VehicleTrackingDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
            });

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }  // arac
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<FuelTank> FuelTanks { get; set; } 
        public DbSet<HydraulicTank> HydraulicTanks { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Tire> Tires { get; set; }
        public DbSet<VehicleFuelHistory> VehicleFuelHistories { get; set; } // Yakit gecmisi
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Maintenance> Maintenance { get; set; }  // Bakim 
        public DbSet<VehicleTireHistory> VehicleTireHistories { get; set; } // Arac lastik gecmisi
        public DbSet<Insurance> Insurances { get; set; } // Sigorta
        public DbSet<MaintenanceResult> MaintenanceResults { get; set; } // Bakim sonucu
        public DbSet<Examination> Examinations { get; set; } // Muayene araliklari
        public DbSet<ExaminationInformation> ExaminationsInformation { get; set; } // Muayene Bilgileri.
        public DbSet<TrafficFine> TrafficFines { get; set; } // Trafik Cezalari
        public DbSet<Staff> Staffs { get; set; } // Çalışanlar (sürücüler dahil)
        public DbSet<StaffVehicle> StaffVehicles { get; set; } // Çalışanlara atanan araçlar
        public DbSet<Warning> Warnings { get; set; } // Uyarı Tanımları
        public DbSet<WarningType> WarningTypes { get; set; } // Uyarı Tipleri

    }
}
