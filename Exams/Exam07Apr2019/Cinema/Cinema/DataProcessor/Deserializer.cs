namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Cinema.Data.Models;
    using Cinema.DataProcessor.ImportDto;
    using Cinema.XMLHelper;
    using Data;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie 
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat 
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection 
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket 
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var moviesDto = JsonConvert.DeserializeObject<MovieImportDto[]>(jsonString);

            var movies = new List<Movie>();

            foreach (var dto in moviesDto)
            {
                if (IsValid(dto))
                {
                    var movie = new Movie
                    {
                        Title = dto.Title,
                        Genre = dto.Genre,
                        Duration = dto.Duration,
                        Rating = dto.Rating,
                        Director = dto.Director
                    };

                    context.Movies.Add(movie);
                    sb.AppendLine($"Successfully imported {movie.Title} with genre {movie.Genre} and rating {movie.Rating:f2}!");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var res = Validator.TryValidateObject(obj, validator, validationRes, true);

            return res;
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var hallsAndSeatsDto = JsonConvert.DeserializeObject<HallsAndSeatsImportDto[]>(jsonString);

            foreach (var dto in hallsAndSeatsDto)
            {
                if (IsValid(dto))
                {
                    var hall = new Hall
                    {
                        Name = dto.Name,
                        Is4Dx = dto.Is4Dx,
                        Is3D = dto.Is3D
                    };

                    context.Halls.Add(hall);
                    var seats = new List<Seat>();

                    for (int i = 0; i < dto.Seats; i++)
                    {
                        seats.Add(new Seat { HallId = hall.Id });
                    }

                    context.Seats.AddRange(seats);
                    context.SaveChanges();

                    var projectionType = "Normal";

                    if (hall.Is3D && hall.Is4Dx)
                    {
                        projectionType = "4Dx/3D";
                    }
                    else if (hall.Is3D)
                    {
                        projectionType = "3D";
                    }
                    else if (hall.Is4Dx)
                    {
                        projectionType = "4Dx";
                    }

                    sb.AppendLine($"Successfully imported {hall.Name}({projectionType}) with {seats.Count} seats!");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Projections";

            var dtoResult = XMLConverter.Deserializer<ProjectionImportDto>(xmlString, rootElement);

            foreach (var dto in dtoResult)
            {
                if (IsValid(dto) && context.Movies.Any(c => c.Id == dto.MovieId) && context.Halls.Any(p => p.Id == dto.HallId))
                {
                    var projection = new Projection
                    {
                        MovieId = dto.MovieId,
                        HallId = dto.HallId,
                        DateTime = DateTime.ParseExact(dto.DateTime,"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    };

                    context.Projections.Add(projection);

                    string dateTimeRes = projection.DateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    sb.AppendLine($"Successfully imported projection {projection.Movie.Title} on {dateTimeRes}!");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Customers";

            var dtoResult = XMLConverter.Deserializer<CustomerWithTicketsImportDto>(xmlString, rootElement);

            foreach (var dto in dtoResult)
            {
                if (IsValid(dto))
                {
                    var customer = new Customer
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Age = dto.Age,
                        Balance = dto.Balance
                    };

                    context.Customers.Add(customer);

                    var tickets = new List<Ticket>();

                    foreach (var dtoTicket in dto.Tickets)
                    {
                        if (IsValid(dtoTicket))
                        {
                            var ticket = new Ticket
                            {
                                ProjectionId = dtoTicket.ProjectionId,
                                CustomerId = customer.Id,
                                Price = dtoTicket.Price
                            };

                            tickets.Add(ticket);
                        }
                    }

                    context.Tickets.AddRange(tickets);
                    context.SaveChanges();

                    sb.AppendLine($"Successfully imported customer {customer.FirstName} {customer.LastName} with bought tickets: {customer.Tickets.Count()}!");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
    }
}