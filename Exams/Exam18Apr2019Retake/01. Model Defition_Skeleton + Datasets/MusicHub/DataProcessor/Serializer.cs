namespace MusicHub.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using MusicHub.DataProcessor.ExportDtos;
    using MusicHub.XMLHelper;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context
                .Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs
                        .Select(s => new 
                        {
                            SongName = s.Name,
                            Price = s.Price.ToString("F2"),
                            Writer = s.Writer.Name,
                        })
                        .OrderByDescending(s => s.SongName)
                        .ThenBy(s => s.Writer)
                        .ToArray(),
                    AlbumPrice = a.Price.ToString("F2")
                })
                .OrderByDescending(p => decimal.Parse(p.AlbumPrice))
                .ToArray();

            var json = JsonConvert.SerializeObject(albums, Formatting.Indented);

            return json;
        }
    

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            const string rootElement = "Songs";

            var songs = context
                .Songs
                .ToArray()
                .Where(s => s.Duration.TotalSeconds > duration)
                    .Select(s => new SongDto
                    {
                        SongName = s.Name,
                        Writer = s.Writer.Name,
                        Performer = s.SongPerformers.Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName).FirstOrDefault(),
                        AlbumProducer = s.Album.Producer.Name,
                        Duration = s.Duration.ToString("c")
                    })
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.Writer)
                .ThenBy(s => s.Performer)
                .ToArray();

            var result = XMLConverter.Serialize(songs, rootElement);

            return result;
        }
    }
}