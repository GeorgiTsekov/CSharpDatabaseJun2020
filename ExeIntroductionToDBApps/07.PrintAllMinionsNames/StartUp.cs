using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace _07.PrintAllMinionsNames
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

            using (connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"SELECT Name FROM Minions";

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader == null)
                    {
                        throw new ArgumentException($"No Minions.");
                    }

                    using (reader)
                    {
                        List<string> towns = new List<string>();
                        Queue<string> townsQueue = new Queue<string>();
                        Stack<string> townsStack = new Stack<string>();

                        while (reader.Read())
                        {
                            towns.Add(reader["Name"].ToString());
                            townsQueue.Enqueue(reader["Name"].ToString());
                            townsStack.Push(reader["Name"].ToString());
                        }

                        int stackCount = townsStack.Count / 2;

                        if (townsStack.Count % 2 == 0)
                        {
                            while (townsStack.Count > stackCount)
                            {
                                Console.WriteLine(townsQueue.Dequeue());
                                Console.WriteLine(townsStack.Pop());
                            }
                        }
                        else
                        {
                            while (townsStack.Count > stackCount)
                            {
                                if (townsStack.Count - 1 > stackCount)
                                {
                                    Console.WriteLine(townsQueue.Dequeue());
                                    Console.WriteLine(townsStack.Pop());
                                }
                                else
                                {
                                    Console.WriteLine(townsQueue.Dequeue());
                                    townsStack.Pop();
                                }
                            }
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
