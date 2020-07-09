using System;
using System.Data.SqlClient;

namespace _03.MinionNames
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

            int id = int.Parse(Console.ReadLine());

            using (connection)
            {
                try
                {
                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.Connection = connection;
                    cmd2.CommandText = @"SELECT Name FROM Villains WHERE Id = @Id";
                    cmd2.Parameters.AddWithValue("@Id", id);

                    object value = cmd2.ExecuteScalar();

                    if (value == null)
                    {
                        throw new ArgumentException(nameof(id), $"No villain with ID {id} exists in the database.");
                    }

                    string villianName = (string)value;

                    cmd2.CommandText = @"SELECT ROW_NUMBER() OVER (ORDER BY m.Name) as RowNum,
                                         m.Name, 
                                         m.Age
                                    FROM MinionsVillains AS mv
                                    JOIN Minions As m ON mv.MinionId = m.Id
                                   WHERE mv.VillainId = @Id
                                ORDER BY m.Name";

                    Console.WriteLine($"Villain: {villianName}");

                    SqlDataReader reader2 = cmd2.ExecuteReader();

                    if (reader2 == null)
                    {
                        throw new ArgumentException(nameof(id), "(no minions)");
                    }

                    using (reader2)
                    {
                        while (reader2.Read())
                        {
                            Console.WriteLine($"{reader2["RowNum"]}. {reader2["Name"]} {reader2["Age"]}");
                        }
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
