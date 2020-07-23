using AutoMapper;
using AutoMapper.QueryableExtensions;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using ProductShop.XMLHelper;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            //Mapper.Initialize(cfg => cfg.AddProfile<ProductShopProfile>());

            using ProductShopContext context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var inputXml = File.ReadAllText("./../../../Datasets/users.xml");
            //var inputXml = File.ReadAllText("./../../../Datasets/products.xml");
            //var inputXml = File.ReadAllText("./../../../Datasets/categories.xml");
            //var inputXml = File.ReadAllText("./../../../Datasets/categories-products.xml");

            //var result = ImportCategoryProducts(context, inputXml);
            var result = GetUsersWithProducts(context);

            Console.WriteLine(result);
            
        }

        //Query 1. Import Users
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Users";

            var usersResult = XMLConverter.Deserializer<ImportUserDto>(inputXml, rootElement);

            var users = usersResult
                .Select(u => new User
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age
                })
                .ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //Query 2. Import Products
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Products";

            var productResult = XMLConverter.Deserializer<ImportProductDto>(inputXml, rootElement);

            var products = productResult
                .Select(u => new Product
                {
                    Name = u.Name,
                    Price = u.Price,
                    SellerId = u.SellerId,
                    BuyerId = u.BuyerId
                })
                .ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Query 3. Import Categories
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Categories";

            var dtoResult = XMLConverter.Deserializer<ImportCategoryDto>(inputXml, rootElement);

            var categories = dtoResult
                .Where(x => x.Name != null)
                .Select(x => new Category
                {
                    Name = x.Name
                })
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        //Query 4. Import Categories and Products
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "CategoryProducts";

            var dtoResult = XMLConverter.Deserializer<ImportCategoryProductDto>(inputXml, rootElement);

            var categoryProducts = dtoResult
                .Where(cp => context.Categories.Any(c => c.Id == cp.CategoryId) && context.Products.Any(p => p.Id == cp.ProductId))
                .Select(x => new CategoryProduct
                {
                    CategoryId = x.CategoryId,
                    ProductId = x.ProductId
                })
                .ToArray();

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        //Query 5. Products In Range
        public static string GetProductsInRange(ProductShopContext context)
        {
            const string rootElement = "Products";

            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new ExportProductInRangeDto
                {
                   Name = p.Name,
                   Price = p.Price,
                   BuyerName = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToArray();

            var result = XMLConverter.Serialize(products, rootElement);

            return result;
        }

        //Query 6. Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            const string rootElement = "Users";

            var users = context
                .Users
                .Where(u => u.ProductsSold.Any())
                .Select(u => new ExportUserWithProductsDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(sp => new ExportSoldProductDto 
                    {
                        Name = sp.Name,
                        Price = sp.Price
                    })
                    .ToArray()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .ToArray();

            var result = XMLConverter.Serialize(users, rootElement);

            return result;
        }

        //Query 7. Categories By Products Count
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            const string rootElement = "Categories";

            var categories = context
                .Categories
                .Select(c => new ExportCategoryDto
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            var result = XMLConverter.Serialize(categories, rootElement);

            return result;
        }

        //Query 8. Users and Products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            const string rootElement = "Users";

            var users = context
                .Users
                .ToArray()
                .Where(u => u.ProductsSold.Any())
                .Select(x => new ExportUserWithAgeFLNameAndProductsDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Age = x.Age,
                    SoldProductsDto = new SoldProductsDto
                    {
                        Count = x.ProductsSold.Count,
                        Products = x.ProductsSold.Select(ps => new ExportSoldProductDto
                        {
                            Name = ps.Name,
                            Price = ps.Price
                        })
                        .OrderByDescending(ps => ps.Price)
                        .ToArray()
                    }
                })
                .OrderByDescending(u => u.SoldProductsDto.Count)
                .Take(10)
                .ToArray();

            var customExport = new ExportUsersCountAndUsersDto
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = users
            };

            var result = XMLConverter.Serialize(customExport, rootElement);

            return result;
        }
    }
}