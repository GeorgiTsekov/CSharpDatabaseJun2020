namespace Cinema.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Cinema.DataProcessor.ExportDto;
    using Cinema.XMLHelper;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context
                .Movies
                .Where(m => m.Rating >= rating && m.Projections.Any(p => p.Tickets.Count > 0))
                .OrderByDescending(m => m.Rating)
                .ThenByDescending(m => m.Projections.Sum(p => p.Tickets.Sum(t => t.Price)))
                .Select(m => new
                {
                    MovieName = m.Title,
                    Rating = m.Rating.ToString("f2"),
                    TotalIncomes = m.Projections.Sum(p => p.Tickets.Sum(t => t.Price)).ToString("f2"),
                    Customers = m.Projections
                    .SelectMany(p => p.Tickets
                        .Select(t => t.Customer)
                        .Select(c => new
                        {
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            Balance = c.Balance.ToString("f2")
                        })
                    )
                    .OrderByDescending(c => c.Balance)
                    .ThenBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .ToList()
                })
                .Take(10)
                .ToList();

            var json = JsonConvert.SerializeObject(movies, Formatting.Indented);

            return json;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            const string rootElement = "Customers";

            var customers = context
                .Customers
                .Where(c => c.Age >= age)
                .OrderByDescending(c => c.Tickets.Sum(t => t.Price))
                .Select(c => new CustomerSpentDto
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    SpentMoney = c.Tickets.Sum(t => t.Price).ToString("f2"),
                    SpentTime = TimeSpan.FromSeconds(c.Tickets.Sum(t => t.Projection.Movie.Duration.TotalSeconds)).ToString("hh\\:mm\\:ss")
                })
                .Take(10)
                .ToArray();

            var result = XMLConverter.Serialize(customers, rootElement);

            return result;
        }
    }
}