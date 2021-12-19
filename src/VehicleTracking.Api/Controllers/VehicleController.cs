using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class VehicleController : BaseController
    {
        public VehicleController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Vehicle>> GetAll()
        {
            var response = new Result<Vehicle> { Meta = new Meta() };
            try
            {
                var vehicles = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Include(x => x.Engine).Include(x => x.FuelTank).Include(x => x.HydraulicTank).Include(x => x.VehicleType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = vehicles;
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
        public ActionResult<Result<Vehicle>> GetById(int id)
        {
            var response = new Result<Vehicle> { Meta = new Meta() };
            try
            {
                var vehicle = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Include(x => x.Engine).Include(x => x.FuelTank).Include(x => x.HydraulicTank).Include(x => x.VehicleType).FirstOrDefault(x => x.Id == id);
                response.Entity = vehicle;
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

        [HttpPost("quickFind")]
        public ActionResult<Result<Vehicle>> QuickFind([FromBody] VehicleFilterModel filter)
        {
            var response = new Result<Vehicle> { Meta = new Meta() };

            if (filter == null || string.IsNullOrEmpty(filter.keyword))
            {
                return response;
            }

            try
            {
                var vehicles = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Where(x => x.IsDeleted == false).ToList();

                if (vehicles.Count == 0 || vehicles.Count < 10)
                {
                    vehicles = vehicles.Where(x => x.PlateNumber.Contains(filter.keyword)).ToList();
                }

                if (vehicles.Count == 0 || vehicles.Count < 10)
                {
                    vehicles = vehicles.Where(x => x.Brand.Name.Contains(filter.keyword)).ToList();
                }

                if (vehicles.Count == 0 || vehicles.Count < 10)
                {
                    vehicles = vehicles.Where(x => x.Model.Name.Contains(filter.keyword)).ToList();
                }

                if (vehicles.Count == 0 || vehicles.Count < 10)
                {
                    vehicles = vehicles.Where(x => x.ModelYear == int.Parse(filter.keyword)).ToList();
                }

                response.Entities = vehicles.ToList();
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

        [HttpPost("filter")]
        public ActionResult<Result<Vehicle>> GetByFilter([FromBody] VehicleModel vehicleModel)
        {
            var response = new Result<Vehicle> { Meta = new Meta() };

            try
            {
                var vehicleFilter = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Include(x => x.Engine).Include(x => x.FuelTank).Include(x => x.HydraulicTank).Include(x => x.VehicleType).Where(x => x.IsDeleted == false);

                if (vehicleModel.BrandId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.BrandId == vehicleModel.BrandId);
                }
                if (vehicleModel.EngineId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.EngineId == vehicleModel.EngineId);
                }
                if (vehicleModel.FuelTankId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.FuelTankId == vehicleModel.FuelTankId);
                }
                if (vehicleModel.HydraulicTankId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.HydraulicTankId == vehicleModel.HydraulicTankId);
                }
                if (vehicleModel.ModelId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.ModelId == vehicleModel.ModelId);
                }
                if (vehicleModel.VehicleTypeId > 0)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.VehicleTypeId == vehicleModel.VehicleTypeId);
                }
                if (!string.IsNullOrEmpty(vehicleModel.OwnershipType))
                {
                    vehicleFilter = vehicleFilter.Where(x => x.OwnershipType == vehicleModel.OwnershipType);
                }
                if (vehicleModel.WarrantyEndDate != null)
                {
                    vehicleFilter = vehicleFilter.Where(x => x.WarrantyEndDate.Year == vehicleModel.WarrantyEndDate.Year && x.WarrantyEndDate.Month == vehicleModel.WarrantyEndDate.Month && x.WarrantyEndDate.Day == vehicleModel.WarrantyEndDate.Day);
                }
                response.Entities = vehicleFilter.ToList();
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

        [HttpPost("ExcelImport")]
        public async Task<Result<string>> UploadExcelVehicle()
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
                        var Vehicles = new List<Vehicle>();

                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //EXCEL PARSe
                            using (var package = new ExcelPackage(ms))
                            {
                                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                var firstSheet = package.Workbook.Worksheets[0];
                                var currentBrands = context.Brands.Where(x => x.IsDeleted == false);
                                var currentModels = context.Models.Where(x => x.IsDeleted == false);
                                var currentEngines = context.Engines.Where(x => x.IsDeleted == false);
                                var currentFuelTanks = context.FuelTanks.Where(x => x.IsDeleted == false);
                                var currentHydraulicTanks = context.HydraulicTanks.Where(x => x.IsDeleted == false);
                                var currentVehicleTypes = context.VehicleTypes.Where(x => x.IsDeleted == false);
                                if (firstSheet.Cells["A1"].Text == "0" && firstSheet.Cells["B1"].Text == "1" && firstSheet.Cells["C1"].Text == "2" && firstSheet.Cells["D1"].Text == "3")
                                {
                                    var _row = 3;
                                    while (1 == 1)
                                    {
                                        if (firstSheet.Cells["B" + _row].Text != "")
                                        {
                                            try
                                            {
                                                var currentBrand = currentBrands.FirstOrDefault(x => x.Name == (firstSheet.Cells["D" + _row].Text));
                                                var currentModel = currentModels.FirstOrDefault(x => x.Name == (firstSheet.Cells["E" + _row].Text));
                                                var currentEngine = currentEngines.FirstOrDefault(x => x.Name == (firstSheet.Cells["F" + _row].Text));
                                                var currentFuelTank = currentFuelTanks.FirstOrDefault(x => x.Name == (firstSheet.Cells["G" + _row].Text));
                                                var currentHydraulicTank = currentHydraulicTanks.FirstOrDefault(x => x.Name == (firstSheet.Cells["H" + _row].Text));
                                                var currentVehicleType = currentVehicleTypes.FirstOrDefault(x => x.Name == (firstSheet.Cells["O" + _row].Text));

                                                bool ısTrackingMode = false;

                                                if ((!string.IsNullOrEmpty(firstSheet.Cells["P" + _row].Text) || firstSheet.Cells["P" + _row].Text != "") && (firstSheet.Cells["P" + _row].Text.ToLower() == "var"))
                                                {
                                                    ısTrackingMode = true;
                                                }
                                                else
                                                {
                                                    ısTrackingMode = false;
                                                }

                                                Vehicles.Add(
                                                    new Vehicle
                                                    {
                                                        Id = int.Parse(firstSheet.Cells["A" + _row].Text),
                                                        PlateNumber = firstSheet.Cells["B" + _row].Text,
                                                        ModelYear = int.Parse(firstSheet.Cells["C" + _row].Text),
                                                        BrandId = currentBrand.Id,
                                                        ModelId = currentModel.Id,
                                                        EngineId = currentEngine.Id,
                                                        FuelTankId = currentFuelTank.Id,
                                                        HydraulicTankId = currentHydraulicTank.Id,
                                                        NumberOfPeople = int.Parse(firstSheet.Cells["I" + _row].Text),
                                                        InstantMileage = long.Parse(firstSheet.Cells["J" + _row].Text),
                                                        WarrantyEndDate = DateTime.Parse(firstSheet.Cells["K" + _row].Text),
                                                        Tonnage = int.Parse(firstSheet.Cells["L" + _row].Text),
                                                        UsageType = firstSheet.Cells["M" + _row].Text,
                                                        OwnershipType = firstSheet.Cells["N" + _row].Text,
                                                        VehicleTypeId = currentVehicleType.Id,
                                                        IsGpsTracking = ısTrackingMode,
                                                        FirstRegisterDateTime = DateTime.Parse(firstSheet.Cells["Q" + _row].Text),
                                                        RegisterDateTime = DateTime.Parse(firstSheet.Cells["R" + _row].Text),
                                                        RegisterPaperSerialNumber = firstSheet.Cells["S" + _row].Text,
                                                        RegisterPaperOrderNumber = firstSheet.Cells["T" + _row].Text,
                                                        CaseNumber = firstSheet.Cells["U" + _row].Text,
                                                        EngineNumber = firstSheet.Cells["V" + _row].Text

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
                                    var currentVehicles = context.Vehicles.Where(x => x.IsDeleted == false).ToList();

                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var vehicle in Vehicles)
                                            {
                                                var currentVehicle = currentVehicles.FirstOrDefault(x => x.Id != vehicle.Id);
                                                if (currentVehicle == null)
                                                {
                                                    var newVehicle = new Vehicle
                                                    {
                                                        VehicleTypeId = vehicle.VehicleTypeId,
                                                        BrandId = vehicle.BrandId,
                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                        EngineId = vehicle.EngineId,
                                                        FuelTankId = vehicle.FuelTankId,
                                                        HydraulicTankId = vehicle.HydraulicTankId,
                                                        ModelId = vehicle.ModelId,
                                                        PlateNumber = vehicle.PlateNumber,
                                                        ModelYear = vehicle.ModelYear,
                                                        NumberOfPeople = vehicle.NumberOfPeople,
                                                        WarrantyEndDate = vehicle.WarrantyEndDate,
                                                        InstantMileage = vehicle.InstantMileage,
                                                        IsGpsTracking = vehicle.IsGpsTracking,
                                                        Tonnage = vehicle.Tonnage,
                                                        UsageType = vehicle.UsageType,
                                                        OwnershipType = vehicle.OwnershipType,
                                                        FirstRegisterDateTime = vehicle.FirstRegisterDateTime,
                                                        RegisterDateTime = vehicle.RegisterDateTime,
                                                        RegisterPaperSerialNumber = vehicle.RegisterPaperSerialNumber,
                                                        RegisterPaperOrderNumber = vehicle.RegisterPaperOrderNumber,
                                                        CaseNumber = vehicle.CaseNumber,
                                                        EngineNumber = vehicle.EngineNumber
                                                    };
                                                    context.Vehicles.Add(newVehicle);
                                                }
                                                else
                                                {
                                                    currentVehicle.VehicleTypeId = vehicle.VehicleTypeId;
                                                    currentVehicle.BrandId = vehicle.BrandId;
                                                    currentVehicle.CreatedDateTime = DateTime.Now;
                                                    currentVehicle.CreatedUserId = GetUserId();
                                                    currentVehicle.EngineId = vehicle.EngineId;
                                                    currentVehicle.FuelTankId = vehicle.FuelTankId;
                                                    currentVehicle.HydraulicTankId = vehicle.HydraulicTankId;
                                                    currentVehicle.ModelId = vehicle.ModelId;
                                                    currentVehicle.ModelYear = vehicle.ModelYear;
                                                    currentVehicle.NumberOfPeople = vehicle.NumberOfPeople;
                                                    currentVehicle.WarrantyEndDate = vehicle.WarrantyEndDate;
                                                    currentVehicle.InstantMileage = vehicle.InstantMileage;
                                                    currentVehicle.IsGpsTracking = vehicle.IsGpsTracking;
                                                    currentVehicle.Tonnage = vehicle.Tonnage;
                                                    currentVehicle.UsageType = vehicle.UsageType;
                                                    currentVehicle.OwnershipType = vehicle.OwnershipType;
                                                    currentVehicle.FirstRegisterDateTime = vehicle.FirstRegisterDateTime;
                                                    currentVehicle.RegisterDateTime = vehicle.RegisterDateTime;
                                                    currentVehicle.RegisterPaperSerialNumber = vehicle.RegisterPaperSerialNumber;
                                                    currentVehicle.RegisterPaperOrderNumber = vehicle.RegisterPaperOrderNumber;
                                                    currentVehicle.CaseNumber = vehicle.CaseNumber;
                                                    currentVehicle.EngineNumber = vehicle.EngineNumber;

                                                    context.Entry(currentVehicle).State = EntityState.Modified;
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

        [HttpPost]
        public ActionResult<Result<Vehicle>> Create([FromBody] VehicleModel vehicleModel)
        {
            var response = new Result<Vehicle> { Meta = new Meta() };

            try
            {
                var vehicle = new Vehicle
                {
                    VehicleTypeId = vehicleModel.VehicleTypeId,
                    BrandId = vehicleModel.BrandId,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                    EngineId = vehicleModel.EngineId,
                    FuelTankId = vehicleModel.FuelTankId,
                    HydraulicTankId = vehicleModel.HydraulicTankId,
                    ModelId = vehicleModel.ModelId,
                    PlateNumber = vehicleModel.PlateNumber,
                    ModelYear = vehicleModel.ModelYear,
                    NumberOfPeople = vehicleModel.NumberOfPeople,
                    WarrantyEndDate = vehicleModel.WarrantyEndDate,
                    InstantMileage = vehicleModel.InstantMileage,
                    IsGpsTracking = vehicleModel.IsGpsTracking,
                    Tonnage = vehicleModel.Tonnage,
                    UsageType = vehicleModel.UsageType,
                    OwnershipType = vehicleModel.OwnershipType,
                    FirstRegisterDateTime = vehicleModel.FirstRegisterDateTime,
                    RegisterDateTime = vehicleModel.RegisterDateTime,
                    RegisterPaperSerialNumber = vehicleModel.RegisterPaperSerialNumber,
                    RegisterPaperOrderNumber = vehicleModel.RegisterPaperOrderNumber,
                    CaseNumber = vehicleModel.CaseNumber,
                    EngineNumber = vehicleModel.EngineNumber,
                    TireCount = vehicleModel.TireCount
                };
                context.Vehicles.Add(vehicle);
                context.SaveChanges();

                response.Entity = new Vehicle();

                var newVehicle = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Include(x => x.Engine).Include(x => x.FuelTank).Include(x => x.HydraulicTank).Include(x => x.VehicleType).FirstOrDefault(x => x.CreatedDateTime == vehicle.CreatedDateTime);

                response.Entity.Id = newVehicle.Id;
                response.Entity.VehicleTypeId = newVehicle.VehicleTypeId;
                response.Entity.VehicleType = newVehicle.VehicleType;
                response.Entity.BrandId = newVehicle.BrandId;
                response.Entity.Brand = newVehicle.Brand;
                response.Entity.CreatedDateTime = newVehicle.CreatedDateTime;
                response.Entity.CreatedUserId = newVehicle.CreatedUserId;
                response.Entity.EngineId = newVehicle.EngineId;
                response.Entity.Engine = newVehicle.Engine;
                response.Entity.FuelTankId = newVehicle.FuelTankId;
                response.Entity.FuelTank = newVehicle.FuelTank;
                response.Entity.HydraulicTankId = newVehicle.HydraulicTankId;
                response.Entity.HydraulicTank = newVehicle.HydraulicTank;
                response.Entity.ModelId = newVehicle.ModelId;
                response.Entity.Model = newVehicle.Model;
                response.Entity.PlateNumber = newVehicle.PlateNumber;
                response.Entity.ModelYear = newVehicle.ModelYear;
                response.Entity.NumberOfPeople = newVehicle.NumberOfPeople;
                response.Entity.WarrantyEndDate = newVehicle.WarrantyEndDate;
                response.Entity.InstantMileage = newVehicle.InstantMileage;
                response.Entity.IsGpsTracking = newVehicle.IsGpsTracking;
                response.Entity.Tonnage = newVehicle.Tonnage;
                response.Entity.UsageType = newVehicle.UsageType;
                response.Entity.OwnershipType = newVehicle.OwnershipType;
                response.Entity.FirstRegisterDateTime = newVehicle.FirstRegisterDateTime;
                response.Entity.RegisterDateTime = newVehicle.RegisterDateTime;
                response.Entity.RegisterPaperSerialNumber = newVehicle.RegisterPaperSerialNumber;
                response.Entity.RegisterPaperOrderNumber = newVehicle.RegisterPaperOrderNumber;
                response.Entity.CaseNumber = newVehicle.CaseNumber;
                response.Entity.EngineNumber = newVehicle.EngineNumber;
                response.Entity.TireCount = newVehicle.TireCount;

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

        [HttpPut("{id}")]
        public ActionResult<Result<Vehicle>> Update(int id, [FromBody] VehicleModel vehicleModel)
        {
            var response = new Result<Vehicle> { Meta = new Meta() };

            try
            {
                var vehicle = context.Vehicles.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (vehicle != null)
                {
                    vehicle.VehicleTypeId = vehicleModel.VehicleTypeId;
                    vehicle.BrandId = vehicleModel.BrandId;
                    vehicle.UpdatedDateTime = DateTime.Now;
                    vehicle.UpdatedUserId = GetUserId();
                    vehicle.EngineId = vehicleModel.EngineId;
                    vehicle.FuelTankId = vehicleModel.FuelTankId;
                    vehicle.HydraulicTankId = vehicleModel.HydraulicTankId;
                    vehicle.ModelId = vehicleModel.ModelId;
                    vehicle.PlateNumber = vehicleModel.PlateNumber;
                    vehicle.ModelYear = vehicleModel.ModelYear;
                    vehicle.NumberOfPeople = vehicleModel.NumberOfPeople;
                    vehicle.WarrantyEndDate = vehicleModel.WarrantyEndDate;
                    vehicle.InstantMileage = vehicleModel.InstantMileage;
                    vehicle.IsGpsTracking = vehicleModel.IsGpsTracking;
                    vehicle.Tonnage = vehicleModel.Tonnage;
                    vehicle.UsageType = vehicleModel.UsageType;
                    vehicle.OwnershipType = vehicleModel.OwnershipType;
                    vehicle.FirstRegisterDateTime = vehicleModel.FirstRegisterDateTime;
                    vehicle.RegisterDateTime = vehicleModel.RegisterDateTime;
                    vehicle.RegisterPaperSerialNumber = vehicleModel.RegisterPaperSerialNumber;
                    vehicle.RegisterPaperOrderNumber = vehicleModel.RegisterPaperOrderNumber;
                    vehicle.CaseNumber = vehicleModel.CaseNumber;
                    vehicle.EngineNumber = vehicleModel.EngineNumber;
                    vehicle.TireCount = vehicleModel.TireCount;

                    context.Entry(vehicle).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Vehicle();

                    vehicle = context.Vehicles.Include(x => x.Brand).Include(x => x.Model).Include(x => x.Engine).Include(x => x.FuelTank).Include(x => x.HydraulicTank).Include(x => x.VehicleType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = vehicle.Id;
                    response.Entity.VehicleTypeId = vehicle.VehicleTypeId;
                    response.Entity.VehicleType = vehicle.VehicleType;
                    response.Entity.BrandId = vehicle.BrandId;
                    response.Entity.Brand = vehicle.Brand;
                    response.Entity.UpdatedDateTime = vehicle.UpdatedDateTime;
                    response.Entity.UpdatedUserId = vehicle.UpdatedUserId;
                    response.Entity.EngineId = vehicle.EngineId;
                    response.Entity.Engine = vehicle.Engine;
                    response.Entity.FuelTankId = vehicle.FuelTankId;
                    response.Entity.FuelTank = vehicle.FuelTank;
                    response.Entity.HydraulicTankId = vehicle.HydraulicTankId;
                    response.Entity.HydraulicTank = vehicle.HydraulicTank;
                    response.Entity.ModelId = vehicle.ModelId;
                    response.Entity.Model = vehicle.Model;
                    response.Entity.PlateNumber = vehicle.PlateNumber;
                    response.Entity.ModelYear = vehicle.ModelYear;
                    response.Entity.NumberOfPeople = vehicle.NumberOfPeople;
                    response.Entity.WarrantyEndDate = vehicle.WarrantyEndDate;
                    response.Entity.InstantMileage = vehicle.InstantMileage;
                    response.Entity.IsGpsTracking = vehicle.IsGpsTracking;
                    response.Entity.Tonnage = vehicle.Tonnage;
                    response.Entity.UsageType = vehicle.UsageType;
                    response.Entity.OwnershipType = vehicle.OwnershipType;
                    response.Entity.FirstRegisterDateTime = vehicle.FirstRegisterDateTime;
                    response.Entity.RegisterDateTime = vehicle.RegisterDateTime;
                    response.Entity.RegisterPaperSerialNumber = vehicle.RegisterPaperSerialNumber;
                    response.Entity.RegisterPaperOrderNumber = vehicle.RegisterPaperOrderNumber;
                    response.Entity.CaseNumber = vehicle.CaseNumber;
                    response.Entity.EngineNumber = vehicle.EngineNumber;
                    response.Entity.TireCount = vehicle.TireCount;

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
        public ActionResult<Result<Vehicle>> Delete(int id)
        {

            var response = new Result<Vehicle> { Meta = new Meta() };
            try
            {
                var vehicle = context.Vehicles.FirstOrDefault(c => c.Id == id);
                vehicle.IsDeleted = true;
                context.Entry(vehicle).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = vehicle;

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

    public class VehicleFilterModel
    {
        public string keyword { get; set; }
    }
}