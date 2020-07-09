using System;
using System.Data.SqlClient;

namespace _06.RemoveVillain
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

            int villainId = int.Parse(Console.ReadLine());

            using (connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"SELECT Name FROM Villains WHERE Id = @villainId";
                    command.Parameters.AddWithValue("@villainId", villainId);

                    object value = command.ExecuteScalar();

                    if (value == null)
                    {
                        throw new ArgumentException("No such villain was found.");
                    }

                    string villainName = (string)value;

                    command.CommandText = @"SELECT COUNT(*) FROM MinionsVillains 
                                                           WHERE VillainId = @villainId";

                    value = command.ExecuteScalar();

                    int minionsCount = (int)value;

                    command.CommandText = @"DELETE FROM MinionsVillains 
                                                   WHERE VillainId = @villainId";
                    command.ExecuteNonQuery();

                    command.CommandText = @"DELETE FROM Villains
                                                  WHERE Id = @villainId";
                    command.ExecuteNonQuery();

                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine($"{villainName} was deleted.");
                    Console.WriteLine($"{minionsCount} minions were released.");
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine(ae.Message);
                }
            }
        }
    }
}
