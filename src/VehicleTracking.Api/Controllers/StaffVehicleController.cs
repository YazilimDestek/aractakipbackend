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
    public class StaffVehicleController : BaseController
    {
        public StaffVehicleController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {

        }

        [HttpGet]
        public ActionResult<Result<StaffVehicle>> GettAll()
        {
            var response = new Result<StaffVehicle>() { Meta = new Meta() };
            try
            {
                var staffVehicles = context.StaffVehicles.Include(x => x.Staff).Include(x => x.Vehicle).Where(x => x.IsDeleted == false).ToList();
                response.Entities = staffVehicles;
                response.Meta.IsSuccess = true;

            }
            catch (Exception exception)
            {
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                response.Meta.IsSuccess = false;

            }
            return response;
        }

        [HttpGet("{id}")]
        public ActionResult<Result<StaffVehicle>> GetById(int id)
        {
            var response = new Result<StaffVehicle> { Meta = new Meta() };
            try
            {
                var staffVehicle = context.StaffVehicles.Include(x => x.Staff).Include(x => x.Vehicle).FirstOrDefault(x => x.Id == id);
                response.Entity = staffVehicle;
                response.Meta.IsSuccess = true;

            }
            catch (Exception exception)
            {
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                response.Meta.IsSuccess = false;

            }
            return response;
        }

        [HttpPost]
        public ActionResult<Result<StaffVehicle>> Create([FromBody] StaffVehicleModel staffVehicleModel)
        {
            var response = new Result<StaffVehicle> { Meta = new Meta() };

            try
            {
                var staffVehicle = new StaffVehicle
                {
                    VehicleId = staffVehicleModel.VehicleId,
                    StaffId = staffVehicleModel.StaffId,
                    IsPermanent = staffVehicleModel.IsPermanent,
                    IsDaily = staffVehicleModel.IsDaily,
                    IsWeek = staffVehicleModel.IsWeek,
                    StartDate = staffVehicleModel.StartDate,
                    EndDate = staffVehicleModel.EndDate,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId()
                };

                context.StaffVehicles.Add(staffVehicle);
                context.SaveChanges();

                response.Entity = new StaffVehicle();

                var newVehicle = context.StaffVehicles.Include(x => x.Staff).Include(x => x.Vehicle).FirstOrDefault(x => x.CreatedDateTime == staffVehicle.CreatedDateTime);

                response.Entity.Id = newVehicle.Id;
                response.Entity.StaffId = newVehicle.StaffId;
                response.Entity.Staff = newVehicle.Staff;
                response.Entity.VehicleId = newVehicle.VehicleId;
                response.Entity.Vehicle = newVehicle.Vehicle;
                response.Entity.IsPermanent = newVehicle.IsPermanent;
                response.Entity.IsDaily = newVehicle.IsDaily;
                response.Entity.IsWeek = newVehicle.IsWeek;
                response.Entity.StartDate = newVehicle.StartDate;
                response.Entity.EndDate = newVehicle.EndDate;

                response.Meta.IsSuccess = true;
            }
            catch (Exception exception)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Belenmeyen bir hata oluştu";
            }

            return response;
        }

        [HttpPut("{id}")]
        public ActionResult<Result<StaffVehicle>> Update(int id, [FromBody] StaffVehicleModel staffVehicleModel)
        {
            var response = new Result<StaffVehicle> { Meta = new Meta() };
            try
            {
                var staffVehicle = context.StaffVehicles.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (staffVehicle != null)
                {
                    staffVehicle.StaffId = staffVehicleModel.StaffId;
                    staffVehicle.VehicleId = staffVehicleModel.VehicleId;
                    staffVehicle.StartDate = staffVehicleModel.StartDate;
                    staffVehicle.EndDate = staffVehicleModel.EndDate;
                    staffVehicle.IsDaily = staffVehicleModel.IsDaily;
                    staffVehicle.IsWeek = staffVehicleModel.IsWeek;
                    staffVehicle.IsPermanent = staffVehicleModel.IsPermanent;
                    staffVehicle.UpdatedDateTime = DateTime.Now;
                    staffVehicle.UpdatedUserId = GetUserId();

                    context.Entry(staffVehicle).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new StaffVehicle();

                    staffVehicle = context.StaffVehicles.Include(x => x.Vehicle).Include(x => x.Staff).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = staffVehicle.Id;
                    response.Entity.StaffId = staffVehicle.StaffId;
                    response.Entity.Staff = staffVehicle.Staff;
                    response.Entity.VehicleId = staffVehicle.VehicleId;
                    response.Entity.Vehicle = staffVehicle.Vehicle;
                    response.Entity.IsPermanent = staffVehicle.IsPermanent;
                    response.Entity.IsDaily = staffVehicle.IsDaily;
                    response.Entity.IsWeek = staffVehicle.IsWeek;
                    response.Entity.StartDate = staffVehicle.StartDate;
                    response.Entity.EndDate = staffVehicle.EndDate;

                    response.Meta.IsSuccess = true;
                }
                else
                {
                    response.Meta.IsSuccess = false;
                    response.Meta.Error = "BadRequest.error";
                    response.Meta.ErrorMessage = "Aradığınız kayıt bulunamadı";
                }

            }
            catch (Exception exception)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";

            }
            return response;
        }

        [HttpDelete("{id}")]
        public ActionResult<Result<StaffVehicle>> Delete(int id)
        {
            var response = new Result<StaffVehicle>() { Meta = new Meta() };
            try
            {
                var staffVehicle = context.StaffVehicles.FirstOrDefault(x => x.Id == id);
                staffVehicle.IsDeleted = false;
                context.Entry(staffVehicle).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = staffVehicle;
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
