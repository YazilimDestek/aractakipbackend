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
    public class ModelController : BaseController
    {
        public ModelController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Model>> GetAll()
        {
            var response = new Result<Model> { Meta = new Meta() };
            try
            {
                var models = context.Models.Where(x => x.IsDeleted == false).ToList();
                response.Entities = models;
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
        public ActionResult<Result<Model>> GetById(int id)
        {
            var response = new Result<Model> { Meta = new Meta() };
            try
            {
                var model= context.Models.FirstOrDefault(x => x.Id == id);
                response.Entity = model;
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
        public ActionResult<Result<Model>> Create([FromBody] ModelRequest modelRequest)
        {
            var response = new Result<Model> { Meta = new Meta() };

            try
            {
                var model = new Model
                {
                    Name = modelRequest.Name,
                    BrandId = modelRequest.BrandId,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId()
                };
                context.Models.Add(model);
                context.SaveChanges();
                response.Entity = model;
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
        public ActionResult<Result<Model>> Update(int id, [FromBody] ModelRequest modelRequest)
        {
            var response = new Result<Model> { Meta = new Meta() };

            try
            {
                var model= context.Models.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (model != null)
                {
                    model.Name = modelRequest.Name;
                    model.UpdatedDateTime = DateTime.Now;
                    model.UpdatedUserId = GetUserId();

                    if(modelRequest.BrandId > 0)
                    {
                        model.BrandId = modelRequest.BrandId;
                    }

                    context.Entry(model).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = model;
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
        public ActionResult<Result<Model>> Delete(int id)
        {

            var response = new Result<Model> { Meta = new Meta() };
            try
            {
                var model = context.Models.FirstOrDefault(c => c.Id == id);
                model.IsDeleted = true;
                context.Entry(model).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = model;

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