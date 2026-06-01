using System;
using System.IO;
using ClinicVetsAvalonia.Data;

namespace ClinicVetsAvalonia.Tests;

public class ExcelExportIntegrationTests
{
    [Fact]
    public void Initialize_CreatesExcelWorkbookWithExpectedSheets()
    {
        string tempDb = Path.Combine(Path.GetTempPath(), $"clinicvets-test-{Guid.NewGuid():N}.db");
        string? previousDb = Environment.GetEnvironmentVariable("CLINICVETS_DB");

        try
        {
            Environment.SetEnvironmentVariable("CLINICVETS_DB", tempDb);
            AppData.Initialize();

            string excelPath = ExcelExportService.LastExportPath;
            Assert.False(string.IsNullOrWhiteSpace(excelPath));
            Assert.True(File.Exists(excelPath), $"Expected Excel at {excelPath}");

            using var workbook = new ClosedXML.Excel.XLWorkbook(excelPath);
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Employees");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "EmployeeApprovals");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Clients");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Animals");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Medications");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Visits");
            Assert.Contains(workbook.Worksheets, ws => ws.Name == "Treatments");
        }
        finally
        {
            Environment.SetEnvironmentVariable("CLINICVETS_DB", previousDb);
            try
            {
                if (File.Exists(tempDb))
                    File.Delete(tempDb);
            }
            catch (IOException)
            {
                // SQLite may still release the file briefly after connections close.
            }
        }
    }
}
