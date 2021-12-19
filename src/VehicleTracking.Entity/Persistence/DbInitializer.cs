using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HesapCo.Entity.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using VehicleTracking.Entity.Persistence;

namespace HesapCo.Entity.Persistence
{
    public static class VehicleTrackingDbInitializer
    {
        public static void Initialize(VehicleTrackingDbContext context)
        {
            try
            {
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();

                //context.Users.Add(new VehicleTracking.Entity.Models.User { CreatedDateTime = DateTime.Now, Email = "ozge@albatrosyazilim.com", IsAdmin = true, Name = "Özge", Password = CreateMd5("123456"), Surname = "Sürmeli", Username = "ozge", IsDeleted = false });
                //context.SaveChanges();

                //context.WarningTypes.Add(new VehicleTracking.Entity.Models.WarningType { CreatedDateTime = DateTime.Now, CreatedUserId = 1, IsDeleted = false, Name = "Muayene" });
                //context.WarningTypes.Add(new VehicleTracking.Entity.Models.WarningType { CreatedDateTime = DateTime.Now, CreatedUserId = 1, IsDeleted = false, Name = "Servis" });
                //context.WarningTypes.Add(new VehicleTracking.Entity.Models.WarningType { CreatedDateTime = DateTime.Now, CreatedUserId = 1, IsDeleted = false, Name = "Lastik" });
                //context.WarningTypes.Add(new VehicleTracking.Entity.Models.WarningType { CreatedDateTime = DateTime.Now, CreatedUserId = 1, IsDeleted = false, Name = "Sigorta" });
                //context.WarningTypes.Add(new VehicleTracking.Entity.Models.WarningType { CreatedDateTime = DateTime.Now, CreatedUserId = 1, IsDeleted = false, Name = "Ceza" });
                //context.SaveChanges();

            }
            catch (Exception e)
            {
                Console.WriteLine("INITIALIZE EDERKEN HATA OLUŞTU!! " + e.Message);
            }
        }

        public static string CreateMd5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
