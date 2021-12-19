using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : BaseController
    {
        public MaintenanceController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Maintenance>> GetAll()
        {
            var response = new Result<Maintenance> { Meta = new Meta() };
            try
            {
                var maintenance = context.Maintenance.Include(x => x.VehicleType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = maintenance;
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
        public ActionResult<Result<Maintenance>> GetById(int id)
        {
            var response = new Result<Maintenance> { Meta = new Meta() };
            try
            {
                var maintenance = context.Maintenance.Include(x => x.VehicleType).FirstOrDefault(x => x.Id == id);
                response.Entity = maintenance;
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
        public ActionResult<Result<Maintenance>> Create([FromBody] MaintenanceModel maintenanceModel)
        {
            var response = new Result<Maintenance> { Meta = new Meta() };

            try
            {
                var maintenance = new Maintenance
                {
                    VehicleTypeId = maintenanceModel.VehicleTypeId,
                    MaintenanceMileage = maintenanceModel.MaintenanceMileage,
                    MaintenancePath = maintenanceModel.MaintenancePath,
                    MaintenanceType = maintenanceModel.MaintenanceType,
                    RememberDate = maintenanceModel.RememberDate,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.Maintenance.Add(maintenance);
                context.SaveChanges();

                response.Entity = new Maintenance();

                var newMaintenance = context.Maintenance.Include(x => x.VehicleType).FirstOrDefault(x => x.CreatedDateTime == maintenance.CreatedDateTime);

                response.Entity.Id = newMaintenance.Id;
                response.Entity.VehicleTypeId = newMaintenance.VehicleTypeId;
                response.Entity.VehicleType = newMaintenance.VehicleType;
                response.Entity.MaintenanceMileage = newMaintenance.MaintenanceMileage;
                response.Entity.MaintenancePath = newMaintenance.MaintenancePath;
                response.Entity.MaintenanceType = newMaintenance.MaintenanceType;
                response.Entity.RememberDate = newMaintenance.RememberDate;
                response.Entity.CreatedDateTime = newMaintenance.CreatedDateTime;
                response.Entity.CreatedUserId = newMaintenance.CreatedUserId;

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
        public ActionResult<Result<Maintenance>> GetByFilter([FromBody] MaintenanceModel maintenanceModel)
        {
            var response = new Result<Maintenance> { Meta = new Meta() };

            try
            {
                var maintenance = context.Maintenance.Include(x => x.VehicleType).Where(x => x.IsDeleted == false);

                if (!string.IsNullOrEmpty(maintenanceModel.MaintenanceType))
                {
                    maintenance = maintenance.Where(x => x.MaintenanceType == maintenanceModel.MaintenanceType);
                }
                if (maintenanceModel.VehicleTypeId > 0)
                {
                    maintenance = maintenance.Where(x => x.VehicleTypeId == maintenanceModel.VehicleTypeId);
                }
                if (maintenanceModel.MaintenanceMileage > 0)
                {
                    maintenance = maintenance.Where(x => x.MaintenanceMileage == maintenanceModel.MaintenanceMileage);
                }
                if (maintenanceModel.RememberDate != null)
                {
                    maintenance = maintenance.Where(x => x.RememberDate.Year == maintenanceModel.RememberDate.Year && x.RememberDate.Month == maintenanceModel.RememberDate.Month && x.RememberDate.Day == maintenanceModel.RememberDate.Day);
                }
                response.Entities = maintenance.ToList();
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

        [HttpPut("update/{id}")]
        public ActionResult<Result<Maintenance>> Update(int id, [FromBody] MaintenanceModel maintenanceModel)
        {
            var response = new Result<Maintenance> { Meta = new Meta() };

            try
            {
                var maintenance = context.Maintenance.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (maintenance != null)
                {
                    maintenance.UpdatedDateTime = DateTime.Now;
                    maintenance.UpdatedUserId = GetUserId();
                    maintenance.VehicleTypeId = maintenanceModel.VehicleTypeId;
                    maintenance.MaintenanceMileage = maintenanceModel.MaintenanceMileage;
                    maintenance.MaintenancePath = maintenanceModel.MaintenancePath;
                    maintenance.MaintenanceType = maintenanceModel.MaintenanceType;
                    maintenance.RememberDate = maintenanceModel.RememberDate;

                    context.Entry(maintenance).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Maintenance();

                    maintenance = context.Maintenance.Include(x => x.VehicleType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = maintenance.Id;
                    response.Entity.VehicleTypeId = maintenance.VehicleTypeId;
                    response.Entity.VehicleType = maintenance.VehicleType;
                    response.Entity.MaintenanceMileage = maintenance.MaintenanceMileage;
                    response.Entity.MaintenancePath = maintenance.MaintenancePath;
                    response.Entity.MaintenanceType = maintenance.MaintenanceType;
                    response.Entity.RememberDate = maintenance.RememberDate;
                    response.Entity.UpdatedDateTime = maintenance.UpdatedDateTime;
                    response.Entity.UpdatedUserId = maintenance.UpdatedUserId;

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
        public ActionResult<Result<Maintenance>> Delete(int id)
        {

            var response = new Result<Maintenance> { Meta = new Meta() };
            try
            {
                var maintenance = context.Maintenance.FirstOrDefault(c => c.Id == id);
                maintenance.IsDeleted = true;
                context.Entry(maintenance).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = maintenance;

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
        public ActionResult<Result<MaintenanceIncomingModel>> GetIncoming()
        {
            var response = new Result<MaintenanceIncomingModel> { Meta = new Meta() };
            try
            {
                var _examinations = context.Maintenance.ToList();
                var _vehicles = context.Vehicles.Where(c => _examinations.Select(d => d.VehicleTypeId).Contains(c.VehicleTypeId)).ToList();
                var _examinationResults = context.MaintenanceResults.Where(c => _vehicles.Select(e => e.Id).ToList().Contains(c.VehicleId ?? 0)).ToList();

                foreach (var vehicle in _vehicles)
                {
                    var _examination = _examinations.FirstOrDefault(c => c.VehicleTypeId == vehicle.VehicleTypeId);
                    if (vehicle.InstantMileage + 500 > _examination.MaintenanceMileage)
                    {
                        var _examinationResult = _examinationResults.FirstOrDefault(c => c.VehicleId == vehicle.Id);
                        if (_examinationResult == null)
                        {
                            response.Entities.Add(new MaintenanceIncomingModel
                            {
                                Alert = true,
                                Maintenance = _examination,
                                Vehicle = vehicle,
                                Remaining = _examination.MaintenanceMileage - vehicle.InstantMileage
                            });
                        }
                        else
                        {
                            if (vehicle.InstantMileage + 500 > _examinationResult.MaintenanceMileage + _examination.MaintenanceMileage)
                            {
                                response.Entities.Add(new MaintenanceIncomingModel
                                {
                                    Alert = true,
                                    Maintenance = _examination,
                                    Vehicle = vehicle,
                                    Remaining = (_examinationResult.MaintenanceMileage + _examination.MaintenanceMileage) - vehicle.InstantMileage
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