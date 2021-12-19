using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Core.ExcelPackage;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace VehicleTracking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : BaseController
    {
        public StaffController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }
        [HttpGet]
        public ActionResult<Result<Staff>> GetAll()
        {
            var response = new Result<Staff> { Meta = new Meta() };
            try
            {
                var staffs = context.Staffs.Where(x => x.IsDeleted == false).ToList();
                response.Entities = staffs;
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
        public ActionResult<Result<Staff>> GetById(int id)
        {
            var response = new Result<Staff> { Meta = new Meta() };
            try
            {
                var staff = context.Staffs.FirstOrDefault(x => x.Id == id);
                response.Entity = staff;
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
        public ActionResult<Result<Staff>> Create([FromBody] StaffModel staffModel)
        {
            var response = new Result<Staff> { Meta = new Meta() };

            try
            {
                var staff = new Staff
                {
                    Name = staffModel.Name,
                    Surname = staffModel.Surname,
                    UserId = GetUserId(),
                    Email = staffModel.Email,
                    Phone = staffModel.Phone,
                    Department = staffModel.Department,
                    Position = staffModel.Position,
                    AbysCode = staffModel.AbysCode,
                    IsTrackingEnable = staffModel.IsTrackingEnable,

                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.Staffs.Add(staff);
                context.SaveChanges();

                response.Entity = new Staff();

                var newStaff = context.Staffs.FirstOrDefault(x => x.CreatedDateTime == staff.CreatedDateTime);

                response.Entity.Id = newStaff.Id;
                response.Entity.Name = newStaff.Name;
                response.Entity.Surname = newStaff.Surname;
                response.Entity.UserId = newStaff.UserId;
                response.Entity.Email = newStaff.Email;
                response.Entity.Phone = newStaff.Phone;
                response.Entity.Department = newStaff.Department;
                response.Entity.Position = newStaff.Position;
                response.Entity.AbysCode = newStaff.AbysCode;
                response.Entity.IsTrackingEnable = newStaff.IsTrackingEnable;

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
        public ActionResult<Result<Staff>> GetByFilter([FromBody] StaffModel staffModel)
        {
            var response = new Result<Staff> { Meta = new Meta() };

            try
            {
                var staffs = context.Staffs.Include(x => x.User).Where(x => x.IsDeleted == false);

                if (!string.IsNullOrEmpty(staffModel.Name))
                {
                    staffs = staffs.Where(x => x.Name == staffModel.Name);
                }
                if (!string.IsNullOrEmpty(staffModel.Surname))
                {
                    staffs = staffs.Where(x => x.Surname == staffModel.Surname);
                }
                if (!string.IsNullOrEmpty(staffModel.Department))
                {
                    staffs = staffs.Where(x => x.Department == staffModel.Department);
                }
                if (!string.IsNullOrEmpty(staffModel.Position))
                {
                    staffs = staffs.Where(x => x.Position == staffModel.Position);
                }
                if (!string.IsNullOrEmpty(staffModel.AbysCode))
                {
                    staffs = staffs.Where(x => x.AbysCode == staffModel.AbysCode);
                }
                response.Entities = staffs.ToList();
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
        public async Task<Result<string>> UploadExcelStaff()
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
                        var Staffs = new List<Staff>();

                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //EXCEL PARSE
                            using (var package = new OfficeOpenXml.ExcelPackage(ms))
                            {
                                OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                                                bool ısTrackingMode = false;
                                                if ((!string.IsNullOrEmpty(firstSheet.Cells["I" + _row].Text) || firstSheet.Cells["I" + _row].Text != "") && (firstSheet.Cells["I" + _row].Text.ToLower() == "var"))
                                                {
                                                    ısTrackingMode = true;
                                                }
                                                else
                                                {
                                                    ısTrackingMode = false;
                                                }
                                                Staffs.Add(
                                                    new Staff
                                                    {
                                                        Id = int.Parse("0" + firstSheet.Cells["A" + _row].Text),
                                                        Name = firstSheet.Cells["B" + _row].Text,
                                                        Surname = firstSheet.Cells["C" + _row].Text,
                                                        Email = firstSheet.Cells["D" + _row].Text,
                                                        Phone = firstSheet.Cells["E" + _row].Text,
                                                        UserId = GetUserId(),
                                                        Department = firstSheet.Cells["F" + _row].Text,
                                                        Position = firstSheet.Cells["G" + _row].Text,
                                                        AbysCode = firstSheet.Cells["H" + _row].Text,
                                                        IsTrackingEnable = ısTrackingMode,
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
                                    var currentStaffs = context.Staffs.Where(x => x.IsDeleted == false).ToList();

                                    using (var dbContextTransaction = context.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            foreach (var staff in Staffs)
                                            {
                                                if (staff.Id == null)
                                                {
                                                    staff.Id = 0;
                                                }

                                                var currentStaff = currentStaffs.FirstOrDefault(x => x.Id != staff.Id && staff.Id > 0);
                                                if (currentStaff == null)
                                                {
                                                    var newStaff = new Staff
                                                    {
                                                        UserId = staff.UserId,
                                                        Name = staff.Name,
                                                        Surname = staff.Surname,
                                                        Email = staff.Email,
                                                        Phone = staff.Phone,
                                                        Department = staff.Department,
                                                        Position = staff.Position,
                                                        AbysCode = staff.AbysCode,
                                                        IsTrackingEnable = staff.IsTrackingEnable,

                                                        CreatedDateTime = DateTime.Now,
                                                        CreatedUserId = GetUserId(),
                                                    };

                                                    context.Staffs.Add(newStaff);

                                                }
                                                else
                                                {
                                                    currentStaff.UserId = staff.UserId;
                                                    currentStaff.Name = staff.Name;
                                                    currentStaff.Surname = staff.Surname;
                                                    currentStaff.Email = staff.Email;
                                                    currentStaff.Phone = staff.Phone;
                                                    currentStaff.Department = staff.Department;
                                                    currentStaff.Position = staff.Position;
                                                    currentStaff.AbysCode = staff.AbysCode;
                                                    currentStaff.IsTrackingEnable = staff.IsTrackingEnable;

                                                    currentStaff.UpdatedDateTime = DateTime.Now;
                                                    currentStaff.UpdatedUserId = GetUserId();

                                                    context.Entry(currentStaff).State = EntityState.Modified;
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
        public ActionResult<Result<Staff>> Update(int id, [FromBody] StaffModel staffModel)
        {
            var response = new Result<Staff> { Meta = new Meta() };

            try
            {
                var staff = context.Staffs.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (staff != null)
                {
                    staff.Name = staffModel.Name;
                    staff.UserId = GetUserId();
                    staff.Email = staffModel.Email;
                    staff.Surname = staffModel.Surname;
                    staff.Phone = staffModel.Phone;
                    staff.Department = staffModel.Department;
                    staff.Position = staffModel.Position;
                    staff.AbysCode = staffModel.AbysCode;
                    staff.IsTrackingEnable = staffModel.IsTrackingEnable;

                    staff.UpdatedDateTime = DateTime.Now;
                    staff.UpdatedUserId = GetUserId();

                    context.Entry(staff).State = EntityState.Modified;
                    context.SaveChanges();

                    response.Entity = new Staff();

                    staff = context.Staffs.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);

                    response.Entity.Id = staff.Id;
                    response.Entity.Name = staff.Name;
                    response.Entity.UserId = staff.UserId;
                    response.Entity.Email = staff.Email;
                    response.Entity.Surname = staff.Surname;
                    response.Entity.Phone = staff.Phone;
                    response.Entity.Department = staff.Department;
                    response.Entity.Position = staff.Position;
                    response.Entity.AbysCode = staff.AbysCode;
                    response.Entity.IsTrackingEnable = staff.IsTrackingEnable;
                    response.Entity.UpdatedDateTime = staff.UpdatedDateTime;
                    response.Entity.UpdatedUserId = staff.UpdatedUserId;

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
        public ActionResult<Result<Staff>> Delete(int id)
        {

            var response = new Result<Staff> { Meta = new Meta() };
            try
            {
                var staff = context.Staffs.FirstOrDefault(c => c.Id == id);
                staff.IsDeleted = true;
                context.Entry(staff).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = staff;

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