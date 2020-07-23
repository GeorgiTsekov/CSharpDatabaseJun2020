using AutoMapper;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System.Linq;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            ////Import 1
            //this.CreateMap<ImportUserDto, User>();
            
            ////Import 2
            //this.CreateMap<ImportProductDto, Product>();

            ////Import 3
            //this.CreateMap<ImportCategoryDto, Category>();

            ////Import 4
            //this.CreateMap<ImportCategoryProductDto, CategoryProduct>();

            ////Export 5
            //this.CreateMap<Product, ExportProductInRangeDto>()
            //    .ForMember(x => x.BuyerName, y => y.MapFrom(x => x.Buyer.FirstName + " " + x.Buyer.LastName));

            ////Export 6
            //this.CreateMap<Product, ExportSoldProductDto>();
            //this.CreateMap<User, ExportUserWithProductsDto>()
            //    .ForMember(x => x.SoldProducts, y => y.MapFrom(x => x.ProductsSold));

            ////Export 7
            //this.CreateMap<Category, ExportCategoryDto>()
            //    .ForMember(x => x.Count, y => y.MapFrom(x => x.CategoryProducts.Count))
            //    .ForMember(x => x.AveragePrice, y => y.MapFrom(x => x.CategoryProducts.Average(cp => cp.Product.Price)))
            //    .ForMember(x => x.TotalRevenue, y => y.MapFrom(x => x.CategoryProducts.Sum(cp => cp.Product.Price)));

            ////Export 8
            
        }
    }
}
