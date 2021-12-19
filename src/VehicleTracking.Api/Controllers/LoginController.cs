using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VehicleTracking.Api.Models;
using VehicleTracking.Api.Security;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseController
    {
        public LoginController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpPost]
        [AllowAnonymous]
        public Result<JwtToken> Login([FromBody]LoginRequest request)
        {
            var response = new Result<JwtToken>();
            response.Meta = new Meta();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "LoginRequest modelstate invalid";
                    response.Meta.ErrorMessage = "Geçersiz form!";
                    return response;
                }

                var user = context.Users.FirstOrDefault(c => c.Username == request.Username && c.Password == CreateMd5(request.Password));
                if (user == null)
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "Username or password is invalid";
                    response.Meta.ErrorMessage = "Kullanıcı adı veya şifre hatalı";
                    return response;
                }
                else if (user.IsDeleted)
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "Your account was deleted";
                    response.Meta.ErrorMessage = "Hesabınız Silinmiştir";
                    return response;
                }
                else
                {
                    var token = new JwtTokenBuilder()
                    .AddSecurityKey(JwtSecurityKey.Create("CINIGAZ2019-92223K-324957-K3596U"))
                    .AddIssuer("192.168.2.70")
                    .AddAudience("192.168.2.70")
                    .AddClaim("user", JsonConvert.SerializeObject(user))
                    .AddClaim("UserID", user.Id.ToString())
                    .AddSubject("entray.com");

                    if (request.Remember == false)
                    {
                        token.AddExpiry(540); // 9 SAAT
                    }
                    else
                    {
                        token.AddExpiry(2000000); // 1388 gün
                    }


                    var tokenWithUserInfo = token.Build();
                    tokenWithUserInfo.User = user;

                    response.Entity = tokenWithUserInfo;
                    response.Meta.IsSuccess = true;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.exception";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu.";
                return response;
            }
        }
    }
}