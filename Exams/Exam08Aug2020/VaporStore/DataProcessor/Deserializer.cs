namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;
    using VaporStore.XMLHelper;

    public static class Deserializer
	{
        public const string ErrorMessage = "Invalid Data";


        public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
            var sb = new StringBuilder();

            var resultDtos = JsonConvert.DeserializeObject<GameImportDto[]>(jsonString);

            List<Game> games = new List<Game>();

            foreach (var dto in resultDtos)
            {
                if (!IsValid(dto) || dto.Tags.Length == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsDateValid(dto.ReleaseDate, "yyyy-MM-dd"))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Game game = new Game
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    ReleaseDate = DateTimeFormated(dto.ReleaseDate, "yyyy-MM-dd"),
                };

                Developer developer = context.Developers.FirstOrDefault(d => d.Name == dto.Developer);

                if (developer == null)
                {
                    developer = new Developer
                    {
                        Name = dto.Developer
                    };
                }

                game.Developer = developer;

                Genre genre = context.Genres.FirstOrDefault(g => g.Name == dto.Genre);

                if (genre == null)
                {
                    genre = new Genre
                    {
                        Name = dto.Genre
                    };
                }

                game.Genre = genre;

                bool isValidTag = true;

                foreach (var tagName in dto.Tags)
                {
                    if (!IsValid(tagName))
                    {
                        isValidTag = false;
                        sb.AppendLine(ErrorMessage);
                        break;
                    }

                    Tag tag = context.Tags.FirstOrDefault(t => t.Name == tagName);

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName
                        };
                    }

                    game.GameTags.Add(new GameTag
                    {
                        Game = game,
                        Tag = tag,
                    });
                }

                if (!isValidTag)
                {
                    continue;
                }

                context.Games.Add(game);
                context.SaveChanges();
                sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
            var sb = new StringBuilder();

            var resultDtos = JsonConvert.DeserializeObject<UserImportDto[]>(jsonString);

            List<User> users = new List<User>();

            foreach (var dto in resultDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                User user = new User
                {
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Email = dto.Email,
                    Age = dto.Age,
                };

                bool isValidTag = true;

                foreach (var cardDto in dto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        isValidTag = false;
                        sb.AppendLine(ErrorMessage);
                        break;
                    }

                    var type = Enum.TryParse(cardDto.Type, out CardType genreResult);

                    if (!type)
                    {
                        isValidTag = false;
                        sb.AppendLine(ErrorMessage);
                        break;
                    }

                    user.Cards.Add(new Card
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.CVC,
                        Type = (CardType)Enum.Parse(typeof(CardType), cardDto.Type),
                    });
                }

                if (!isValidTag)
                {
                    continue;
                }

                users.Add(user);

                //context.Users.Add(user);
                //context.SaveChanges();
                sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();

            const string rootElement = "Purchases";

            var dtoResult = XMLConverter.Deserializer<PurchaseImportDto>(xmlString, rootElement);

            List<Purchase> purchases = new List<Purchase>();

            foreach (var dto in dtoResult)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsDateValid(dto.Date, "dd/MM/yyyy HH:mm"))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                 Purchase purchase = new Purchase
                 {
                     Type = (PurchaseType)Enum.Parse(typeof(PurchaseType), dto.Type),
                     ProductKey = dto.ProductKey,
                     Date = DateTimeFormated(dto.Date, "dd/MM/yyyy HH:mm"),
                 };

                Game game = context.Games.FirstOrDefault(g => g.Name == dto.title);

                if (game == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                purchase.Game = game;

                Card card = context.Cards.FirstOrDefault(d => d.Number == dto.Card);

                if (card == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                purchase.Card = card;

                purchases.Add(purchase);

                sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
            }

            context.Purchases.AddRange(purchases);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}

        public static bool IsDateValid(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return isCurrentDateValid;
        }

        public static DateTime DateTimeFormated(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return currentDate;
        }
    }
}