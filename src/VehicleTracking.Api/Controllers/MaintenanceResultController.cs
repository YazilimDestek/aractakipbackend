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
    public class MaintenanceResultController : BaseController
    {
        public MaintenanceResultController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<MaintenanceResult>> GetAll()
        {
            var response = new Result<MaintenanceResult> { Meta = new Meta() };
            try
            {
                var maintenanceResults = context.MaintenanceResults.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).Where(x => x.IsDeleted == false).ToList();
                response.Entities = maintenanceResults;
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
        public ActionResult<Result<MaintenanceResult>> GetById(int id)
        {
            var response = new Result<MaintenanceResult> { Meta = new Meta() };
            try
            {
                var maintenanceResult = context.MaintenanceResults.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.Id == id);
                response.Entity = maintenanceResult;
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

        [HttpPost("filter")]
        public ActionResult<Result<MaintenanceResult>> GetByFilter([FromBody] MaintenanceResultModel maintenanceResultModel)
        {
            var response = new Result<MaintenanceResult> { Meta = new Meta() };

            try
            {
                var maintenanceResults = context.MaintenanceResults.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).Where(x => x.IsDeleted == false);
                if (!string.IsNullOrEmpty(maintenanceResultModel.MaintenanceType))
                {
                    maintenanceResults = maintenanceResults.Where(x => x.MaintenanceType == maintenanceResultModel.MaintenanceType);
                }
                if (maintenanceResultModel.VehicleId > 0)
                {
                    maintenanceResults = maintenanceResults.Where(x => x.VehicleId == maintenanceResultModel.VehicleId);
                }
                if (maintenanceResultModel.MaintenanceMileage >= 0)
                {
                    maintenanceResults = maintenanceResults.Where(x => x.MaintenanceMileage == maintenanceResultModel.MaintenanceMileage);
                }
                if (maintenanceResultModel.MaintenanceDate != null)
                {
                    maintenanceResults = maintenanceResults.Where(x => x.MaintenanceDate.Year == maintenanceResultModel.MaintenanceDate.Year && x.MaintenanceDate.Month == maintenanceResultModel.MaintenanceDate.Month && x.MaintenanceDate.Day == maintenanceResultModel.MaintenanceDate.Day);
                }
                response.Entities = maintenanceResults.ToList();
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

        [HttpPost("excelImport")]
        public async Task<Result<string>> UploadExcelMaintenanceResult()
        {
            var response = new Result<string>() { Meta = new Meta { ErrorMessage = string.Empty } };
            response.Entities = new List<string>();
            try
            {

                var user = context.Users.FirstOrDefault(x => x.Id == GetUserId());
                if (user.IsDeleted == true || user.IsAdmin == false)
                {
                    return response;
                }

                var files = Request.Form.Files;
                bool fileOK = false;

                foreach (var file in files)
                {
                    var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    fileOK = false;
                    String[] allowedExtensions = { ".xls", ".xlsx" };
                    for (int i = 0; i < allowedExtensions.Length; i++)
                    {
                        if (fileExtension == allowedExtensions[i])
                        {
                            fileOK = true;
                        }
                    }

                    if (fileOK)
                    {
                        try
                        {
                            var MaintenanceResults = new List<MaintenanceResult>();

                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                //EXCEL PARSE
                                using (var package = new ExcelPackage(ms))
                                {
                                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                    var firstSheet = package.Workbook.Worksheets[0];
                                    var currentVehicles = context.Vehicles.Where(x => x.IsDeleted == false);
                                    if (firstSheet.Cells["A1"].Text == "0" && firstSheet.Cells["B1"].Text == "1" && firstSheet.Cells["C1"].Text == "2" && firstSheet.Cells["D1"].Text == "3")
                                    {
                                        var _row = 3;
                                        while (1 == 1)
                                        {
                                            if (firstSheet.Cells["B" + _row].Text != "")
                                            {
                                                try
                                                {
                                                    var plateNo = firstSheet.Cells["B" + _row].Text;
                                                    var currentVehicle = currentVehicles.FirstOrDefault(x => x.PlateNumber == plateNo);
                                                    MaintenanceResults.Add(
                                                        new MaintenanceResult
                                                        {
                                                            Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                            VehicleId = currentVehicle.Id,
                                                            MaintenanceType = firstSheet.Cells["C" + _row].Text,
                                                            MaintenanceMileage = long.Parse(firstSheet.Cells["D" + _row].Text),
                                                            MaintenanceDate = DateTime.Parse(firstSheet.Cells["E" + _row].Text)

                                                        });
                                                }
                                                catch (Exception ex)
                                                {
                                                    response.Entities.Add(_row + " numaralı satırda veri tipi hatası! Numerik değerleri kontrol ediniz");
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            _row++;
                                        }
                                    }
                                    else
                                    {
                                        response.Meta.IsSuccess = false;
                                        response.Meta.Error = "unexpected.exception";
                                        response.Meta.ErrorMessage += "Excel formatını kontrol ederek tekrar deneyiniz, 'Dışarı Aktar' tuşuna basarak formatı alabilirsiniz.";
                                        return response;
                                    }
                                    if (response.Entities.Count == 0)
                                    {
                                        var currentMaintenanceResults = context.MaintenanceResults.Where(x => x.IsDeleted == false).ToList();
                                        using (var dbContextTranssaction = context.Database.BeginTransaction())
                                        {
                                            try
                                            {
                                                foreach (var maintenanceResult in MaintenanceResults)
                                                {
                                                    if (maintenanceResult.Id == null)
                                                    {
                                                        maintenanceResult.Id = 0;
                                                    }
                                                    var currentMaintenanceResult = currentMaintenanceResults.FirstOrDefault(x => x.Id != maintenanceResult.Id && maintenanceResult.Id > 0);
                                                    if (currentMaintenanceResult == null)
                                                    {
                                                        var newMaintenanceResult = new MaintenanceResult
                                                        {
                                                            VehicleId = maintenanceResult.VehicleId,
                                                            MaintenanceType = maintenanceResult.MaintenanceType,
                                                            MaintenanceDate = maintenanceResult.MaintenanceDate,
                                                            MaintenanceMileage = maintenanceResult.MaintenanceMileage,

                                                            CreatedDateTime = DateTime.Now,
                                                            CreatedUserId = GetUserId(),
                                                        };
                                                        context.MaintenanceResults.Add(newMaintenanceResult);
                                                    }
                                                    else
                                                    {
                                                        currentMaintenanceResult.MaintenanceType = maintenanceResult.MaintenanceType;
                                                        currentMaintenanceResult.MaintenanceDate = maintenanceResult.MaintenanceDate;
                                                        currentMaintenanceResult.MaintenanceMileage = maintenanceResult.MaintenanceMileage;

                                                        currentMaintenanceResult.UpdatedDateTime = DateTime.Now;
                                                        currentMaintenanceResult.UpdatedUserId = GetUserId();

                                                        context.Entry(currentMaintenanceResult).State = EntityState.Modified;
                                                    }
                                                    context.SaveChanges();
                                                }
                                                dbContextTranssaction.Commit();
                                                response.Meta.IsSuccess = true;
                                            }
                                            catch
                                            {
                                                dbContextTranssaction.Rollback();
                                                response.Meta.IsSuccess = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return response;
        }

        [HttpPost]
        public ActionResult<Result<MaintenanceResult>> Create([FromBody] MaintenanceResultModel maintenanceResultModel)
        {
            var response = new Result<MaintenanceResult> { Meta = new Meta() };

            try
            {
                var maintenanceResult = new MaintenanceResult
                {
                    VehicleId = maintenanceResultModel.VehicleId,
                    MaintenanceType = maintenanceResultModel.MaintenanceType,
                    MaintenanceMileage = maintenanceResultModel.MaintenanceMileage,
                    MaintenanceDate = maintenanceResultModel.MaintenanceDate,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.MaintenanceResults.Add(maintenanceResult);
                context.SaveChanges();

                response.Entity = new MaintenanceResult();

                var newMaitenanceResult = context.MaintenanceResults.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.CreatedDateTime == maintenanceResult.CreatedDateTime);

                response.Entity.Id = newMaitenanceResult.Id;
                response.Entity.VehicleId = newMaitenanceResult.VehicleId;
                response.Entity.Vehicle = newMaitenanceResult.Vehicle;
                response.Entity.MaintenanceType = newMaitenanceResult.MaintenanceType;
                response.Entity.MaintenanceMileage = newMaitenanceResult.MaintenanceMileage;
                response.Entity.MaintenanceDate = newMaitenanceResult.MaintenanceDate;
                response.Entity.CreatedUserId = newMaitenanceResult.CreatedUserId;
                response.Entity.CreatedDateTime = newMaitenanceResult.CreatedDateTime;

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
        public ActionResult<Result<MaintenanceResult>> Update(int id, [FromBody] MaintenanceResultModel maintenanceResultModel)
        {
            var response = new Result<MaintenanceResult> { Meta = new Meta() };

            try
            {
                var maintenanceResult = context.MaintenanceResults.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (maintenanceResult != null)
                {
                    maintenanceResult.UpdatedDateTime = DateTime.Now;
                    maintenanceResult.UpdatedUserId = GetUserId();
                    maintenanceResult.VehicleId = maintenanceResultModel.VehicleId;
                    maintenanceResult.MaintenanceType = maintenanceResultModel.MaintenanceType;
                    maintenanceResult.MaintenanceMileage = maintenanceResultModel.MaintenanceMileage;
                    maintenanceResult.MaintenanceDate = maintenanceResultModel.MaintenanceDate;

                    context.Entry(maintenanceResult).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new MaintenanceResult();

                    maintenanceResult = context.MaintenanceResults.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = maintenanceResult.Id;
                    response.Entity.VehicleId = maintenanceResult.VehicleId;
                    response.Entity.Vehicle = maintenanceResult.Vehicle;
                    response.Entity.MaintenanceType = maintenanceResult.MaintenanceType;
                    response.Entity.MaintenanceMileage = maintenanceResult.MaintenanceMileage;
                    response.Entity.MaintenanceDate = maintenanceResult.MaintenanceDate;
                    response.Entity.UpdatedUserId = maintenanceResult.UpdatedUserId;
                    response.Entity.UpdatedDateTime = maintenanceResult.UpdatedDateTime;


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
        public ActionResult<Result<MaintenanceResult>> Delete(int id)
        {

            var response = new Result<MaintenanceResult> { Meta = new Meta() };
            try
            {
                var maintenanceResult = context.MaintenanceResults.FirstOrDefault(c => c.Id == id);
                maintenanceResult.IsDeleted = true;
                context.Entry(maintenanceResult).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = maintenanceResult;

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