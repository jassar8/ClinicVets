using System;
using System.IO;
using ClosedXML.Excel;

namespace ClinicVetsAvalonia.Data
{
    public static class ExcelExportService
    {
        public static string LastExportPath { get; private set; } = "";

        public static void ExportAll()
        {
            try
            {
                string targetPath = ExcelSettings.ActiveExcelPath;
                string? targetFolder = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                using var workbook = new XLWorkbook();
                WriteEmployeesSheet(workbook);
                WriteEmployeeApprovalsSheet(workbook);
                WriteClientsSheet(workbook);
                WriteAnimalsSheet(workbook);
                WriteMedicationsSheet(workbook);
                WriteVisitsSheet(workbook);
                WriteTreatmentsSheet(workbook);

                workbook.SaveAs(targetPath);
                LastExportPath = targetPath;

                TryCopyToProjectPath(targetPath);
            }
            catch
            {
                // Excel is a mirror export; a locked file must not break saves.
            }
        }

        private static void TryCopyToProjectPath(string sourcePath)
        {
            string projectPath = ExcelSettings.ProjectExcelPath;
            if (string.Equals(
                Path.GetFullPath(sourcePath),
                Path.GetFullPath(projectPath),
                StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                string? folder = Path.GetDirectoryName(projectPath);
                if (string.IsNullOrEmpty(folder))
                    return;

                Directory.CreateDirectory(folder);
                File.Copy(sourcePath, projectPath, overwrite: true);
            }
            catch
            {
                // Published RunApp may not write into Source/Data.
            }
        }

        private static void WriteEmployeesSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Employees");
            sheet.Cell(1, 1).Value = "Username";
            sheet.Cell(1, 2).Value = "Password";
            sheet.Cell(1, 3).Value = "EmployeeNumber";
            sheet.Cell(1, 4).Value = "Email";
            sheet.Cell(1, 5).Value = "IdNumber";
            sheet.Cell(1, 6).Value = "Role";

            int row = 2;
            foreach (var employee in AppData.Employees)
            {
                sheet.Cell(row, 1).Value = employee.Username;
                sheet.Cell(row, 2).Value = employee.Password;
                sheet.Cell(row, 3).Value = employee.EmployeeNumber;
                sheet.Cell(row, 4).Value = employee.Email;
                sheet.Cell(row, 5).Value = employee.IdNumber;
                sheet.Cell(row, 6).Value = employee.Role;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteEmployeeApprovalsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("EmployeeApprovals");
            sheet.Cell(1, 1).Value = "Username";
            sheet.Cell(1, 2).Value = "IsApproved";
            sheet.Cell(1, 3).Value = "ApprovedBy";
            sheet.Cell(1, 4).Value = "ApprovedAt";

            int row = 2;
            foreach (var employee in AppData.Employees)
            {
                sheet.Cell(row, 1).Value = employee.Username;
                sheet.Cell(row, 2).Value = employee.IsApproved ? 1 : 0;
                sheet.Cell(row, 3).Value = employee.IsApproved ? "system" : "";
                sheet.Cell(row, 4).Value = employee.IsApproved
                    ? DateTime.UtcNow.ToString("o")
                    : "";
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteClientsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Clients");
            sheet.Cell(1, 1).Value = "IdNumber";
            sheet.Cell(1, 2).Value = "FullName";
            sheet.Cell(1, 3).Value = "Phone";
            sheet.Cell(1, 4).Value = "Email";
            sheet.Cell(1, 5).Value = "Gender";

            int row = 2;
            foreach (var client in AppData.Clients)
            {
                sheet.Cell(row, 1).Value = client.IdNumber;
                sheet.Cell(row, 2).Value = client.FullName;
                sheet.Cell(row, 3).Value = client.Phone;
                sheet.Cell(row, 4).Value = client.Email;
                sheet.Cell(row, 5).Value = client.Gender;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteAnimalsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Animals");
            sheet.Cell(1, 1).Value = "Id";
            sheet.Cell(1, 2).Value = "Name";
            sheet.Cell(1, 3).Value = "Species";
            sheet.Cell(1, 4).Value = "ChipNumber";
            sheet.Cell(1, 5).Value = "Weight";
            sheet.Cell(1, 6).Value = "BirthDate";
            sheet.Cell(1, 7).Value = "OwnerIdNumber";
            sheet.Cell(1, 8).Value = "LastVaccinationDate";

            int row = 2;
            foreach (var animal in AppData.Animals)
            {
                sheet.Cell(row, 1).Value = animal.Id;
                sheet.Cell(row, 2).Value = animal.Name;
                sheet.Cell(row, 3).Value = animal.Species;
                sheet.Cell(row, 4).Value = animal.ChipNumber;
                sheet.Cell(row, 5).Value = animal.Weight;
                sheet.Cell(row, 6).Value = animal.BirthDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 7).Value = animal.OwnerIdNumber;
                sheet.Cell(row, 8).Value = animal.LastVaccinationDate.ToString("yyyy-MM-dd");
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteMedicationsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Medications");
            sheet.Cell(1, 1).Value = "Id";
            sheet.Cell(1, 2).Value = "Name";
            sheet.Cell(1, 3).Value = "StockQuantity";
            sheet.Cell(1, 4).Value = "UnitPrice";
            sheet.Cell(1, 5).Value = "ExpirationDate";
            sheet.Cell(1, 6).Value = "Notes";

            int row = 2;
            foreach (var medication in AppData.Medications)
            {
                sheet.Cell(row, 1).Value = medication.Id;
                sheet.Cell(row, 2).Value = medication.Name;
                sheet.Cell(row, 3).Value = medication.StockQuantity;
                sheet.Cell(row, 4).Value = medication.UnitPrice;
                sheet.Cell(row, 5).Value = medication.ExpirationDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 6).Value = medication.Notes;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteVisitsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Visits");
            sheet.Cell(1, 1).Value = "Id";
            sheet.Cell(1, 2).Value = "AnimalChipNumber";
            sheet.Cell(1, 3).Value = "VisitDate";
            sheet.Cell(1, 4).Value = "Reason";
            sheet.Cell(1, 5).Value = "Symptoms";
            sheet.Cell(1, 6).Value = "Diagnosis";
            sheet.Cell(1, 7).Value = "VeterinarianName";
            sheet.Cell(1, 8).Value = "BaseCost";
            sheet.Cell(1, 9).Value = "MedicationName";
            sheet.Cell(1, 10).Value = "MedicationQuantity";
            sheet.Cell(1, 11).Value = "TotalCost";
            sheet.Cell(1, 12).Value = "ArrivalStatus";
            sheet.Cell(1, 13).Value = "ArrivalNote";

            int row = 2;
            foreach (var visit in AppData.Visits)
            {
                sheet.Cell(row, 1).Value = visit.Id;
                sheet.Cell(row, 2).Value = visit.AnimalChipNumber;
                sheet.Cell(row, 3).Value = visit.VisitDate.ToString("yyyy-MM-dd HH:mm");
                sheet.Cell(row, 4).Value = visit.Reason;
                sheet.Cell(row, 5).Value = visit.Symptoms;
                sheet.Cell(row, 6).Value = visit.Diagnosis;
                sheet.Cell(row, 7).Value = visit.VeterinarianName;
                sheet.Cell(row, 8).Value = visit.BaseCost;
                sheet.Cell(row, 9).Value = visit.MedicationName;
                sheet.Cell(row, 10).Value = visit.MedicationQuantity;
                sheet.Cell(row, 11).Value = visit.TotalCost;
                sheet.Cell(row, 12).Value = visit.ArrivalStatus;
                sheet.Cell(row, 13).Value = visit.ArrivalNote;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void WriteTreatmentsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Treatments");
            sheet.Cell(1, 1).Value = "Id";
            sheet.Cell(1, 2).Value = "VisitId";
            sheet.Cell(1, 3).Value = "Description";
            sheet.Cell(1, 4).Value = "MedicationName";
            sheet.Cell(1, 5).Value = "MedicationQuantity";
            sheet.Cell(1, 6).Value = "LineCost";

            int row = 2;
            foreach (var visit in AppData.Visits)
            {
                foreach (var line in visit.TreatmentLines)
                {
                    sheet.Cell(row, 1).Value = line.Id;
                    sheet.Cell(row, 2).Value = line.VisitId;
                    sheet.Cell(row, 3).Value = line.Description;
                    sheet.Cell(row, 4).Value = line.MedicationName;
                    sheet.Cell(row, 5).Value = line.MedicationQuantity;
                    sheet.Cell(row, 6).Value = line.LineCost;
                    row++;
                }
            }

            sheet.Columns().AdjustToContents();
        }
    }
}
