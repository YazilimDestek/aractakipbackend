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
    public class TireController : BaseController
    {
        public TireController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }
        [HttpGet]
        public ActionResult<Result<Tire>> GetAll()
        {
            var response = new Result<Tire> { Meta = new Meta() };
            try
            {
                var tires = context.Tires.Where(x => x.IsDeleted == false).ToList();
                response.Entities = tires;
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
        public ActionResult<Result<Tire>> GetById(int id)
        {
            var response = new Result<Tire> { Meta = new Meta() };
            try
            {
                var tire = context.Tires.FirstOrDefault(x => x.Id == id);
                response.Entity = tire;
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
        public ActionResult<Result<Tire>> Create([FromBody] TireModel tireModel)
        {
            var response = new Result<Tire> { Meta = new Meta() };

            try
            {
                var tire = new Tire
                {
                    Model = tireModel.Model,
                    Figure = tireModel.Figure,
                    Brand = tireModel.Brand,
                    SerialNumber = tireModel.SerialNumber,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                    RimDiameter = tireModel.RimDiameter,
                    Height = tireModel.Height,
                    Width = tireModel.Width
                };
                context.Tires.Add(tire);
                context.SaveChanges();
               
                response.Entity=new Tire();

                var newTire = context.Tires.FirstOrDefault(x => x.CreatedDateTime == tire.CreatedDateTime);

                response.Entity.Id = newTire.Id;
                response.Entity.Model = newTire.Model;
                response.Entity.Figure = newTire.Figure;
                response.Entity.Brand = newTire.Brand;
                response.Entity.SerialNumber = newTire.SerialNumber;
                response.Entity.RimDiameter = newTire.RimDiameter;
                response.Entity.Height = newTire.Height;
                response.Entity.Width = newTire.Width;
                response.Entity.CreatedDateTime = newTire.CreatedDateTime;
                response.Entity.CreatedUserId=newTire.CreatedUserId;

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
        public ActionResult<Result<Tire>> GetByFilter([FromBody] TireModel tireModel)
        {
            var response = new Result<Tire> { Meta = new Meta() };
            try
            {
                var tires = context.Tires.Where(x => x.IsDeleted == false);

                if (!string.IsNullOrEmpty(tireModel.Brand))
                {
                    tires = tires.Where(x => x.Brand == tireModel.Brand);
                }
                if (!string.IsNullOrEmpty(tireModel.Model))
                {
                    tires = tires.Where(x => x.Model == tireModel.Model);
                }
                if (!string.IsNullOrEmpty(tireModel.Figure))
                {
                    tires = tires.Where(x => x.Figure == tireModel.Figure);
                }
                if (!string.IsNullOrEmpty(tireModel.SerialNumber))
                {
                    tires = tires.Where(x => x.SerialNumber == tireModel.SerialNumber);
                }
                response.Entities = tires.ToList();
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
        public async Task<Result<string>> UploadExcelTire()
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
                        var Tires = new List<Tire>();

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
                                                
                                                Tires.Add(
                                                    new Tire
                                                    {
                                                        Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                        Brand = firstSheet.Cells["B" + _row].Text,
                                                        Model = firstSheet.Cells["C" + _row].Text,
                                                        Figure = firstSheet.Cells["D" + _row].Text,
                                                        SerialNumber = firstSheet.Cells["E" + _row].Text,
                                                        RimDiameter = int.Parse(firstSheet.Cells["F" + _row].Text),
                                                        Width = int.Parse(firstSheet.Cells["G" + _row].Text),
                                                        Height = int.Parse(firstSheet.Cells["H" + _row].Text)

                                                    });
                                            }
                                            catch
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
                                    var currentTires = context.Tires.Where(x => x.IsDeleted == false).ToList();
                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var tire in Tires)
                                            {
                                                if (tire.Id == null)
                                                {
                                                    tire.Id = 0;
                                                }
                                                var currentTire = currentTires.FirstOrDefault(x => x.Id != tire.Id && tire.Id > 0);
                                                if (currentTire == null)
                                                {
                                                    var newTire = new Tire
                                                    {
                                                        Brand = tire.Brand,
                                                        Model = tire.Model,
                                                        Figure = tire.Figure,
                                                        SerialNumber = tire.SerialNumber,
                                                        RimDiameter = tire.RimDiameter,
                                                        Width = tire.Width,
                                                        Height = tire.Height,

                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                    };
                                                    context.Tires.Add(newTire);
                                                }
                                                else
                                                {
                                                    currentTire.Brand = tire.Brand;
                                                    currentTire.Model = tire.Model;
                                                    currentTire.Figure = tire.Figure;
                                                    currentTire.SerialNumber = tire.SerialNumber;
                                                    currentTire.RimDiameter = tire.RimDiameter;
                                                    currentTire.Width = tire.Width;
                                                    currentTire.Height = tire.Height;

                                                    currentTire.UpdatedDateTime = DateTime.Now;
                                                    currentTire.UpdatedUserId = GetUserId();

                                                    context.Entry(currentTire).State = EntityState.Modified;
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
        public ActionResult<Result<Tire>> Update(int id, [FromBody] TireModel tireModel)
        {
            var response = new Result<Tire> { Meta = new Meta() };

            try
            {
                var tire = context.Tires.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (tire != null)
                {
                    tire.Model = tireModel.Model;
                    tire.Figure = tireModel.Figure;
                    tire.Brand = tireModel.Brand;
                    tire.SerialNumber = tireModel.SerialNumber;
                    tire.UpdatedDateTime = DateTime.Now;
                    tire.UpdatedUserId = GetUserId();
                    tire.RimDiameter = tireModel.RimDiameter;
                    tire.Height = tireModel.Height;
                    tire.Width = tireModel.Width;


                    context.Entry(tire).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity =new Tire();

                    tire = context.Tires.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = tire.Id;
                    response.Entity.Model = tire.Model;
                    response.Entity.Figure = tire.Figure;
                    response.Entity.Brand = tire.Brand;
                    response.Entity.SerialNumber = tire.SerialNumber;
                    response.Entity.UpdatedDateTime = tire.UpdatedDateTime;
                    response.Entity.UpdatedUserId = tire.UpdatedUserId;
                    response.Entity.RimDiameter = tire.RimDiameter;
                    response.Entity.Height = tire.Height;
                    response.Entity.Width = tire.Width;

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
        public ActionResult<Result<Tire>> Delete(int id)
        {

            var response = new Result<Tire> { Meta = new Meta() };
            try
            {
                var tire = context.Tires.FirstOrDefault(c => c.Id == id);
                tire.IsDeleted = true;
                context.Entry(tire).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = tire;

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