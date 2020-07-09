using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace _08.IncreaseMinionAge
{
    class StartUp
    {
        private static string connectionString = "Server=DESKTOP-M0LGMHO\\SQLEXPRESS;" +
           "Database=MinionsDB;" +
           "Integrated Security=true";

        private static SqlConnection connection = new SqlConnection(connectionString);

        static void Main(string[] args)
        {
            connection.Open();

            string input = Console.ReadLine();
            List<int> listOfIds = input.Split(' ').Select(int.Parse).ToList();

            using (connection)
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;

                foreach (var id in listOfIds)
                {
                    command.CommandText = $@"UPDATE Minions
                                                   SET Name = LOWER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1
                                                 WHERE Id = {id}";
                    command.ExecuteNonQuery();
                }

                command.CommandText = @"SELECT Name, Age FROM Minions";
                SqlDataReader reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Name"]} {reader["Age"]}");
                    }
                }
            }
        }
    }
}
