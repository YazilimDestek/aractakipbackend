using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        public UsersController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<List<User>> Get()
        {
            var users = context.Users.OrderBy(c => c.Username).ToList();
            return users;
        }

        [HttpPost]
        public ActionResult<Result<User>> Post([FromBody] User user)
        {
            var response = new Result<User>();
            response.Meta = new Meta();
            var requestedUser = GetUser();
            if (requestedUser.IsAdmin == true)
            {
                try
                {
                    user.Id = 0;
                    user.Password = CreateMd5(user.Password);
                    user.CreatedDateTime = DateTime.Now;
                    user.CreatedUserId = requestedUser.Id;
                    context.Users.Add(user);
                    context.SaveChanges();

                    response.Meta.IsSuccess = true;
                    response.Entity = user;

                }
                catch (DbUpdateException exception)
                {
                    response.Meta.IsSuccess = false;
                    if (exception.InnerException != null && exception.InnerException is PostgresException postgresException)
                    {
                        if (int.Parse(postgresException.SqlState) == 23505)
                        {
                            response.Meta.Error = "Dublicated key error";
                            var keySource = postgresException.ConstraintName == "IX_Users_Email" ? "email" : "kullanıcı adı";
                            response.Meta.ErrorMessage = $"{keySource} alanı için böyle bir kayıt zaten var";
                        }
                    }
                }
                catch (Exception exception)
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "unexpected.error";
                    response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                }
            }
            else
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "authority.error";
                response.Meta.ErrorMessage = "Yetkiniz Bulunmamaktadır";

            }
            return response;
        }
    }
}