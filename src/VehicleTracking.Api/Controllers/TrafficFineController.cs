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
    public class TrafficFineController : BaseController
    {
        public TrafficFineController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<TrafficFine>> GetAll()
        {
            var response = new Result<TrafficFine> { Meta = new Meta() };
            try
            {
                var trafficFines = context.TrafficFines.Include(x => x.Vehicle).Include(x=>x.Staff).Where(x => x.IsDeleted == false).ToList();
                response.Entities = trafficFines;
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
        public ActionResult<Result<TrafficFine>> GetById(int id)
        {
            var response = new Result<TrafficFine> { Meta = new Meta() };
            try
            {
                var trafficFine = context.TrafficFines.Include(x=>x.Vehicle).Include(x=>x.Staff).FirstOrDefault(x => x.Id == id);
                response.Entity = trafficFine;
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
        public ActionResult<Result<TrafficFine>> Create([FromBody] TrafficFineModel trafficFineModel)
        {
            var response = new Result<TrafficFine> { Meta = new Meta() };

            try
            {
                var trafficFine = new TrafficFine
                {
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),

                    Price = trafficFineModel.Price,
                    Description = trafficFineModel.Description,
                    IssueDate = trafficFineModel.IssueDate,
                    LastPaymentDate = trafficFineModel.LastPaymentDate,
                    VehicleId = trafficFineModel.VehicleId,
                    StaffId = trafficFineModel.StaffId,
                    IsPayed = trafficFineModel.IsPayed,
                    IsReported = trafficFineModel.IsReported
                };
                context.TrafficFines.Add(trafficFine);
                context.SaveChanges();
                response.Entity = trafficFine;
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
        public ActionResult<Result<TrafficFine>> Update(int id, [FromBody]  TrafficFineModel trafficFineModel)
        {
            var response = new Result<TrafficFine> { Meta = new Meta() };

            try
            {
                var trafficFine = context.TrafficFines.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (trafficFine != null)
                {
                    trafficFine.UpdatedDateTime = DateTime.Now;
                    trafficFine.UpdatedUserId = GetUserId();

                    trafficFine.Price = trafficFineModel.Price;
                    trafficFine.Description = trafficFineModel.Description;
                    trafficFine.IssueDate = trafficFineModel.IssueDate;
                    trafficFine.LastPaymentDate = trafficFineModel.LastPaymentDate;
                    trafficFine.IsPayed = trafficFineModel.IsPayed;
                    trafficFine.IsReported = trafficFineModel.IsReported;
                    trafficFine.VehicleId= trafficFineModel.VehicleId;
                    trafficFine.StaffId = trafficFineModel.StaffId;

                    context.Entry(trafficFine).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = trafficFine;
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
        public ActionResult<Result<TrafficFine>> Delete(int id)
        {

            var response = new Result<TrafficFine> { Meta = new Meta() };
            try
            {
                var trafficFine = context.TrafficFines.FirstOrDefault(c => c.Id == id);
                trafficFine.IsDeleted = true;
                context.Entry(trafficFine).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = trafficFine;

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