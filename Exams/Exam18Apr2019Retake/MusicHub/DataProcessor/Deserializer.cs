namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
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

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var writersDto = JsonConvert.DeserializeObject<WriterImportDto[]>(jsonString);

            var writers = new List<Writer>();

            foreach (var dto in writersDto)
            {
                if (IsValid(dto))
                {
                    var writer = new Writer
                    {
                        Name = dto.Name,
                        Pseudonym = dto.Pseudonym,
                    };

                    context.Writers.Add(writer);
                    sb.AppendLine($"Imported {writer.Name}");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.Writers.AddRange(writers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var producersWithAlbumsDto = JsonConvert.DeserializeObject<ProducerWithAlbumsImportDto[]>(jsonString);

            foreach (var dto in producersWithAlbumsDto)
            {
                if (IsValid(dto))
                {
                    var producer = new Producer
                    {
                        Name = dto.Name,
                        Pseudonym = dto.Pseudonym,
                        PhoneNumber = dto.PhoneNumber
                    };

                    var albums = new List<Album>();
                    int countIsValid = 0;

                    foreach (var albumDto in dto.Albums)
                    {
                        if (IsValid(albumDto))
                        {
                            albums.Add(new Album
                            {
                                //ProducerId = producer.Id,
                                Name = albumDto.Name,
                                ReleaseDate = DateTime.ParseExact(albumDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                Producer = producer
                            });
                        }
                        else
                        {
                            countIsValid++;
                            continue;
                        }
                    }

                    if (countIsValid == 0)
                    {
                        context.Producers.Add(producer);

                        context.Albums.AddRange(albums);
                        context.SaveChanges();
                        if (producer.PhoneNumber == null)
                        {
                            sb.AppendLine($"Imported {producer.Name} with no phone number produces {albums.Count} albums");
                        }
                        else
                        {
                            sb.AppendLine($"Imported {producer.Name} with phone: {producer.PhoneNumber} produces {albums.Count} albums");
                        }
                    }
                    else
                    {
                        sb.AppendLine(ErrorMessage);
                    }
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Songs";

            var dtoResult = XMLConverter.Deserializer<SongImportDto>(xmlString, rootElement);

            foreach (var dto in dtoResult)
            {
                if (IsValid(dto) 
                    && context.Albums.Any(s => s.Id == dto.AlbumId)
                    && context.Writers.Any(w => w.Id == dto.WriterId)
                    && (dto.Genre == Genre.Blues.ToString()
                    || dto.Genre == Genre.Jazz.ToString()
                    || dto.Genre == Genre.PopMusic.ToString()
                    || dto.Genre == Genre.Rap.ToString()
                    || dto.Genre == Genre.Rock.ToString())
                    )
                {
                    var song = new Song
                    {
                        Name = dto.Name,
                        Duration = TimeSpan.ParseExact(dto.Duration, "c", CultureInfo.InvariantCulture),
                        CreatedOn = DateTime.ParseExact(dto.CreatedOn, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Genre = (Genre)Enum.Parse(typeof(Genre), dto.Genre),
                        AlbumId = dto.AlbumId,
                        WriterId = dto.WriterId,
                        Price = dto.Price,
                    };

                    context.Songs.Add(song);

                    sb.AppendLine($"Imported {song.Name} ({song.Genre} genre) with duration {song.Duration}");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Performers";

            var dtoResult = XMLConverter.Deserializer<PerformerWithSongsImportDto>(xmlString, rootElement);

            foreach (var dto in dtoResult)
            {
                if (IsValid(dto))
                {
                    var performer = new Performer
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Age = dto.Age,
                        NetWorth = dto.NetWorth
                    };

                    var songPerformers = new List<SongPerformer>();

                    int countOfValidSongs = 0;

                    foreach (var dtoSongPerformer in dto.PerformerSongs)
                    {
                        if (context.Songs.Any(sp => sp.Id == dtoSongPerformer.SongId))
                        {
                            var song = new SongPerformer
                            {
                                SongId = dtoSongPerformer.SongId,
                                //LOOK here!!!
                                Performer = performer  
                            };

                            songPerformers.Add(song);
                        }
                        else
                        {
                            countOfValidSongs++;
                            continue;
                        }
                    }

                    if (countOfValidSongs == 0)
                    {
                        context.Performers.Add(performer);

                        context.SongsPerformers.AddRange(songPerformers);
                        context.SaveChanges();

                        sb.AppendLine($"Imported {performer.FirstName} ({songPerformers.Count} songs)");
                    }
                    else
                    {
                        sb.AppendLine(ErrorMessage);
                    }
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var res = Validator.TryValidateObject(obj, validator, validationRes, true);

            return res;
        }
    }
}