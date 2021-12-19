using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class VehicleTireHistoryController : BaseController
    {
        public VehicleTireHistoryController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<VehicleTireHistory>> GetAll()
        {
            var response = new Result<VehicleTireHistory> { Meta = new Meta() };
            try
            {
                var vehicleTireHistories = context.VehicleTireHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Include(x => x.Tire).Where(x => x.IsDeleted == false).ToList();
                response.Entities = vehicleTireHistories;
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
        public ActionResult<Result<VehicleTireHistory>> GetById(int id)
        {
            var response = new Result<VehicleTireHistory> { Meta = new Meta() };
            try
            {
                var vehicleTireHistory = context.VehicleTireHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Include(x => x.Tire).FirstOrDefault(x => x.Id == id);
                response.Entity = vehicleTireHistory;
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
        public ActionResult<Result<VehicleTireHistory>> GetByFilter([FromBody] VehicleTireHistoryFilterModel vehicleTireHistoryFilterModel)
        {
            var response = new Result<VehicleTireHistory> { Meta = new Meta() };

            try
            {
                var vehicleTireHistoryFilter = context.VehicleTireHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Include(x => x.Tire).Where(x => x.IsDeleted == false);

                if (vehicleTireHistoryFilterModel.VehicleId > 0)
                {
                    vehicleTireHistoryFilter = vehicleTireHistoryFilter.Where(x =>
                        x.VehicleId == vehicleTireHistoryFilterModel.VehicleId);
                }

                if (vehicleTireHistoryFilterModel.TireId > 0)
                {
                    vehicleTireHistoryFilter =
                        vehicleTireHistoryFilter.Where(x => x.TireId == vehicleTireHistoryFilterModel.TireId);
                }

                if (vehicleTireHistoryFilterModel.isOnVehicle == true)
                {

                    if (vehicleTireHistoryFilterModel.InstalledDateStart != null)
                    {
                        vehicleTireHistoryFilter = vehicleTireHistoryFilter.Where(x =>
                            x.InstalledDate >= vehicleTireHistoryFilterModel.InstalledDateStart && x.InstalledDate <= vehicleTireHistoryFilterModel.InstalledDateEnd);
                    }

                }
                else if (vehicleTireHistoryFilterModel.isOnVehicle == false)
                {
                    if (vehicleTireHistoryFilterModel.InstalledDateStart != null)
                    {
                        vehicleTireHistoryFilter = vehicleTireHistoryFilter.Where(x =>
                            x.InstalledDate >= vehicleTireHistoryFilterModel.InstalledDateStart && x.InstalledDate <= vehicleTireHistoryFilterModel.InstalledDateEnd);
                    }
                    if (vehicleTireHistoryFilterModel.RemovedDateStart != null)
                    {
                        vehicleTireHistoryFilter = vehicleTireHistoryFilter.Where(x =>
                            x.RemovedDate >= vehicleTireHistoryFilterModel.RemovedDateStart && x.RemovedDate <= vehicleTireHistoryFilterModel.RemovedDateEnd);
                    }
                }

                response.Entities = vehicleTireHistoryFilter.ToList();
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
        public async Task<Result<string>> UploadExcelVehicleTireHistory()
        {
            var response = new Result<string>() { Meta = new Meta { ErrorMessage = string.Empty } };
            response.Entities = new List<string>();

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
                        var VehicleTireHistories = new List<VehicleTireHistory>();

                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //EXCEL PARSE
                            using (var package = new ExcelPackage(ms))
                            {
                                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                var firstSheet = package.Workbook.Worksheets[0];

                                if (firstSheet.Cells["A1"].Text == "0" && firstSheet.Cells["B1"].Text == "1" && firstSheet.Cells["C1"].Text == "2" && firstSheet.Cells["D1"].Text == "3")
                                {
                                    var _row = 3;
                                    while (1 == 1)
                                    {
                                        if (firstSheet.Cells["B" + _row].Text != "")
                                        {
                                            try
                                            {
                                                VehicleTireHistories.Add(new VehicleTireHistory
                                                {
                                                    Id = int.Parse(firstSheet.Cells["A" + _row].Text),
                                                    VehicleId = int.Parse(firstSheet.Cells["B" + _row].Text),
                                                    TireId = int.Parse(firstSheet.Cells["C" + _row].Text),
                                                    HoleOrder = int.Parse(firstSheet.Cells["D" + _row].Text),
                                                    InstalledDate = DateTime.Parse(firstSheet.Cells["E" + _row].Text),
                                                    RemovedDate = DateTime.Parse(firstSheet.Cells["F" + _row].Text),
                                                    RemovedReason = firstSheet.Cells["G" + _row].Text,

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
                                    response.Meta.ErrorMessage += " Excel formatını kontrol ederek tekrar deneyiniz, 'Dışarı Aktar' tuşuna basarak formatı alabilirsiniz.";
                                    return response;
                                }
                                if (response.Entities.Count == 0)
                                {
                                    var currentVehicleTireHistories = context.VehicleTireHistories.Where(x => x.IsDeleted == false).ToList();

                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var vehicleTireHistory in VehicleTireHistories)
                                            {
                                                var currentVehicleTireHistory = currentVehicleTireHistories.FirstOrDefault(x => x.Id == vehicleTireHistory.Id);

                                                if (currentVehicleTireHistory == null)
                                                {
                                                    var newVehicleTireHistory = new VehicleTireHistory
                                                    {
                                                        VehicleId = vehicleTireHistory.VehicleId,
                                                        TireId = vehicleTireHistory.TireId,
                                                        HoleOrder = vehicleTireHistory.HoleOrder,
                                                        RemovedDate = vehicleTireHistory.RemovedDate,
                                                        InstalledDate = vehicleTireHistory.InstalledDate,
                                                        RemovedReason = vehicleTireHistory.RemovedReason,
                                                        CreatedUserId = GetUserId(),
                                                        CreatedDateTime = DateTime.Now,
                                                    };
                                                    context.VehicleTireHistories.Add(newVehicleTireHistory);

                                                }
                                                else
                                                {
                                                    currentVehicleTireHistory.VehicleId = vehicleTireHistory.VehicleId;
                                                    currentVehicleTireHistory.TireId = vehicleTireHistory.TireId;
                                                    currentVehicleTireHistory.HoleOrder = vehicleTireHistory.HoleOrder;
                                                    currentVehicleTireHistory.RemovedDate = vehicleTireHistory.RemovedDate;
                                                    currentVehicleTireHistory.InstalledDate = vehicleTireHistory.InstalledDate;
                                                    currentVehicleTireHistory.RemovedReason = vehicleTireHistory.RemovedReason;
                                                    currentVehicleTireHistory.UpdatedUserId = GetUserId();
                                                    currentVehicleTireHistory.UpdatedDateTime = DateTime.Now;

                                                    context.Entry(currentVehicleTireHistory).State = EntityState.Modified;
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

        [HttpPost("create")]
        public ActionResult<Result<VehicleTireHistory>> Create([FromBody] VehicleTireHistoryModel vehicleTireHistoryModel)
        {
            var response = new Result<VehicleTireHistory> { Meta = new Meta() };

            try
            {
                var vehicleTireHistory = new VehicleTireHistory
                {
                    TireId = vehicleTireHistoryModel.TireId,
                    HoleOrder = vehicleTireHistoryModel.HoleOrder,
                    RemovedDate = vehicleTireHistoryModel.RemovedDate,
                    RemovedReason = vehicleTireHistoryModel.RemovedReason,
                    VehicleId = vehicleTireHistoryModel.VehicleId,
                    InstalledDate = vehicleTireHistoryModel.InstalledDate,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.VehicleTireHistories.Add(vehicleTireHistory);
                context.SaveChanges();

                response.Entity = new VehicleTireHistory();

                var newVehicleTireHistory = context.VehicleTireHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Include(x => x.Tire).FirstOrDefault(x => x.CreatedDateTime == vehicleTireHistory.CreatedDateTime);

                response.Entity.Id = newVehicleTireHistory.Id;
                response.Entity.VehicleId = newVehicleTireHistory.VehicleId;
                response.Entity.Vehicle = newVehicleTireHistory.Vehicle;
                response.Entity.TireId = newVehicleTireHistory.TireId;
                response.Entity.Tire = newVehicleTireHistory.Tire;
                response.Entity.InstalledDate = newVehicleTireHistory.InstalledDate;
                response.Entity.RemovedDate = newVehicleTireHistory.RemovedDate;
                response.Entity.RemovedReason = newVehicleTireHistory.RemovedReason;
                response.Entity.HoleOrder = newVehicleTireHistory.HoleOrder;
                response.Entity.CreatedDateTime = newVehicleTireHistory.CreatedDateTime;
                response.Entity.CreatedUserId = newVehicleTireHistory.CreatedUserId;

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
        public ActionResult<Result<VehicleTireHistory>> Update(int id, [FromBody] VehicleTireHistoryModel vehicleTireHistoryModel)
        {
            var response = new Result<VehicleTireHistory> { Meta = new Meta() };

            try
            {
                var vehicleTireHistory = context.VehicleTireHistories.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (vehicleTireHistory != null)
                {
                    vehicleTireHistory.UpdatedDateTime = DateTime.Now;
                    vehicleTireHistory.UpdatedUserId = GetUserId();
                    vehicleTireHistory.VehicleId = vehicleTireHistoryModel.VehicleId;
                    vehicleTireHistory.HoleOrder = vehicleTireHistoryModel.HoleOrder;
                    vehicleTireHistory.InstalledDate = vehicleTireHistoryModel.InstalledDate;
                    vehicleTireHistory.RemovedDate = vehicleTireHistoryModel.RemovedDate;
                    vehicleTireHistory.RemovedReason = vehicleTireHistoryModel.RemovedReason;
                    vehicleTireHistory.TireId = vehicleTireHistoryModel.TireId;

                    context.Entry(vehicleTireHistory).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new VehicleTireHistory();

                    vehicleTireHistory = context.VehicleTireHistories.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Include(x => x.Tire).FirstOrDefault(x => x.Id == id);

                    response.Entity.Id = vehicleTireHistory.Id;
                    response.Entity.VehicleId = vehicleTireHistory.VehicleId;
                    response.Entity.Vehicle = vehicleTireHistory.Vehicle;
                    response.Entity.TireId = vehicleTireHistory.TireId;
                    response.Entity.Tire = vehicleTireHistory.Tire;
                    response.Entity.InstalledDate = vehicleTireHistory.InstalledDate;
                    response.Entity.RemovedDate = vehicleTireHistory.RemovedDate;
                    response.Entity.RemovedReason = vehicleTireHistory.RemovedReason;
                    response.Entity.HoleOrder = vehicleTireHistory.HoleOrder;
                    response.Entity.UpdatedDateTime = vehicleTireHistory.UpdatedDateTime;
                    response.Entity.UpdatedUserId = vehicleTireHistory.UpdatedUserId;

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
        public ActionResult<Result<VehicleTireHistory>> Delete(int id)
        {

            var response = new Result<VehicleTireHistory> { Meta = new Meta() };
            try
            {
                var vehicleTireHistory = context.VehicleTireHistories.FirstOrDefault(c => c.Id == id);
                vehicleTireHistory.IsDeleted = true;
                context.Entry(vehicleTireHistory).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = vehicleTireHistory;

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