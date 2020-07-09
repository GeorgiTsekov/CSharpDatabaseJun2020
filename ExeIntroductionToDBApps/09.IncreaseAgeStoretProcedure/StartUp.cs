using System;
using System.Data.SqlClient;

namespace _09.IncreaseAgeStoretProcedure
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

            int minionId = int.Parse(Console.ReadLine());

            using (connection)
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = $"EXEC usp_GetOlder @id";
                command.Parameters.AddWithValue("@id", minionId);
                command.ExecuteNonQuery();

                command.CommandText = @"SELECT Name, Age FROM Minions WHERE Id = @Id";
                SqlDataReader reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Name"]} - {reader["Age"]} years old");
                    }
                }
            }
        }
    }
}
