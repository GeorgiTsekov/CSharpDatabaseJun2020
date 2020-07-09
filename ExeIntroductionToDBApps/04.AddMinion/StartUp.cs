using System;
using System.Data.SqlClient;

namespace _04.AddMinion
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

            string inputMinion = Console.ReadLine();
            string inputVillain = Console.ReadLine();

            string[] splitedInputMinion = inputMinion.Split(' ');
            string[] splitedInputVillain = inputVillain.Split(' ');

            string minionName = splitedInputMinion[1];
            int minionAge = int.Parse(splitedInputMinion[2]);
            string minionTown = splitedInputMinion[3];
            string villainName = splitedInputVillain[1];

            using (connection)
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"SELECT Id FROM Towns WHERE Name = @townName";
                command.Parameters.AddWithValue("@townName", minionTown);

                object value = command.ExecuteScalar();

                if (value == null)
                {
                    command.CommandText = @"INSERT INTO Towns (Name, CountryCode) VALUES (@townName, 5)";
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Town {minionTown} was added to the database.");
                }

                command.CommandText = @"SELECT Id FROM Towns WHERE Name = @townName";
                value = command.ExecuteScalar();
                int townId = (int)value;

                command.CommandText = @"SELECT Id FROM Villains WHERE Name = @villainName";
                command.Parameters.AddWithValue("@villainName", villainName);

                value = command.ExecuteScalar();

                if (value == null)
                {
                    command.CommandText = @"INSERT INTO Villains (Name, EvilnessFactorId)  VALUES (@villainName, 4)";
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Villain {villainName} was added to the database");
                }

                command.CommandText = @"SELECT Id FROM Villains WHERE Name = @villainName";
                value = command.ExecuteScalar();
                int villainId = (int)value;

                command.CommandText = @"SELECT Id FROM Minions WHERE Name = @minionName";
                command.Parameters.AddWithValue("@minionName", minionName);
                command.Parameters.AddWithValue("@age", minionAge);
                command.Parameters.AddWithValue("@townId", townId);
                command.Parameters.AddWithValue("@villainId", villainId);

                value = command.ExecuteScalar();

                if (value == null)
                {
                    command.CommandText = @"INSERT INTO Minions (Name, Age, TownId) VALUES (@minionName, @age, @townId)";
                    command.ExecuteNonQuery();

                    command.CommandText = @"SELECT Id FROM Villains WHERE Name = @villainName";
                    value = command.ExecuteScalar();
                    int minionId = (int)value;

                    command.Parameters.AddWithValue("@minionId", minionId);

                    command.CommandText = @"INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@villainId, @minionId)";
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Successfully added {minionName} to be minion of {villainName}.");
                }
            }
        }
    }
}
