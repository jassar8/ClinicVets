using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal sealed class AnimalRepository : SqliteRepositoryBase
    {
        public List<Animal> LoadAll()
        {
            var animals = new List<Animal>();

            using var connection = OpenConnection();

            string query = @"
                SELECT Id, Name, Species, ChipNumber, Weight, BirthDate, OwnerIdNumber, LastVaccinationDate
                FROM Animals
                ORDER BY Id;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                animals.Add(new Animal
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Species = reader.GetString(2),
                    ChipNumber = reader.GetString(3),
                    Weight = reader.GetDouble(4),
                    BirthDate = DateTime.Parse(reader.GetString(5)),
                    OwnerIdNumber = reader.GetString(6),
                    LastVaccinationDate = DateTime.Parse(reader.GetString(7))
                });
            }

            return animals;
        }

        public void SaveAll(IReadOnlyList<Animal> animals)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            foreach (var animal in animals)
            {
                string insertQuery = @"
                    INSERT INTO Animals
                    (Name, Species, ChipNumber, Weight, BirthDate, OwnerIdNumber, LastVaccinationDate)
                    VALUES
                    (@Name, @Species, @ChipNumber, @Weight, @BirthDate, @OwnerIdNumber, @LastVaccinationDate)
                    ON CONFLICT(ChipNumber) DO UPDATE SET
                        Name = excluded.Name,
                        Species = excluded.Species,
                        Weight = excluded.Weight,
                        BirthDate = excluded.BirthDate,
                        OwnerIdNumber = excluded.OwnerIdNumber,
                        LastVaccinationDate = excluded.LastVaccinationDate;";

                using var command = new SqliteCommand(insertQuery, connection);
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Species", animal.Species);
                command.Parameters.AddWithValue("@ChipNumber", animal.ChipNumber);
                command.Parameters.AddWithValue("@Weight", animal.Weight);
                command.Parameters.AddWithValue("@BirthDate", animal.BirthDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@OwnerIdNumber", animal.OwnerIdNumber);
                command.Parameters.AddWithValue("@LastVaccinationDate", animal.LastVaccinationDate.ToString("yyyy-MM-dd"));

                command.ExecuteNonQuery();
            }

            var savedChipNumbers = new HashSet<string>();
            foreach (var animal in animals)
                savedChipNumbers.Add(animal.ChipNumber);

            var databaseChipNumbers = new List<string>();
            using (var selectCommand = new SqliteCommand("SELECT ChipNumber FROM Animals;", connection))
            {
                selectCommand.Transaction = transaction;
                using var reader = selectCommand.ExecuteReader();

                while (reader.Read())
                    databaseChipNumbers.Add(reader.GetString(0));
            }

            foreach (string chipNumber in databaseChipNumbers)
            {
                if (savedChipNumbers.Contains(chipNumber))
                    continue;

                using var deleteCommand = new SqliteCommand(
                    "DELETE FROM Animals WHERE ChipNumber = @ChipNumber;",
                    connection);
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@ChipNumber", chipNumber);
                deleteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
