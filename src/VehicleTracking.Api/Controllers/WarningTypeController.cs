using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarningTypeController : BaseController
    {
        public WarningTypeController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<WarningType>> GetAll()
        {
            var response = new Result<WarningType> { Meta = new Meta() };
            try
            {
                var warningtypes = context.WarningTypes.Where(x => x.IsDeleted == false).ToList();
                response.Entities = warningtypes;
                response.Meta.IsSuccess = true;

            }
            catch (Exception exception)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";

            }

            return response;

        }
        [HttpGet("{id}")]
        public ActionResult<Result<WarningType>> GetById(int id)
        {
            var response = new Result<WarningType> { Meta = new Meta() };
            try
            {
                var warningType = context.WarningTypes.FirstOrDefault(x => x.Id == id);
                response.Entity = warningType;
                response.Meta.IsSuccess = true;

            }
            catch (Exception exception)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";

            }

            return response;

        }
    }
}
