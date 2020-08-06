namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Castle.Core.Internal;
    using Data;
    using MusicHub.Data.Models;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using MusicHub.XMLHelper;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter 
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone 
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong 
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";
        public static bool IsDateValid(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return isCurrentDateValid;
        }

        public static bool IsTimeSpanValid(string date, string format)
        {
            TimeSpan currentTimeSpan;
            bool isCurrentTimeSpanValid = TimeSpan.TryParseExact(date, format, CultureInfo.InvariantCulture, TimeSpanStyles.None, out currentTimeSpan);

            return isCurrentTimeSpanValid;
        }

        public static TimeSpan TimeSpanFormated(string date, string format)
        {
            TimeSpan currentTimeSpan;
            bool isCurrentTimeSpanValid = TimeSpan.TryParseExact(date, format, CultureInfo.InvariantCulture, TimeSpanStyles.None, out currentTimeSpan);

            return currentTimeSpan;
        }

        public static DateTime DateTimeFormated(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return currentDate;
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var dtoResult = JsonConvert.DeserializeObject<WriterImportDto[]>(jsonString);

            List<Writer> writers = new List<Writer>();

            foreach (var dto in dtoResult)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Writer writer = new Writer
                {
                    Name = dto.Name,
                    Pseudonym = dto.Pseudonym,
                };

                writers.Add(writer);

                sb.AppendLine(String.Format(SuccessfullyImportedWriter, writer.Name));
            }

            context.Writers.AddRange(writers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var producerImportDtos = JsonConvert.DeserializeObject<ProducerImportDto[]>(jsonString);

            List<Producer> producers = new List<Producer>();

            foreach (var dto in producerImportDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Producer producer = new Producer
                {
                    Name = dto.Name,
                    Pseudonym = dto.Pseudonym,
                    PhoneNumber = dto.PhoneNumber,
                };

                bool isValidAlbum = true;

                foreach (var albumDto in dto.Albums)
                {
                    if (!IsValid(albumDto))
                    {
                        isValidAlbum = false;
                        sb.AppendLine(ErrorMessage);
                        break;
                    }

                    //if (!IsDateValid(albumDto.ReleaseDate, "dd/MM/yyyy"))
                    //{
                    //    isValidAlbum = false;
                    //    sb.AppendLine(ErrorMessage);
                    //    break;
                    //}

                    DateTime albumReleaseDate = DateTimeFormated(albumDto.ReleaseDate, "dd/MM/yyyy");

                    producer.Albums.Add(new Album()
                    {
                        Name = dto.Name,
                        ReleaseDate = albumReleaseDate,
                    });
                }

                if (!isValidAlbum)
                {
                    continue;
                }

                producers.Add(producer);

                if (producer.PhoneNumber.IsNullOrEmpty())
                {
                    sb.AppendLine(String.Format(SuccessfullyImportedProducerWithNoPhone, producer.Name, producer.Albums.Count));
                }
                else
                {
                    sb.AppendLine(String.Format(SuccessfullyImportedProducerWithPhone, producer.Name, producer.PhoneNumber, producer.Albums.Count));
                }
            }

            context.Producers.AddRange(producers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Songs";

            var dtoResult = XMLConverter.Deserializer<ImportSongDto>(xmlString, rootElement);

            List<Song> songs = new List<Song>();

            foreach (var dto in dtoResult)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //if (!context.Writers.Any(w => w.Id == dto.WriterId))
                //{
                //    sb.AppendLine(ErrorMessage);
                //    continue;
                //}

                //if (dto.Genre != Genre.Blues.ToString()
                //    && dto.Genre != Genre.Jazz.ToString()
                //    && dto.Genre != Genre.PopMusic.ToString()
                //    && dto.Genre != Genre.Rap.ToString()
                //    && dto.Genre != Genre.Rock.ToString()
                //    )
                //{
                //    sb.AppendLine(ErrorMessage);
                //    continue;
                //}

                //if (dto.AlbumId.HasValue)
                //{
                //    if (!context.Albums.Any(a => a.Id == dto.AlbumId.Value))
                //    {
                //        sb.AppendLine(ErrorMessage);
                //        continue;
                //    }
                //}

                var genre = Enum.TryParse(dto.Genre, out Genre genreResult);
                var album = context.Albums.Find(dto.AlbumId);
                var writer = context.Writers.Find(dto.WriterId);
                var songTitle = songs.Any(s => s.Name == dto.Name);

                if (!genre || album == null || writer == null || songTitle)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Song song = new Song
                {
                    Name = dto.Name,
                    Duration = TimeSpanFormated(dto.Duration, "c"),
                    CreatedOn = DateTimeFormated(dto.CreatedOn, "dd/MM/yyyy"),
                    Genre = (Genre)Enum.Parse(typeof(Genre), dto.Genre),
                    AlbumId = dto.AlbumId,
                    WriterId = dto.WriterId,
                    Price = dto.Price,
                };

                songs.Add(song);
                sb.AppendLine(String.Format(SuccessfullyImportedSong, song.Name, song.Genre, song.Duration));
            }

            context.Songs.AddRange(songs);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Performers";

            var dtoResult = XMLConverter.Deserializer<PerformerImportDto>(xmlString, rootElement);

            List<Performer> performers = new List<Performer>();

            foreach (var dto in dtoResult)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var validSongsCount = context.Songs.Count(s => dto.PerformerSongs.Any(i => i.SongId == s.Id));

                if (validSongsCount != dto.PerformerSongs.Length)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Performer performer = new Performer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Age = dto.Age,
                    NetWorth = dto.NetWorth,
                };

                //bool isSongIdValid = true;

                foreach (var songDto in dto.PerformerSongs)
                {
                    //if (!context.Songs.Any(s => s.Id == songDto.SongId))
                    //{
                    //    isSongIdValid = false;
                    //    sb.AppendLine(ErrorMessage);
                    //    break;
                    //}

                    SongPerformer songPerformer = new SongPerformer
                    {
                        SongId = songDto.SongId,
                    };

                    performer.PerformerSongs.Add(songPerformer);
                }

                //if (!isSongIdValid)
                //{
                //    continue;
                //}

                performers.Add(performer);
                sb.AppendLine(String.Format(SuccessfullyImportedPerformer, performer.FirstName, performer.PerformerSongs.Count));
            }

            context.Performers.AddRange(performers);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }
    }
}