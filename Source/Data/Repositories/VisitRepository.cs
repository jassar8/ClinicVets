using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal sealed class VisitRepository : SqliteRepositoryBase
    {
        public List<Visit> LoadAll(IReadOnlyList<Medication> medications)
        {
            var visits = new List<Visit>();

            using var connection = OpenConnection();

            string query = @"
                SELECT Id, AnimalChipNumber, VisitDate, Reason, Symptoms, Diagnosis,
                       VeterinarianName, BaseCost, MedicationName, MedicationQuantity, TotalCost,
                       ArrivalStatus, ArrivalNote
                FROM Visits
                ORDER BY Id;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                visits.Add(new Visit
                {
                    Id = reader.GetInt32(0),
                    AnimalChipNumber = reader.GetString(1),
                    VisitDate = DateTime.Parse(reader.GetString(2)),
                    Reason = reader.GetString(3),
                    Symptoms = reader.GetString(4),
                    Diagnosis = reader.GetString(5),
                    VeterinarianName = reader.GetString(6),
                    BaseCost = reader.GetDouble(7),
                    MedicationName = reader.GetString(8),
                    MedicationQuantity = reader.GetInt32(9),
                    TotalCost = reader.GetDouble(10),
                    ArrivalStatus = reader.GetString(11),
                    ArrivalNote = reader.GetString(12)
                });
            }

            var treatmentLines = LoadVisitTreatmentLines(connection);
            foreach (var visit in visits)
            {
                visit.TreatmentLines = treatmentLines
                    .Where(line => line.VisitId == visit.Id)
                    .ToList();

                if (visit.TreatmentLines.Count == 0 &&
                    (!string.IsNullOrWhiteSpace(visit.MedicationName) ||
                     !string.IsNullOrWhiteSpace(visit.Diagnosis)))
                {
                    var medication = medications.FirstOrDefault(m => m.Name == visit.MedicationName);
                    double lineCost = medication != null
                        ? medication.UnitPrice * visit.MedicationQuantity
                        : 0;

                    visit.TreatmentLines.Add(new VisitTreatmentLine
                    {
                        VisitId = visit.Id,
                        Description = string.IsNullOrWhiteSpace(visit.Diagnosis)
                            ? visit.Reason
                            : visit.Diagnosis,
                        MedicationName = visit.MedicationName,
                        MedicationQuantity = visit.MedicationQuantity,
                        LineCost = lineCost
                    });
                }

                visit.SyncLegacyMedicationFields();
            }

            return visits;
        }

        public void SaveAll(IReadOnlyList<Visit> visits)
        {
            using var connection = OpenConnection();

            using var deleteLinesCommand = new SqliteCommand("DELETE FROM VisitTreatmentLines", connection);
            deleteLinesCommand.ExecuteNonQuery();

            using var deleteCommand = new SqliteCommand("DELETE FROM Visits", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var visit in visits)
            {
                visit.SyncLegacyMedicationFields();

                string insertQuery = @"
                    INSERT INTO Visits
                    (Id, AnimalChipNumber, VisitDate, Reason, Symptoms, Diagnosis, VeterinarianName,
                     BaseCost, MedicationName, MedicationQuantity, TotalCost, ArrivalStatus, ArrivalNote)
                    VALUES
                    (@Id, @AnimalChipNumber, @VisitDate, @Reason, @Symptoms, @Diagnosis, @VeterinarianName,
                     @BaseCost, @MedicationName, @MedicationQuantity, @TotalCost, @ArrivalStatus, @ArrivalNote);";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Id", visit.Id == 0 ? (object)DBNull.Value : visit.Id);
                command.Parameters.AddWithValue("@AnimalChipNumber", visit.AnimalChipNumber);
                command.Parameters.AddWithValue("@VisitDate", visit.VisitDate.ToString("yyyy-MM-dd HH:mm"));
                command.Parameters.AddWithValue("@Reason", visit.Reason);
                command.Parameters.AddWithValue("@Symptoms", visit.Symptoms);
                command.Parameters.AddWithValue("@Diagnosis", visit.Diagnosis);
                command.Parameters.AddWithValue("@VeterinarianName", visit.VeterinarianName);
                command.Parameters.AddWithValue("@BaseCost", visit.BaseCost);
                command.Parameters.AddWithValue("@MedicationName", visit.MedicationName);
                command.Parameters.AddWithValue("@MedicationQuantity", visit.MedicationQuantity);
                command.Parameters.AddWithValue("@TotalCost", visit.TotalCost);
                command.Parameters.AddWithValue("@ArrivalStatus", visit.ArrivalStatus);
                command.Parameters.AddWithValue("@ArrivalNote", visit.ArrivalNote);

                command.ExecuteNonQuery();

                if (visit.Id == 0)
                {
                    using var idCommand = new SqliteCommand("SELECT last_insert_rowid();", connection);
                    visit.Id = Convert.ToInt32(idCommand.ExecuteScalar());
                }

                foreach (var line in visit.TreatmentLines)
                {
                    line.VisitId = visit.Id;

                    string insertLineQuery = @"
                        INSERT INTO VisitTreatmentLines
                        (Id, VisitId, Description, MedicationName, MedicationQuantity, LineCost)
                        VALUES
                        (@Id, @VisitId, @Description, @MedicationName, @MedicationQuantity, @LineCost);";

                    using var lineCommand = new SqliteCommand(insertLineQuery, connection);
                    lineCommand.Parameters.AddWithValue("@Id", line.Id == 0 ? (object)DBNull.Value : line.Id);
                    lineCommand.Parameters.AddWithValue("@VisitId", line.VisitId);
                    lineCommand.Parameters.AddWithValue("@Description", line.Description);
                    lineCommand.Parameters.AddWithValue("@MedicationName", line.MedicationName);
                    lineCommand.Parameters.AddWithValue("@MedicationQuantity", line.MedicationQuantity);
                    lineCommand.Parameters.AddWithValue("@LineCost", line.LineCost);
                    lineCommand.ExecuteNonQuery();

                    if (line.Id == 0)
                    {
                        using var lineIdCommand = new SqliteCommand("SELECT last_insert_rowid();", connection);
                        line.Id = Convert.ToInt32(lineIdCommand.ExecuteScalar());
                    }
                }
            }
        }

        private static List<VisitTreatmentLine> LoadVisitTreatmentLines(SqliteConnection connection)
        {
            var lines = new List<VisitTreatmentLine>();

            string query = @"
                SELECT Id, VisitId, Description, MedicationName, MedicationQuantity, LineCost
                FROM VisitTreatmentLines;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lines.Add(new VisitTreatmentLine
                {
                    Id = reader.GetInt32(0),
                    VisitId = reader.GetInt32(1),
                    Description = reader.GetString(2),
                    MedicationName = reader.GetString(3),
                    MedicationQuantity = reader.GetInt32(4),
                    LineCost = reader.GetDouble(5)
                });
            }

            return lines;
        }
    }
}
