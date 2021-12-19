using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuelTankController : BaseController
    {
        public FuelTankController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<FuelTank>> GetAll()
        {
            var response = new Result<FuelTank> { Meta = new Meta() };
            try
            {
                var fuelTanks = context.FuelTanks.Where(x => x.IsDeleted == false).ToList();
                response.Entities = fuelTanks;
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
        public ActionResult<Result<FuelTank>> GetById(int id)
        {
            var response = new Result<FuelTank> { Meta = new Meta() };
            try
            {
                var fuelTank = context.FuelTanks.FirstOrDefault(x => x.Id == id);
                response.Entity = fuelTank;
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
        public ActionResult<Result<FuelTank>> Create([FromBody] FuelTankModel fuelTankModel)
        {
            var response = new Result<FuelTank> { Meta = new Meta() };

            try
            {
                var fuelTank = new FuelTank
                {
                    Name = fuelTankModel.Name,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.FuelTanks.Add(fuelTank);
                context.SaveChanges();
                response.Entity = fuelTank;
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
        public ActionResult<Result<FuelTank>> Update(int id, [FromBody]FuelTankModel fuelTankModel)
        {
            var response = new Result<FuelTank> { Meta = new Meta() };

            try
            {
                var fuelTank = context.FuelTanks.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (fuelTank != null)
                {
                    fuelTank.Name = fuelTankModel.Name;
                    fuelTank.UpdatedDateTime = DateTime.Now;
                    fuelTank.UpdatedUserId = GetUserId();


                    context.Entry(fuelTank).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = fuelTank;
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
        public ActionResult<Result<FuelTank>> Delete(int id)
        {

            var response = new Result<FuelTank> { Meta = new Meta() };
            try
            {
                var fuelTank = context.FuelTanks.FirstOrDefault(c => c.Id == id);
                fuelTank.IsDeleted = true;
                context.Entry(fuelTank).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = fuelTank;

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