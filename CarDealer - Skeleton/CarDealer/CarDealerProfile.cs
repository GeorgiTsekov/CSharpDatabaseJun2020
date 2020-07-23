using AutoMapper;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Linq;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            ////Import
            ////9
            //this.CreateMap<ImportSupplierDto, Supplier>();

            ////10
            //this.CreateMap<ImportPartDto, Part>();

            ////12
            //this.CreateMap<ImportCustomerDto, Customer>();

            ////13
            //this.CreateMap<ImportSaleDto, Sale>();

            ////Export
            ////14
            //this.CreateMap<Car, ExportCarWithDistanceDto>();

            ////15
            //this.CreateMap<Car, ExportCarsFromMakeBmwDto>();

            ////16
            //this.CreateMap<Supplier, ExportLocalSuppliersDto>()
            //    .ForMember(x => x.PartsCount, y => y.MapFrom(x => x.Parts.Count));

            ////17
            //this.CreateMap<Part, ExportCarPartDto>();
            //this.CreateMap<Car, ExportCarWithTheirListOfPartsDto>()
            //    .ForMember(x => x.Parts, y => y.MapFrom(x => x.PartCars
            //        .Select(pc => pc.Part)
            //        .OrderByDescending(pc => pc.Price)));

            ////18
            //this.CreateMap<Customer, ExportCustomerDto>()
            //    .ForMember(x => x.BougthCars, y => y.MapFrom(x => x.Sales.Count))
            //    .ForMember(x => x.SpentMoney, y => y.MapFrom(x => x.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price))));

            ////19
            //this.CreateMap<Car, ExportCarDto>();
            //this.CreateMap<Sale, ExportSaleDto>()
            //    .ForMember(x => x.Price, y => y.MapFrom(x => x.Car.PartCars.Sum(pc => pc.Part.Price)))
            //    .ForMember(x => x.PriceWithDiscount, y => y.MapFrom(x => decimal.Round(x.Car.PartCars.Sum(pc => pc.Part.Price) * (1M - x.Discount / 100), 2, MidpointRounding.AwayFromZero)));
        }
    }
}
