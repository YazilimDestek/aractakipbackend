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
    public class VehicleTypeController : BaseController
    {
        public VehicleTypeController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }
        [HttpGet]
        public ActionResult<Result<VehicleType>> GetAll()
        {
            var response = new Result<VehicleType> { Meta = new Meta() };
            try
            {
                var vehicleTypes = context.VehicleTypes.Where(x => x.IsDeleted == false).ToList();
                response.Entities = vehicleTypes;
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
        public ActionResult<Result<VehicleType>> GetById(int id)
        {
            var response = new Result<VehicleType> { Meta = new Meta() };
            try
            {
                var vehicleType = context.VehicleTypes.FirstOrDefault(x => x.Id == id);
                response.Entity = vehicleType;
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
        [HttpPost("create")]
        public ActionResult<Result<VehicleType>> Create([FromBody] VehicleTypeModel vehicleTypeModel)
        {
            var response = new Result<VehicleType> { Meta = new Meta() };

            try
            {
                var vehicleType = new VehicleType
                {
                    Name = vehicleTypeModel.Name,
                    SubType = vehicleTypeModel.SubType,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.VehicleTypes.Add(vehicleType);
                context.SaveChanges();
                response.Entity = vehicleType;
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
        public ActionResult<Result<VehicleType>> Update(int id, [FromBody] VehicleTypeModel vehicleTypeModel)
        {
            var response = new Result<VehicleType> { Meta = new Meta() };

            try
            {
                var vehicleType = context.VehicleTypes.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (vehicleType != null)
                {
                    vehicleType.Name = vehicleTypeModel.Name;
                    vehicleType.UpdatedDateTime = DateTime.Now;
                    vehicleType.UpdatedUserId = GetUserId();


                    context.Entry(vehicleType).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = vehicleType;
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
        public ActionResult<Result<VehicleType>> Delete(int id)
        {

            var response = new Result<VehicleType> { Meta = new Meta() };
            try
            {
                var vehicleType = context.VehicleTypes.FirstOrDefault(c => c.Id == id);
                vehicleType.IsDeleted = true;
                context.Entry(vehicleType).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = vehicleType;

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