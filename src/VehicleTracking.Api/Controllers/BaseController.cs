using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public readonly VehicleTrackingDbContext context;
        
        public BaseController(VehicleTrackingDbContext dbContext)
        {
            context = dbContext;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
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
        
        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetUserId()
        {
            return int.Parse(User.Claims.First(c => c.Type == "UserID").Value);
        }

        [ApiExplorerSettings(IgnoreApi=true)]
        public User GetUser()
        {
            return JsonConvert.DeserializeObject<User>(User.Claims.First(c => c.Type == "user").Value);
        }
    }
}