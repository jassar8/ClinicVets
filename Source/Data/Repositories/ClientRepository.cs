using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal sealed class ClientRepository : SqliteRepositoryBase
    {
        public List<Client> LoadAll()
        {
            var clients = new List<Client>();

            using var connection = OpenConnection();

            string query = @"
                SELECT FullName, IdNumber, Phone, Email, Gender
                FROM Clients;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                clients.Add(new Client
                {
                    FullName = reader.GetString(0),
                    IdNumber = reader.GetString(1),
                    Phone = reader.GetString(2),
                    Email = reader.GetString(3),
                    Gender = reader.GetString(4)
                });
            }

            return clients;
        }

        public void SaveAll(IReadOnlyList<Client> clients)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            foreach (var client in clients)
            {
                string insertQuery = @"
                    INSERT INTO Clients
                    (IdNumber, FullName, Phone, Email, Gender)
                    VALUES
                    (@IdNumber, @FullName, @Phone, @Email, @Gender)
                    ON CONFLICT(IdNumber) DO UPDATE SET
                        FullName = excluded.FullName,
                        Phone = excluded.Phone,
                        Email = excluded.Email,
                        Gender = excluded.Gender;";

                using var command = new SqliteCommand(insertQuery, connection);
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@IdNumber", client.IdNumber);
                command.Parameters.AddWithValue("@FullName", client.FullName);
                command.Parameters.AddWithValue("@Phone", client.Phone);
                command.Parameters.AddWithValue("@Email", client.Email);
                command.Parameters.AddWithValue("@Gender", client.Gender);

                command.ExecuteNonQuery();
            }

            var savedIdNumbers = new HashSet<string>();
            foreach (var client in clients)
                savedIdNumbers.Add(client.IdNumber);

            var databaseIdNumbers = new List<string>();
            using (var selectCommand = new SqliteCommand("SELECT IdNumber FROM Clients;", connection))
            {
                selectCommand.Transaction = transaction;
                using var reader = selectCommand.ExecuteReader();

                while (reader.Read())
                    databaseIdNumbers.Add(reader.GetString(0));
            }

            foreach (string idNumber in databaseIdNumbers)
            {
                if (savedIdNumbers.Contains(idNumber))
                    continue;

                using var deleteCommand = new SqliteCommand(
                    "DELETE FROM Clients WHERE IdNumber = @IdNumber;",
                    connection);
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@IdNumber", idNumber);
                deleteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
