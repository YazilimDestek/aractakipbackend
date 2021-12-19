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
    public class EngineController : BaseController
    {
        public EngineController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Engine>> GetAll()
        {
            var response = new Result<Engine> { Meta = new Meta() };
            try
            {
                var engines = context.Engines.Where(x => x.IsDeleted == false).ToList();
                response.Entities = engines;
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
        public ActionResult<Result<Engine>> GetById(int id)
        {
            var response = new Result<Engine> { Meta = new Meta() };
            try
            {
                var engine = context.Engines.FirstOrDefault(x => x.Id == id);
                response.Entity = engine;
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
        public ActionResult<Result<Engine>> Create([FromBody] EngineModel engineModel)
        {
            var response = new Result<Engine> { Meta = new Meta() };

            try
            {
                var engine = new Engine
                {
                    Name = engineModel.Name,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.Engines.Add(engine);
                context.SaveChanges();
                response.Entity = engine;
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
        public ActionResult<Result<Engine>> Update(int id, [FromBody] EngineModel engineModel)
        {
            var response = new Result<Engine> { Meta = new Meta() };

            try
            {
                var engine = context.Engines.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (engine != null)
                {
                    engine.Name = engineModel.Name;
                    engine.UpdatedDateTime = DateTime.Now;
                    engine.UpdatedUserId = GetUserId();


                    context.Entry(engine).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = engine;
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
        public ActionResult<Result<Engine>> Delete(int id)
        {

            var response = new Result<Engine> { Meta = new Meta() };
            try
            {
                var engine= context.Engines.FirstOrDefault(c => c.Id == id);
                engine.IsDeleted = true;
                context.Entry(engine).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = engine;

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