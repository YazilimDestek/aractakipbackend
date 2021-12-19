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
    public class ExaminationController : BaseController
    {
        public ExaminationController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Examination>> GetAll()
        {
            var response = new Result<Examination> { Meta = new Meta() };
            try
            {
                var examinations = context.Examinations.Include(x => x.VehicleType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = examinations;
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
        public ActionResult<Result<Examination>> GetById(int id)
        {
            var response = new Result<Examination> { Meta = new Meta() };
            try
            {
                var examination = context.Examinations.Include(x => x.VehicleType).FirstOrDefault(x => x.Id == id);
                response.Entity = examination;
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
        public ActionResult<Result<Examination>> Create([FromBody] ExaminationModel examinationModel)
        {
            var response = new Result<Examination> { Meta = new Meta() };

            try
            {
                var examination = new Examination
                {
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                    Mileage = examinationModel.Mileage,
                    VehicleTypeId = examinationModel.VehicleTypeId

                };
                context.Examinations.Add(examination);
                context.SaveChanges();

                response.Entity = new Examination();

                var newExamination = context.Examinations.Include(x=>x.VehicleType).FirstOrDefault(x => x.CreatedDateTime == examination.CreatedDateTime);

                response.Entity.Id = newExamination.Id;
                response.Entity.Mileage = newExamination.Mileage;
                response.Entity.VehicleTypeId = newExamination.VehicleTypeId;
                response.Entity.VehicleType = newExamination.VehicleType;
                response.Entity.CreatedDateTime = newExamination.CreatedDateTime;
                response.Entity.CreatedUserId = newExamination.CreatedUserId;
                response.Entity.Id = newExamination.Id;

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
        public ActionResult<Result<Examination>> GetByFilter([FromBody]ExaminationModel examinationModel)
        {
            var response = new Result<Examination> { Meta = new Meta() };
            try
            {
                var examination = context.Examinations.Include(x => x.VehicleType).Where(x => x.IsDeleted == false);
                if (examinationModel.VehicleTypeId > 0)
                {
                    examination = examination.Where(x => x.VehicleTypeId == examinationModel.VehicleTypeId);
                }
                if (examinationModel.Mileage > 0)
                {
                    examination = examination.Where(x => x.Mileage == examinationModel.Mileage);
                }

                response.Entities = examination.ToList();
                response.Meta.IsSuccess = true;
                return response;
            }
            catch
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return response;
            }
        }

        [HttpPut("update/{id}")]
        public ActionResult<Result<Examination>> Update(int id, [FromBody] ExaminationModel examinationModel)
        {
            var response = new Result<Examination> { Meta = new Meta() };

            try
            {
                var examination = context.Examinations.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (examination != null)
                {
                    examination.UpdatedDateTime = DateTime.Now;
                    examination.UpdatedUserId = GetUserId();
                    examination.Mileage = examinationModel.Mileage;
                    examination.VehicleTypeId = examinationModel.VehicleTypeId;

                    context.Entry(examination).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Examination();

                    examination = context.Examinations.Include(x => x.VehicleType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = examination.Id;
                    response.Entity.VehicleTypeId = examination.VehicleTypeId;
                    response.Entity.VehicleType = examination.VehicleType;
                    response.Entity.Mileage = examination.Mileage;
                    response.Entity.UpdatedDateTime = examination.UpdatedDateTime;
                    response.Entity.UpdatedUserId = examination.UpdatedUserId;

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

        [HttpDelete("delete/{id}")]
        public ActionResult<Result<Examination>> Delete(int id)
        {

            var response = new Result<Examination> { Meta = new Meta() };
            try
            {
                var examination = context.Examinations.FirstOrDefault(c => c.Id == id);
                examination.IsDeleted = true;
                context.Entry(examination).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = examination;

            }
            catch (Exception ex)
            {
                response.Meta.IsSuccess = false;
                response.Meta.Error = "unexpected.error";
                response.Meta.ErrorMessage = "Beklenmeyen bir hata oluştu";
            }
            return response;
        }

        [HttpGet("incoming")]
        public ActionResult<Result<ExaminationIncomingModel>> GetIncoming()
        {
            var response = new Result<ExaminationIncomingModel> { Meta = new Meta() };
            try
            {
                var _examinations = context.Examinations.ToList();
                var _vehicles = context.Vehicles.Where(c => _examinations.Select(d => d.VehicleTypeId).Contains(c.VehicleTypeId)).ToList();
                var _examinationResults = context.MaintenanceResults.Where(c => _vehicles.Select(e => e.Id).ToList().Contains(c.VehicleId ?? 0)).ToList();

                foreach (var vehicle in _vehicles)
                {
                    var _examination = _examinations.FirstOrDefault(c => c.VehicleTypeId == vehicle.VehicleTypeId);
                    if (vehicle.InstantMileage + 500 > _examination.Mileage)
                    {
                        var _examinationResult = _examinationResults.FirstOrDefault(c => c.VehicleId == vehicle.Id);
                        if (_examinationResult == null)
                        {
                            response.Entities.Add(new ExaminationIncomingModel
                            {
                                Alert = true,
                                Maintenance = _examination,
                                Vehicle = vehicle,
                                Remaining = _examination.Mileage - vehicle.InstantMileage
                            });
                        }
                        else
                        {
                            if (vehicle.InstantMileage + 500 > _examinationResult.MaintenanceMileage + _examination.Mileage)
                            {
                                response.Entities.Add(new ExaminationIncomingModel
                                {
                                    Alert = true,
                                    Maintenance = _examination,
                                    Vehicle = vehicle,
                                    Remaining = (_examinationResult.MaintenanceMileage + _examination.Mileage) - vehicle.InstantMileage
                                });
                            }
                        }
                    }
                }

                response.Meta.IsSuccess = true;
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