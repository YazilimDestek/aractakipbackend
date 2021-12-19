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
    public class WarningController : BaseController
    {
        public WarningController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Warning>> GetAll()
        {
            var response = new Result<Warning> { Meta = new Meta() };
            try
            {
                var warning = context.Warnings.Include(x => x.WarningType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = warning;
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
        public ActionResult<Result<Warning>> GetById(int id)
        {
            var response = new Result<Warning> { Meta = new Meta() };
            try
            {
                var warning = context.Warnings.Include(x => x.WarningType).FirstOrDefault(x => x.Id == id);
                response.Entity = warning;
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
        public ActionResult<Result<Warning>> Create([FromBody] WarningModel warningModel)
        {
            var response = new Result<Warning> { Meta = new Meta() };

            try
            {
                var warning = new Warning
                {
                    Name = warningModel.Name,
                    Description = warningModel.Description,
                    DaysLeft = warningModel.DaysLeft,
                    WarningMethod = warningModel.WarningMethod,
                    WarningTypeId = warningModel.WarningTypeId,

                };
                context.Warnings.Add(warning);
                context.SaveChanges();

                response.Entity = new Warning();

                var newWarning = context.Warnings.Include(x => x.WarningType).FirstOrDefault(x => x.CreatedDateTime == warning.CreatedDateTime);

                response.Entity.Id = newWarning.Id;
                response.Entity.Name = newWarning.Name;
                response.Entity.Description = newWarning.Description;
                response.Entity.DaysLeft = newWarning.DaysLeft;
                response.Entity.WarningMethod = newWarning.WarningMethod;
                response.Entity.WarningTypeId = newWarning.WarningTypeId;
                response.Entity.WarningType = newWarning.WarningType;
                response.Entity.UpdatedUserId = newWarning.UpdatedUserId;
                response.Entity.UpdatedDateTime = newWarning.UpdatedDateTime;

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

        [HttpPost("filter")]
        public ActionResult<Result<Warning>> GetByFilter([FromBody] WarningModel warningModel)
        {
            var response = new Result<Warning> { Meta = new Meta() };

            try
            {
                var warning = context.Warnings.Include(x => x.WarningType).Where(x => x.IsDeleted == false);

                if (warningModel.WarningTypeId > 0)
                {
                    warning = warning.Where(x => x.WarningTypeId == warningModel.WarningTypeId);
                }
                if (!string.IsNullOrEmpty(warningModel.Name))
                {
                    warning = warning.Where(x => x.Name == warningModel.Name);
                }
                if (warningModel.DaysLeft > 0)
                {
                    warning = warning.Where(x => x.DaysLeft == warningModel.DaysLeft);
                }
                if (!string.IsNullOrEmpty(warningModel.WarningMethod))
                {
                    warning = warning.Where(x => x.WarningMethod == warningModel.WarningMethod);
                }
                response.Entities = warning.ToList();
                response.Meta.IsSuccess = true;
                return response;
            }
            catch
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.exception";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu!";
                return response;
            }
        }

        [HttpPut("{id}")]
        public ActionResult<Result<Warning>> Update(int id, [FromBody] WarningModel warningModel)
        {
            var response = new Result<Warning> { Meta = new Meta() };

            try
            {
                var warning = context.Warnings.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (warning != null)
                {
                    warning.Name = warningModel.Name;
                    warning.Description = warningModel.Description;
                    warning.DaysLeft = warningModel.DaysLeft;
                    warning.WarningMethod = warningModel.WarningMethod;
                    warning.WarningTypeId = warningModel.WarningTypeId;
                    warning.UpdatedDateTime = DateTime.Now;
                    warning.UpdatedUserId = GetUserId();

                    context.Entry(warning).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Warning();

                    warning = context.Warnings.Include(x => x.WarningType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = warning.Id;
                    response.Entity.Name = warning.Name;
                    response.Entity.Description = warning.Description;
                    response.Entity.DaysLeft = warning.DaysLeft;
                    response.Entity.WarningMethod = warning.WarningMethod;
                    response.Entity.WarningTypeId = warning.WarningTypeId;
                    response.Entity.WarningType = warning.WarningType;
                    response.Entity.UpdatedUserId = warning.UpdatedUserId;
                    response.Entity.UpdatedDateTime = warning.UpdatedDateTime;

                    response.Meta.IsSuccess = true;

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

        [HttpDelete("{id}")]
        public ActionResult<Result<Warning>> Delete(int id)
        {

            var response = new Result<Warning> { Meta = new Meta() };
            try
            {
                var warnings = context.Warnings.FirstOrDefault(c => c.Id == id);
                warnings.IsDeleted = true;
                context.Entry(warnings).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = warnings;

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
