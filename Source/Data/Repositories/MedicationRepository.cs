using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal sealed class MedicationRepository : SqliteRepositoryBase
    {
        public List<Medication> LoadAll()
        {
            var medications = new List<Medication>();

            using var connection = OpenConnection();

            string query = @"
                SELECT Id, Name, StockQuantity, UnitPrice, ExpirationDate, Notes
                FROM Medications
                ORDER BY Id;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                medications.Add(new Medication
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    StockQuantity = reader.GetInt32(2),
                    UnitPrice = reader.GetDouble(3),
                    ExpirationDate = DateTime.Parse(reader.GetString(4)),
                    Notes = reader.GetString(5)
                });
            }

            return medications;
        }

        public void SaveAll(IReadOnlyList<Medication> medications)
        {
            using var connection = OpenConnection();

            using var deleteCommand = new SqliteCommand("DELETE FROM Medications", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var medication in medications)
            {
                string insertQuery = @"
                    INSERT INTO Medications
                    (Name, StockQuantity, UnitPrice, ExpirationDate, Notes)
                    VALUES
                    (@Name, @StockQuantity, @UnitPrice, @ExpirationDate, @Notes);";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Name", medication.Name);
                command.Parameters.AddWithValue("@StockQuantity", medication.StockQuantity);
                command.Parameters.AddWithValue("@UnitPrice", medication.UnitPrice);
                command.Parameters.AddWithValue("@ExpirationDate", medication.ExpirationDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Notes", medication.Notes);

                command.ExecuteNonQuery();
            }
        }
    }
}
