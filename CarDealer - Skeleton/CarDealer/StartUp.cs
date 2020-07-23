using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using CarDealer.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using CarDealerContext context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var inputXml = File.ReadAllText("./../../../Datasets/sales.xml");

            //var result = ImportSales(context, inputXml);
            var result = GetSalesWithAppliedDiscount(context);

            Console.WriteLine(result);
        }

        //Query 9. Import Suppliers
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            const string rootElement = "Suppliers";

            var dtoResult = XMLConverter.Deserializer<ImportSupplierDto>(inputXml, rootElement);

            var suppliers = dtoResult
                .Select(x => new Supplier
                {
                    Name = x.Name,
                    IsImporter = x.IsImporter
                })
                .ToArray();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
        }

        //Query 10. Import Parts
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            const string rootElement = "Parts";

            var dtoResult = XMLConverter.Deserializer<ImportPartDto>(inputXml, rootElement);

            var parts = dtoResult
                .Where(p => context.Suppliers.Any(s => s.Id == p.SupplierId))
                .Select(x => new Part
                {
                    Name = x.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    SupplierId = x.SupplierId
                })
                .ToArray();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Length}";
        }

        //Query 11. Import Cars
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            const string rootElement = "Cars";

            var dtoResult = XMLConverter.Deserializer<ImportCarDto>(inputXml, rootElement);

            List<Car> cars = new List<Car>();
            List<PartCar> partCars = new List<PartCar>();

            foreach (var carDto in dtoResult)
            {
                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance
                };

                var parts = carDto
                    .Parts
                    .Where(pdto => context.Parts.Any(p => p.Id == pdto.Id))
                    .Select(p => p.Id)
                    .Distinct();

                foreach (var partId in parts)
                {
                    var partCar = new PartCar()
                    {
                        PartId = partId,
                        Car = car
                    };

                    partCars.Add(partCar);
                }

                cars.Add(car);
            }

            context.Cars.AddRange(cars);
            context.PartCars.AddRange(partCars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        //Query 12. Import Customers
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            const string rootElement = "Customers";

            var dtoResult = XMLConverter.Deserializer<ImportCustomerDto>(inputXml, rootElement);

            var customers = dtoResult
                .Select(x => new Customer
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate,
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}";
        }

        //Query 13. Import Sales
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            const string rootElement = "Sales";

            var dtoResult = XMLConverter.Deserializer<ImportSaleDto>(inputXml, rootElement);

            var sales = dtoResult
                .Where(s => context.Cars.Any(c => c.Id == s.CarId))
                .Select(x => new Sale
                {
                    Discount = x.Discount,
                    CarId = x.CarId,
                    CustomerId = x.CustomerId
                })
                .ToArray();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}";
        }

        //Query 14. Cars With Distance
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            const string rootElement = "cars";

            var cars = context
                .Cars
                .Where(c => c.TravelledDistance > 2000000)
                .Select(c => new ExportCarWithDistanceDto
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .ToArray();

            var result = XMLConverter.Serialize(cars, rootElement);

            return result;
        }

        //Query 15. Cars from make BMW
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            const string rootElement = "cars";

            var cars = context
                .Cars
                .Where(c => c.Make.ToLower() == "bmw")
                .Select(c => new ExportCarsFromMakeBmwDto
                {
                    Id = c.Id,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .ToArray();

            var result = XMLConverter.Serialize(cars, rootElement);

            return result;
        }

        //Query 16. Local Suppliers
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            const string rootElement = "suppliers";

            var suppliers = context
                .Suppliers
                .Where(s => !s.IsImporter)
                .Select(s => new ExportLocalSuppliersDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToArray();

            var result = XMLConverter.Serialize(suppliers, rootElement);

            return result;
        }

        //Query 17. Cars with Their List of Parts
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            const string rootElement = "cars";

            var cars = context
                .Cars
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new ExportCarWithTheirListOfPartsDto
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    Parts = c.PartCars.Select(pc => new ExportCarPartDto
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToArray()
                })
                .ToArray();

            var result = XMLConverter.Serialize(cars, rootElement);

            return result;
        }

        //Query 18. Total Sales by Customer
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            const string rootElement = "customers";

            var customers = context
                .Customers
                .Where(c => c.Sales.Count > 0)
                .Select(c => new ExportCustomerDto
                {
                    Name = c.Name,
                    BougthCars = c.Sales.Count,
                    SpentMoney = c.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price))
                })
                .OrderByDescending(c => c.SpentMoney)
                .ToArray();

            var result = XMLConverter.Serialize(customers, rootElement);

            return result;
        }

        //Query 19. Sales with Applied Discount
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            const string rootElement = "sales";

            var sales = context
                .Sales
                .Select(s => new ExportSaleDto
                {
                    Car = new ExportCarDto
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    Name = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = s.Car.PartCars.Sum(pc => pc.Part.Price) - (s.Car.PartCars.Sum(pc => pc.Part.Price) * s.Discount / 100)
                })
                .ToArray();

            var result = XMLConverter.Serialize(sales, rootElement);

            return result;
        }
    }
}