namespace VaporStore.DataProcessor
{
	using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;
    using VaporStore.XMLHelper;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
            var genres = context
                .Genres
                .ToArray()
                .Where(g => genreNames.Contains(g.Name))
                .Select(g => new
                {
                    Id = g.Id,
                    Genre = g.Name,
                    Games = g.Games
                        .Where(ga => ga.Purchases.Any())
                        .Select(ga => new
                        {
                            Id = ga.Id,
                            Title = ga.Name,
                            Developer = ga.Developer.Name,
                            Tags = String.Join(", ", ga.GameTags.Select(gt => gt.Tag.Name).ToArray()),
                            Players = ga.Purchases.Count,
                        })
                        .OrderByDescending(ga => ga.Players)
                        .ThenBy(ga => ga.Id)
                        .ToArray(),
                    TotalPlayers = g.Games.Sum(ga => ga.Purchases.Count)
                })
                .OrderByDescending(g => g.TotalPlayers)
                .ThenBy(g => g.Id)
                .ToArray();

            var json = JsonConvert.SerializeObject(genres, Formatting.Indented);

            return json;
        }

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
            const string rootElement = "Users";

            PurchaseType purchaseTypeEnum = Enum.Parse<PurchaseType>(storeType);

            var users = context
                .Users
                .ToArray()
                .Where(u => u.Cards.Any(c => c.Purchases.Any()))
                .Select(u => new UserDto
                {
                    username = u.Username,
                    Purchases = u.Cards
                        .ToArray()
                        .SelectMany(c => c.Purchases)
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum)
                        .OrderBy(p => p.Date)
                        .Select(p => new PurchaseDto
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new GameDto
                            {
                                title = p.Game.Name,
                                Genre = p.Game.Genre.Name,
                                Price = p.Game.Price,
                            }
                        })
                        .ToArray(),
                    TotalSpent = u.Cards.Sum(c => c.Purchases.Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum).Sum(p => p.Game.Price))
                })
                .Where(u => u.Purchases.Length > 0)
                .OrderByDescending(u => u.TotalSpent)
                .ThenBy(u => u.username)
                .ToArray();

            var result = XMLConverter.Serialize(users, rootElement);

            return result;
        }
	}
}