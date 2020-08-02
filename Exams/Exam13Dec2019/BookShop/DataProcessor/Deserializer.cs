namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using BookShop.XMLHelper;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";


        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Books";

            var dtoResult = XMLConverter.Deserializer<BookImportDto>(xmlString, rootElement);

            foreach (var dto in dtoResult)
            {
                if (IsValid(dto))
                {
                    var book = new Book
                    {
                        Name = dto.Name,
                        Genre = (Genre)dto.Genre,
                        Price = dto.Price,
                        Pages = dto.Pages,
                        PublishedOn = DateTime.ParseExact(dto.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    };

                    context.Books.Add(book);

                    sb.AppendLine(String.Format(SuccessfullyImportedBook, book.Name, book.Price));
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var authorBooksImportDtos = JsonConvert.DeserializeObject<AuthorBooksImportDto[]>(jsonString);

            foreach (var dto in authorBooksImportDtos)
            {
                if (IsValid(dto) && !context.Authors.Any(a => a.Email == dto.Email))
                {
                    var author = new Author
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Phone = dto.Phone,
                        Email = dto.Email,
                    };

                    var books = new List<AuthorBook>();

                    foreach (var bookFromAuthorDto in dto.Books)
                    {
                        Book book = context.Books
                                .FirstOrDefault(b => b.Id == bookFromAuthorDto.BookId);

                        if (book != null && bookFromAuthorDto.BookId != null)
                        {
                            books.Add(new AuthorBook
                            {
                                Book = book,
                                Author = author
                            });
                        }
                    }

                    if (books.Count > 0)
                    {
                        context.Authors.Add(author);

                        context.AuthorsBooks.AddRange(books);
                        context.SaveChanges();

                        sb.AppendLine(String.Format(SuccessfullyImportedAuthor, author.FirstName + " " + author.LastName, books.Count));
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

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}