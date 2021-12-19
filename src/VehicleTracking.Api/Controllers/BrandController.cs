using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;
using VehicleTracking.Entity.Persistence;

namespace VehicleTracking.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : BaseController
    {
        public BrandController(VehicleTrackingDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public ActionResult<Result<Brand>> GetAll()
        {
            var response = new Result<Brand> { Meta = new Meta() };
            try
            {
                var brands = context.Brands.Where(x => x.IsDeleted == false).ToList();
                response.Entities = brands;
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
        public ActionResult<Result<Brand>> GetById(int id)
        {
            var response = new Result<Brand> { Meta = new Meta() };
            try
            {
                var brand = context.Brands.FirstOrDefault(x => x.Id == id);
                response.Entity = brand;
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
        [HttpPost("create")]
        public ActionResult<Result<Brand>> Create([FromBody] BrandModel brandModel)
        {
            var response = new Result<Brand> { Meta = new Meta() };
          
            try
            {
                var brand = new Brand
                {
                    Name = brandModel.Name,
                    CreatedDateTime = DateTime.Now,
                    CreatedUserId = GetUserId(),
                };
                context.Brands.Add(brand);
                context.SaveChanges();
                response.Entity = brand;
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
        public ActionResult<Result<Brand>> Update(int id, [FromBody] BrandModel brandModel)
        {
            var response = new Result<Brand> { Meta = new Meta() };

            try
            {
                var brand = context.Brands.FirstOrDefault(x => x.IsDeleted == false && x.Id == id);
                if (brand != null)
                {
                    brand.Name = brandModel.Name;
                    brand.UpdatedDateTime = DateTime.Now;
                    brand.UpdatedUserId = GetUserId();


                    context.Entry(brand).State = EntityState.Modified;
                    context.SaveChanges();
                    response.Meta.IsSuccess = true;
                    response.Entity = brand;
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
        public ActionResult<Result<Brand>> Delete(int id)
        {

            var response = new Result<Brand> { Meta = new Meta() };
            try
            {
                var brand = context.Brands.FirstOrDefault(c => c.Id == id);
                brand.IsDeleted = true;
                context.Entry(brand).State = EntityState.Modified;
                context.SaveChanges();
                response.Meta.IsSuccess = true;
                response.Entity = brand;

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
