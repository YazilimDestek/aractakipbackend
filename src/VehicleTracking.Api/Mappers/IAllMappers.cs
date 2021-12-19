using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using VehicleTracking.Api.Models;
using VehicleTracking.Entity.Models;

namespace VehicleTracking.Api.Mappers
{
    public interface IAllMappers
    {
        Vehicle MapVehicle(VehicleModel vehicleModel);
    }

    public class AllMappers: IAllMappers
    {
        private readonly IMapper _mapper;

        public AllMappers()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VehicleModel, Vehicle>()
                    .ForMember(dst => dst.Id,
                        opt =>
                            opt.MapFrom(x => x.NumberOfPeople.ToString()));
            });
            _mapper = config.CreateMapper();

        }

        public Vehicle MapVehicle(VehicleModel vehicleModel)
        {
            throw new NotImplementedException();
        }
    }
}
