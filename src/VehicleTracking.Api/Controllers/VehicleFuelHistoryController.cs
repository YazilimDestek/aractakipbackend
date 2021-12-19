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
    public class VehicleFuelHistoryController : BaseController
    {
        public VehicleFuelHistoryController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<VehicleFuelHistory>> GetAll()
        {
            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };
            try
            {
                var vehicleFuelHistories = context.VehicleFuelHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = vehicleFuelHistories;
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
        public ActionResult<Result<VehicleFuelHistory>> GetById(int id)
        {
            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };
            try
            {
                var vehicleFuelHistory = context.VehicleFuelHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).FirstOrDefault(x => x.Id == id);
                response.Entity = vehicleFuelHistory;
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
        public ActionResult<Result<VehicleFuelHistory>> Create([FromBody] VehicleFuelHistoryModel vehicleFuelHistoryModel)
        {
            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };

            try
            {
                var vehicleFuelHistory = new VehicleFuelHistory
                {
                    VehicleId = vehicleFuelHistoryModel.VehicleId,
                    Liter = vehicleFuelHistoryModel.Liter,
                    Mileage = vehicleFuelHistoryModel.Mileage,
                    FuelType = vehicleFuelHistoryModel.FuelType,
                    TakenDate = vehicleFuelHistoryModel.TakenDate,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.VehicleFuelHistories.Add(vehicleFuelHistory);
                context.SaveChanges();

                response.Entity = new VehicleFuelHistory();

                var newVehicleFuelHistory = context.VehicleFuelHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).FirstOrDefault(x => x.CreatedDateTime == vehicleFuelHistory.CreatedDateTime);

                response.Entity.Id = newVehicleFuelHistory.Id;
                response.Entity.VehicleId = newVehicleFuelHistory.VehicleId;
                response.Entity.Vehicle = newVehicleFuelHistory.Vehicle;
                response.Entity.Liter = newVehicleFuelHistory.Liter;
                response.Entity.Mileage = newVehicleFuelHistory.Mileage;
                response.Entity.FuelType = newVehicleFuelHistory.FuelType;
                response.Entity.TakenDate = newVehicleFuelHistory.TakenDate;
                response.Entity.CreatedDateTime = newVehicleFuelHistory.CreatedDateTime;
                response.Entity.CreatedUserId = newVehicleFuelHistory.CreatedUserId;

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
        public ActionResult<Result<VehicleFuelHistory>> GetByFilter([FromBody] VehicleFuelHistoryModel vehicleFuelHistoryModel)
        {
            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };

            try
            {

                var vehicleFuelHistory = context.VehicleFuelHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Where(x => x.IsDeleted == false);

                if (vehicleFuelHistoryModel.VehicleId > 0)
                {
                    vehicleFuelHistory = vehicleFuelHistory.Where(x => x.VehicleId == vehicleFuelHistoryModel.VehicleId);
                }
                if (!string.IsNullOrEmpty(vehicleFuelHistoryModel.FuelType))
                {
                    vehicleFuelHistory = vehicleFuelHistory.Where(x => x.FuelType == vehicleFuelHistoryModel.FuelType);
                }
                if (vehicleFuelHistoryModel.TakenDate != null)
                {
                    vehicleFuelHistory = vehicleFuelHistory.Where(x => x.TakenDate.Year == vehicleFuelHistoryModel.TakenDate.Year && x.TakenDate.Month == vehicleFuelHistoryModel.TakenDate.Month && x.TakenDate.Day == vehicleFuelHistoryModel.TakenDate.Day);
                }
                response.Entities = vehicleFuelHistory.ToList();
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

        [HttpPost("excelImport")]
        public async Task<Result<string>> UploadExcelVehicleFuelHistory()
        {
            var response = new Result<string>() { Meta = new Meta { ErrorMessage = string.Empty } };
            response.Entities = new List<string>();

            var user = context.Users.FirstOrDefault(x => x.Id == GetUserId());
            if (user.IsDeleted == true || user.IsAdmin == false)
            {
                return response;
            }

            var files = Request.Form.Files; // Now you have them
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
                        var VehicleFuelHistories = new List<VehicleFuelHistory>();

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
                                                VehicleFuelHistories.Add(
                                                    new VehicleFuelHistory
                                                    {
                                                        Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                        VehicleId = currentVehicle.Id,
                                                        FuelType = firstSheet.Cells["C" + _row].Text,
                                                        Liter = int.Parse(firstSheet.Cells["D" + _row].Text),
                                                        Mileage = long.Parse(firstSheet.Cells["E" + _row].Text),
                                                        TakenDate = DateTime.Parse(firstSheet.Cells["F" + _row].Text)
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
                                    var currentVehicleFuelHistories = context.VehicleFuelHistories.Where(x => x.IsDeleted == false);
                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var vehicleFuelHistory in VehicleFuelHistories)
                                            {
                                                if (vehicleFuelHistory.Id == null)
                                                {
                                                    vehicleFuelHistory.Id = 0;
                                                }

                                                var currentvehicleFuelHistory = currentVehicleFuelHistories.FirstOrDefault(x => x.Id != vehicleFuelHistory.Id && vehicleFuelHistory.Id > 0);
                                                if (currentvehicleFuelHistory == null)
                                                {
                                                    var newvehicleFuelHistory = new VehicleFuelHistory
                                                    {
                                                        VehicleId = vehicleFuelHistory.VehicleId,
                                                        FuelType = vehicleFuelHistory.FuelType,
                                                        Liter = vehicleFuelHistory.Liter,
                                                        Mileage = vehicleFuelHistory.Mileage,
                                                        TakenDate = vehicleFuelHistory.TakenDate,

                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                    };
                                                    context.VehicleFuelHistories.Add(newvehicleFuelHistory);
                                                }
                                                else
                                                {
                                                    currentvehicleFuelHistory.VehicleId = vehicleFuelHistory.VehicleId;
                                                    currentvehicleFuelHistory.FuelType = vehicleFuelHistory.FuelType;
                                                    currentvehicleFuelHistory.Liter = vehicleFuelHistory.Liter;
                                                    currentvehicleFuelHistory.Mileage = vehicleFuelHistory.Mileage;
                                                    currentvehicleFuelHistory.TakenDate = vehicleFuelHistory.TakenDate;

                                                    currentvehicleFuelHistory.UpdatedDateTime = DateTime.Now;
                                                    currentvehicleFuelHistory.UpdatedUserId = GetUserId();

                                                    context.Entry(currentvehicleFuelHistory).State = EntityState.Modified;
                                                }
                                                context.SaveChanges();
                                            }
                                            dbContextTransaction.Commit();
                                            response.Meta.IsSuccess = true;
                                        }
                                        catch
                                        {
                                            dbContextTransaction.Rollback();
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
            return response;
        }

        [HttpPut("update/{id}")]
        public ActionResult<Result<VehicleFuelHistory>> Update(int id, [FromBody] VehicleFuelHistoryModel vehicleFuelHistoryModel)
        {
            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };

            try
            {
                var vehicleFuelHistory = context.VehicleFuelHistories.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (vehicleFuelHistory != null)
                {
                    vehicleFuelHistory.VehicleId = vehicleFuelHistoryModel.VehicleId;
                    vehicleFuelHistory.Liter = vehicleFuelHistoryModel.Liter;
                    vehicleFuelHistory.Mileage = vehicleFuelHistoryModel.Mileage;
                    vehicleFuelHistory.FuelType = vehicleFuelHistoryModel.FuelType;
                    vehicleFuelHistory.TakenDate = vehicleFuelHistoryModel.TakenDate;
                    vehicleFuelHistory.UpdatedDateTime = DateTime.Now;
                    vehicleFuelHistory.UpdatedUserId = GetUserId();

                    context.Entry(vehicleFuelHistory).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new VehicleFuelHistory();

                    vehicleFuelHistory = context.VehicleFuelHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = vehicleFuelHistory.Id;
                    response.Entity.VehicleId = vehicleFuelHistory.VehicleId;
                    response.Entity.Vehicle = vehicleFuelHistory.Vehicle;
                    response.Entity.Liter = vehicleFuelHistory.Liter;
                    response.Entity.Mileage = vehicleFuelHistory.Mileage;
                    response.Entity.FuelType = vehicleFuelHistory.FuelType;
                    response.Entity.TakenDate = vehicleFuelHistory.TakenDate;
                    response.Entity.UpdatedUserId = vehicleFuelHistory.UpdatedUserId;
                    response.Entity.UpdatedDateTime = vehicleFuelHistory.UpdatedDateTime;

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
        public ActionResult<Result<VehicleFuelHistory>> Delete(int id)
        {

            var response = new Result<VehicleFuelHistory> { Meta = new Meta() };
            try
            {
                var vehicleFuelHistory = context.VehicleFuelHistories.FirstOrDefault(c => c.Id == id);
                vehicleFuelHistory.IsDeleted = true;
                context.Entry(vehicleFuelHistory).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = vehicleFuelHistory;

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