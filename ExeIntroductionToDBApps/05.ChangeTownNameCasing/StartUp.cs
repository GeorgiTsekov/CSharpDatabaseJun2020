using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace _05.ChangeTownNameCasing
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

            string countryName = Console.ReadLine();

            using (connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"SELECT COUNT(*) 
                                           FROM Towns as t
                                           JOIN Countries AS c ON c.Id = t.CountryCode
                                          WHERE c.Name = @countryName";
                    command.Parameters.AddWithValue("@countryName", countryName);

                    object value = command.ExecuteScalar();
                    int countOfTownsAffected = (int)value;

                    if (countOfTownsAffected == 0)
                    {
                        throw new ArgumentException($"No town names were affected.");
                    }

                    command.CommandText = @"UPDATE Towns
                                           SET Name = UPPER(Name)
                                         WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = @countryName)";
                    command.ExecuteNonQuery();

                    command.CommandText = @"SELECT t.Name 
                                           FROM Towns as t
                                           JOIN Countries AS c ON c.Id = t.CountryCode
                                          WHERE c.Name = @countryName";
                    
                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine($"{countOfTownsAffected} town names were affected.");
                   
                    using (reader)
                    {
                        List<string> towns = new List<string>();

                        while (reader.Read())
                        {
                            towns.Add(reader["Name"].ToString());
                        }

                        Console.WriteLine($"[{string.Join(", ", towns)}]");
                    }
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine(ae.Message);
                }
            }
        }
    }
}
