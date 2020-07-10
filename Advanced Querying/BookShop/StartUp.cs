using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookShop
{
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            //var command = Console.ReadLine();
            //Console.WriteLine(GetBooksByAgeRestriction(db, command)); // Problem 1

            //Console.WriteLine(GetGoldenBooks(db)); // Problem 2

            //Console.WriteLine(GetBooksByPrice(db)); // Problem 3

            //var year = int.Parse(Console.ReadLine());
            //Console.WriteLine(GetBooksNotReleasedIn(db, year)); // Problem 4

            //var input = Console.ReadLine();
            //Console.WriteLine(GetBooksByCategory(db, input)); // Problem 5

            //var input = Console.ReadLine();
            //Console.WriteLine(GetBooksReleasedBefore(db, input)); // Problem 6

            //var input = Console.ReadLine();
            //Console.WriteLine(GetAuthorNamesEndingIn(db, input)); // Problem 7

            //var input = Console.ReadLine();
            //Console.WriteLine(GetBookTitlesContaining(db, input)); // Problem 8

            //var input = Console.ReadLine();
            //Console.WriteLine(GetBooksByAuthor(db, input)); // Problem 9

            //var input = int.Parse(Console.ReadLine());
            //Console.WriteLine(CountBooks(db, input)); // Problem 10

            //Console.WriteLine(CountCopiesByAuthor(db)); // Problem 11

            //Console.WriteLine(GetTotalProfitByCategory(db)); // Problem 12

            //Console.WriteLine(GetMostRecentBooks(db)); // Problem 13

            //IncreasePrices(db); // Problem 14

            Console.WriteLine(RemoveBooks(db)); // Problem 15
        }

        // 1. Age Restriction
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var result = new StringBuilder();
            var books = context
                  .Books
                  .Where(b => ((Enum)b.AgeRestriction).ToString().ToLower() == command.ToLower())
                  .Select(b => new
                  {
                      b.Title
                  })
                  .OrderBy(b => b.Title)
                  .ToList();
            foreach (var b in books)
            {
                result.AppendLine(b.Title);
            }

            return result.ToString().TrimEnd();
        }

        //2. Golden Books
        public static string GetGoldenBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .Where(b => b.EditionType.ToString() == "Gold" && b.Copies < 5000)
                .Select(b => new
                {
                    b.Title,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var b in books)
            {
                sb.AppendLine(b.Title);
            }

            return sb.ToString().TrimEnd();
        }

        // 3. Books by Price
        public static string GetBooksByPrice(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Price,
                    b.Title
                })
                .OrderByDescending(b => b.Price)
                .ToList();

            foreach (var b in books)
            {
                sb.AppendLine($"{b.Title} - ${b.Price}");
            }

            return sb.ToString().TrimEnd();
        }

        //// 4. Not Released In
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .Select(b => new
                {
                    b.Title,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var b in books)
            {
                sb.AppendLine(b.Title);
            }

            return sb.ToString().TrimEnd();
        }

        //// 5. Book Titles by Category
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categories = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.ToLower())
                .ToArray();

            var books = context
            .Books
            .Where(b => b.BookCategories
                .Any(bc => categories.Contains(bc.Category.Name.ToLower())))
            .Select(b => b.Title)
            .OrderBy(b => b)
            .ToList();

            return String.Join(Environment.NewLine, books);
        }

        //// 6. Released Before Date
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime dateTime = DateTime.Parse(date);

            var books = context
                .Books
                .Where(b => b.ReleaseDate.Value.Date < dateTime)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price,
                    b.ReleaseDate
                })
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var b in books)
            {
                sb.AppendLine($"{b.Title} - {b.EditionType} - ${b.Price}");
            }

            return sb.ToString().TrimEnd();
        }
        //        .Where(b => b.ReleaseDate.Value < DateTime.ParseExact(date, "dd-MM-yyyy", null))
        // var startDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);


        //// 7. Author Search
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var authors = context
                .Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName
                })
                .OrderBy(a => a.FirstName).ThenBy(a => a.LastName)
                .ToList();

            foreach (var a in authors)
            {
                sb.AppendLine($"{a.FirstName} {a.LastName}");
            }

            return sb.ToString().TrimEnd();
        }

        //// 8. Book Search
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            foreach (var b in books)
            {
                sb.AppendLine(b);
            }

            return sb.ToString().TrimEnd();
        }

        //// 9. Book Search by Author
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                    b.Author.FirstName,
                    b.Author.LastName
                })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var b in books)
            {
                sb.AppendLine($"{b.Title} ({b.FirstName} {b.LastName})");
            }

            return sb.ToString().TrimEnd();
        }

        //// 10. Count Books
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            int bookCount = context
                .Books
                .Count(b => b.Title.Length > lengthCheck);

            return bookCount;
        }

        //// 11. Total Book Copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context
                .Authors
                .Select(a => new
                {
                    FirstName = $"{a.FirstName} {a.LastName}",
                    CountOfCopies = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(a => a.CountOfCopies)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var a in authors)
            {
                sb.AppendLine($"{a.FirstName} - {a.CountOfCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        //// 12. Profit by Category
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context
                .Categories
                .Select(c => new
                {
                    c.Name,
                    SumOfCopies = c.CategoryBooks.Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .OrderByDescending(c => c.SumOfCopies).ThenBy(c => c.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var c in categories)
            {
                sb.AppendLine($"{c.Name} ${c.SumOfCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        //// 13. Most Recent Books
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context
                .Categories
                .Select(c => new
                {
                    c.Name,
                    bookCategories = c.CategoryBooks.Select(cb => new
                    {
                        bookYear = cb.Book.ReleaseDate.Value.Year,
                        bookYearForOrder = cb.Book.ReleaseDate.Value,
                        bookName = cb.Book.Title
                    })
                    .OrderByDescending(cb => cb.bookYearForOrder)
                    .Take(3)
                    .ToList()
                })
                .OrderBy(c => c.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var c in categories)
            {
                sb.AppendLine($"--{c.Name}");
                foreach (var bc in c.bookCategories)
                {
                    sb.AppendLine($"{bc.bookName} ({bc.bookYear})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //// 14. Increase Prices - only 50/100 in SoftUniJudge
        public static void IncreasePrices(BookShopContext context)
        {
            context
                .Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .Sum(b => b.Price + 5);

            context.SaveChanges();
        }

        //// 15. Remove Books - only 50/100 in SoftUniJudge
        public static int RemoveBooks(BookShopContext context)
        {
            var booksCount = context
                .Books
                .Where(b => b.Copies < 4200)
                .Count();

            var books = context
                .Books
                .Where(b => b.Copies < 4200)
                .ToList();

            context.RemoveRange(books);

            context.SaveChanges();

            return booksCount;
        }
    }
}
