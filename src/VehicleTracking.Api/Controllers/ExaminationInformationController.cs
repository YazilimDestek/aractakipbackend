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
    public class ExaminationInformationController : BaseController
    {
        public ExaminationInformationController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }
        [HttpGet]
        public ActionResult<Result<ExaminationInformation>> GetAll()
        {
            var response = new Result<ExaminationInformation> { Meta = new Meta() };
            try
            {
                var examinationsInformation = context.ExaminationsInformation.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).Where(x => x.IsDeleted == false).ToList();
                response.Entities = examinationsInformation;
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
        public ActionResult<Result<ExaminationInformation>> GetById(int id)
        {
            var response = new Result<ExaminationInformation> { Meta = new Meta() };
            try
            {
                var examinationInformation = context.ExaminationsInformation.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.Id == id);
                response.Entity = examinationInformation;
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
        public ActionResult<Result<ExaminationInformation>> GetBYFilter([FromBody] ExaminationInformationModel examinationInformationModel)
        {
            var response = new Result<ExaminationInformation> { Meta = new Meta() };
            try
            {
                var examinationInformation = context.ExaminationsInformation.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).Where(x => x.IsDeleted == false);

                if (!string.IsNullOrEmpty(examinationInformationModel.ExaminationResult))
                {
                    examinationInformation = examinationInformation.Where(x => x.ExaminationResult == examinationInformationModel.ExaminationResult);
                }
                if (examinationInformationModel.VehicleId > 0)
                {
                    examinationInformation = examinationInformation.Where(x => x.VehicleId == examinationInformationModel.VehicleId);
                }
                if (examinationInformationModel.ExaminationDate != null)
                {
                    examinationInformation = examinationInformation.Where(x => x.ExaminationDate.Year == examinationInformationModel.ExaminationDate.Year&& x.ExaminationDate.Month == examinationInformationModel.ExaminationDate.Month&& x.ExaminationDate.Day == examinationInformationModel.ExaminationDate.Day);
                }

                response.Entities = examinationInformation.ToList();
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
        public async Task<Result<string>> UploadExcelExaminationInformation()
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
                        var ExaminationInformations = new List<ExaminationInformation>();
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //EXCEL PARSe
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
                                                ExaminationInformations.Add(
                                                    new ExaminationInformation
                                                    {
                                                        Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                        VehicleId = currentVehicle.Id,
                                                        ExaminationResult = firstSheet.Cells["C" + _row].Text,
                                                        ExaminationDate = DateTime.Parse(firstSheet.Cells["D" + _row].Text),
                                                        ExaminationResultDocument = firstSheet.Cells["E" + _row].Text
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
                                    var currentExaminationInformations = context.ExaminationsInformation.Where(x => x.IsDeleted == false);
                                    
                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var examinationInformation in ExaminationInformations)
                                            {
                                                if (examinationInformation.Id == null)
                                                {
                                                    examinationInformation.Id = 0;
                                                }

                                                var currentExaminationInformation = currentExaminationInformations.FirstOrDefault(x => x.Id != examinationInformation.Id && examinationInformation.Id > 0);
                                                if (currentExaminationInformation == null)
                                                {
                                                    var newExaminationInformation = new ExaminationInformation
                                                    {
                                                        VehicleId = examinationInformation.VehicleId,
                                                        ExaminationDate = examinationInformation.ExaminationDate,
                                                        ExaminationResult = examinationInformation.ExaminationResult,
                                                        ExaminationResultDocument = examinationInformation.ExaminationResultDocument,

                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                    };
                                                    context.ExaminationsInformation.Add(newExaminationInformation);
                                                }
                                                else
                                                {
                                                    currentExaminationInformation.VehicleId = examinationInformation.VehicleId;
                                                    currentExaminationInformation.ExaminationDate = examinationInformation.ExaminationDate;
                                                    currentExaminationInformation.ExaminationResult = examinationInformation.ExaminationResult;
                                                    currentExaminationInformation.ExaminationResultDocument = examinationInformation.ExaminationResultDocument;

                                                    currentExaminationInformation.UpdatedUserId = GetUserId();
                                                    currentExaminationInformation.UpdatedDateTime = DateTime.Now;

                                                    context.Entry(currentExaminationInformation).State = EntityState.Modified;
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
        public ActionResult<Result<ExaminationInformation>> Create([FromBody] ExaminationInformationModel examinationInformationModel)
        {
            var response = new Result<ExaminationInformation> { Meta = new Meta() };

            try
            {
                var examinationInformation = new ExaminationInformation
                {
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                    VehicleId = examinationInformationModel.VehicleId,
                    ExaminationDate = examinationInformationModel.ExaminationDate,
                    ExaminationResult = examinationInformationModel.ExaminationResult,
                    ExaminationResultDocument = examinationInformationModel.ExaminationResultDocument,
                };

                context.ExaminationsInformation.Add(examinationInformation);
                context.SaveChanges();

                response.Entity=new ExaminationInformation();

                var newExaminationInformation =
                    context.ExaminationsInformation.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x =>
                        x.CreatedDateTime == examinationInformation.CreatedDateTime);

                response.Entity.Id = newExaminationInformation.Id;
                response.Entity.Vehicle = newExaminationInformation.Vehicle;
                response.Entity.VehicleId = newExaminationInformation.VehicleId;
                response.Entity.ExaminationDate = newExaminationInformation.ExaminationDate;
                response.Entity.ExaminationResult = newExaminationInformation.ExaminationResult;
                response.Entity.ExaminationResultDocument = newExaminationInformation.ExaminationResultDocument;
                response.Entity.CreatedUserId = newExaminationInformation.CreatedUserId;
                response.Entity.CreatedDateTime = newExaminationInformation.CreatedDateTime;

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
        public ActionResult<Result<ExaminationInformation>> Update(int id, [FromBody] ExaminationInformationModel examinationInformationModel)
        {
            var response = new Result<ExaminationInformation> { Meta = new Meta() };

            try
            {
                var examinationInformation = context.ExaminationsInformation.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (examinationInformation != null)
                {
                    examinationInformation.UpdatedDateTime = DateTime.Now;
                    examinationInformation.UpdatedUserId = GetUserId();
                    examinationInformation.ExaminationResult = examinationInformationModel.ExaminationResult;
                    examinationInformation.ExaminationDate = examinationInformationModel.ExaminationDate;
                    examinationInformation.ExaminationResultDocument = examinationInformationModel.ExaminationResultDocument;
                    examinationInformation.VehicleId = examinationInformationModel.VehicleId;

                    context.Entry(examinationInformation).State = EntityState.Modified;
                    context.SaveChanges();
                    
                    response.Entity = new ExaminationInformation();
                    
                    examinationInformation = context.ExaminationsInformation.Include(x => x.Vehicle).Include(x => x.Vehicle.VehicleType).Include(x => x.Vehicle.HydraulicTank).Include(x => x.Vehicle.Engine).Include(x => x.Vehicle.FuelTank).Include(x => x.Vehicle.Brand).Include(x => x.Vehicle.Model).FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = examinationInformation.Id;
                    response.Entity.VehicleId = examinationInformation.VehicleId;
                    response.Entity.Vehicle = examinationInformation.Vehicle;
                    response.Entity.ExaminationResult = examinationInformation.ExaminationResult;
                    response.Entity.ExaminationDate = examinationInformation.ExaminationDate;
                    response.Entity.ExaminationResultDocument = examinationInformation.ExaminationResultDocument;
                    response.Entity.UpdatedDateTime = examinationInformation.UpdatedDateTime;
                    response.Entity.UpdatedUserId = examinationInformation.UpdatedUserId;


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
        public ActionResult<Result<ExaminationInformation>> Delete(int id)
        {

            var response = new Result<ExaminationInformation> { Meta = new Meta() };
            try
            {
                var examinationInformation = context.ExaminationsInformation.FirstOrDefault(c => c.Id == id);
                examinationInformation.IsDeleted = true;
                context.Entry(examinationInformation).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = examinationInformation;

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