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
    public class InsuranceController : BaseController
    {
        public InsuranceController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Insurance>> GetAll()
        {
            var response = new Result<Insurance> { Meta = new Meta() };
            try
            {
                var insurances = context.Insurances.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Where(x => x.IsDeleted == false).ToList();
                response.Entities = insurances;
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
        public ActionResult<Result<Insurance>> GetById(int id)
        {
            var response = new Result<Insurance> { Meta = new Meta() };
            try
            {
                var insurance = context.Insurances.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).FirstOrDefault(x => x.Id == id);
                response.Entity = insurance;
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
        public ActionResult<Result<Insurance>> GetByFilter([FromBody] InsuranceModel insuranceModel)
        {
            var response = new Result<Insurance> { Meta = new Meta() };

            try
            {
                var insurances = context.Insurances.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).Where(x => x.IsDeleted == false);

                if (!string.IsNullOrEmpty(insuranceModel.InsuranceFirm))
                {
                    insurances = insurances.Where(x => x.InsuranceFirm == insuranceModel.InsuranceFirm);
                }
                if (!string.IsNullOrEmpty(insuranceModel.InsuranceType))
                {
                    insurances = insurances.Where(x => x.InsuranceType == insuranceModel.InsuranceType);
                }
                if (insuranceModel.VehicleId > 0)
                {
                    insurances = insurances.Where(x => x.VehicleId == insuranceModel.VehicleId);
                }
                if (insuranceModel.DateStart != null)
                {
                    insurances = insurances.Where(x => x.DateStart >= insuranceModel.DateStart);
                }
                if (insuranceModel.DateEnd != null)
                {
                    insurances = insurances.Where(x => x.DateEnd <= insuranceModel.DateEnd);
                }
                response.Entities = insurances.ToList();
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
        public async Task<Result<string>> UploadExcelInsurance()
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
                        var Insurances = new List<Insurance>();

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
                                                Insurances.Add(
                                                    new Insurance
                                                    {
                                                        Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                        VehicleId = currentVehicle.Id,
                                                        InsuranceFirm = firstSheet.Cells["C" + _row].Text,
                                                        InsuranceType = firstSheet.Cells["D" + _row].Text,
                                                        DateStart = DateTime.Parse(firstSheet.Cells["E" + _row].Text),
                                                        DateEnd = DateTime.Parse(firstSheet.Cells["F" + _row].Text)
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
                                    var currentInsurances = context.Insurances.Where(x => x.IsDeleted == false).ToList();
                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var insurance in Insurances)
                                            {
                                                if (insurance.Id == null)
                                                {
                                                    insurance.Id = 0;
                                                }

                                                var currentInsurance = currentInsurances.FirstOrDefault(x => x.Id != insurance.Id && insurance.Id > 0);
                                                if (currentInsurance == null)
                                                {
                                                    var newInsurance = new Insurance
                                                    {
                                                        InsuranceFirm = insurance.InsuranceFirm,
                                                        InsuranceType = insurance.InsuranceType,
                                                        DateEnd = insurance.DateEnd,
                                                        DateStart = insurance.DateStart,
                                                        VehicleId = insurance.VehicleId,

                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                    };

                                                    context.Insurances.Add(newInsurance);
                                                }
                                                else
                                                {
                                                    currentInsurance.InsuranceFirm = insurance.InsuranceFirm;
                                                    currentInsurance.InsuranceType = insurance.InsuranceType;
                                                    currentInsurance.DateEnd = insurance.DateEnd;
                                                    currentInsurance.DateStart = insurance.DateStart;

                                                    currentInsurance.UpdatedDateTime = DateTime.Now;
                                                    currentInsurance.UpdatedUserId = GetUserId();

                                                    context.Entry(currentInsurance).State = EntityState.Modified;
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
        public ActionResult<Result<Insurance>> Create([FromBody] InsuranceModel insuranceModel)
        {
            var response = new Result<Insurance> { Meta = new Meta() };

            try
            {
                var insurance = new Insurance
                {
                    VehicleId = insuranceModel.VehicleId,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                    DateEnd = insuranceModel.DateEnd,
                    DateStart = insuranceModel.DateStart,
                    InsuranceFirm = insuranceModel.InsuranceFirm,
                    InsuranceType = insuranceModel.InsuranceType
                };
                context.Insurances.Add(insurance);
                context.SaveChanges();

                response.Entity = new Insurance();

                var newInsurance = context.Insurances.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.CreatedDateTime == insurance.CreatedDateTime);

                response.Entity.Id = newInsurance.Id;
                response.Entity.VehicleId = newInsurance.VehicleId;
                response.Entity.Vehicle = newInsurance.Vehicle;
                response.Entity.DateStart = newInsurance.DateStart;
                response.Entity.DateEnd = newInsurance.DateEnd;
                response.Entity.InsuranceFirm = newInsurance.InsuranceFirm;
                response.Entity.InsuranceType = newInsurance.InsuranceType;
                response.Entity.CreatedDateTime = newInsurance.CreatedDateTime;
                response.Entity.CreatedUserId = newInsurance.CreatedUserId;

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
        public ActionResult<Result<Insurance>> Update(int id, [FromBody] InsuranceModel insuranceModel)
        {
            var response = new Result<Insurance> { Meta = new Meta() };

            try
            {
                var insurance = context.Insurances.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (insurance != null)
                {
                    insurance.VehicleId = insuranceModel.VehicleId;
                    insurance.DateStart = insuranceModel.DateStart;
                    insurance.DateEnd = insuranceModel.DateEnd;
                    insurance.InsuranceType = insuranceModel.InsuranceType;
                    insurance.InsuranceFirm = insuranceModel.InsuranceFirm;
                    insurance.UpdatedDateTime = DateTime.Now;
                    insurance.UpdatedUserId = GetUserId();

                    context.Entry(insurance).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Insurance();

                    insurance = context.Insurances.Include(x => x.Vehicle).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Model).Include(x => x.Vehicle.VehicleType).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = insurance.Id;
                    response.Entity.VehicleId = insurance.VehicleId;
                    response.Entity.Vehicle = insurance.Vehicle;
                    response.Entity.DateStart = insurance.DateStart;
                    response.Entity.DateEnd = insurance.DateEnd;
                    response.Entity.InsuranceFirm = insurance.InsuranceFirm;
                    response.Entity.InsuranceType = insurance.InsuranceType;
                    response.Entity.UpdatedDateTime = insurance.UpdatedDateTime;
                    response.Entity.UpdatedUserId = insurance.UpdatedUserId;

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
        public ActionResult<Result<Insurance>> Delete(int id)
        {

            var response = new Result<Insurance> { Meta = new Meta() };
            try
            {
                var insurance = context.Insurances.FirstOrDefault(c => c.Id == id);
                insurance.IsDeleted = true;
                context.Entry(insurance).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = insurance;

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