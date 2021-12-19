using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class HydraulicTankController : BaseController
    {
        public HydraulicTankController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }
        [HttpGet]
        public ActionResult<Result<HydraulicTank>> GetAll()
        {
            var response = new Result<HydraulicTank> { Meta = new Meta() };
            try
            {
                var hydraulicTanks = context.HydraulicTanks.Where(x => x.IsDeleted == false).ToList();
                response.Entities = hydraulicTanks;
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
        public ActionResult<Result<HydraulicTank>> GetById(int id)
        {
            var response = new Result<HydraulicTank> { Meta = new Meta() };
            try
            {
                var hydraulicTank= context.HydraulicTanks.FirstOrDefault(x => x.Id == id);
                response.Entity = hydraulicTank;
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

        [HttpPost]
        public ActionResult<Result<HydraulicTank>> Create([FromBody] HydraulicTankModel hydraulicTankModel)
        {
            var response = new Result<HydraulicTank> { Meta = new Meta() };

            try
            {
                var hydraulicTank= new HydraulicTank
                {
                    Name = hydraulicTankModel.Name,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.HydraulicTanks.Add(hydraulicTank);
                context.SaveChanges();
                response.Entity = hydraulicTank;
                response.Meta.IsSuccess = true;

            }
            catch (Exception e)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
            }

            return response;

        }

        [HttpPut("update/{id}")]
        public ActionResult<Result<HydraulicTank>> Update(int id, [FromBody] HydraulicTankModel hydraulicTankModel)
        {
            var response = new Result<HydraulicTank> { Meta = new Meta() };

            try
            {
                var hydraulicTank= context.HydraulicTanks.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (hydraulicTank != null)
                {
                    hydraulicTank.Name = hydraulicTankModel.Name;
                    hydraulicTank.UpdatedDateTime = DateTime.Now;
                    hydraulicTank.UpdatedUserId = GetUserId();


                    context.Entry(hydraulicTank).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = hydraulicTank;
                }
                else
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "BadRequest.error";
                    response.Meta.ErrorMessage = "Aradığınız kayıt bulunamadı";
                }

            }
            catch (Exception e)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
            }


            return response;
        }

        [HttpDelete("delete/{id}")]
        public ActionResult<Result<HydraulicTank>> Delete(int id)
        {

            var response = new Result<HydraulicTank> { Meta = new Meta() };
            try
            {
                var hydraulicTank = context.HydraulicTanks.FirstOrDefault(c => c.Id == id);
                hydraulicTank.IsDeleted = true;
                context.Entry(hydraulicTank).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = hydraulicTank;

            }
            catch (Exception ex)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
            }

            return response;
        }
    }
}