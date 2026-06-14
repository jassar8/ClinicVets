using System;
using System.Collections.Generic;
using System.Linq;
using ClinicVetsAvalonia.Data.Repositories;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Data
{
    public static class FakeDataSeeder
    {
        public static bool IsDatabaseEmpty()
        {
            var employeeRepository = new EmployeeRepository();
            var clientRepository = new ClientRepository();
            return employeeRepository.LoadAll().Count == 0 &&
                   clientRepository.LoadAll().Count == 0;
        }

        public static void SeedAllIfEmpty()
        {
            if (!DatabaseSettings.SeedDemoDataWhenEmpty || !IsDatabaseEmpty())
                return;

            var employees = CreateEmployees();
            var clients = CreateClients();
            var animals = CreateAnimals(clients);
            var medications = CreateMedications();
            var visits = CreateVisits(animals, medications, employees);

            var employeeRepository = new EmployeeRepository();
            foreach (var employee in employees)
                employeeRepository.Insert(employee);

            new ClientRepository().SaveAll(clients);
            new AnimalRepository().SaveAll(animals);
            new MedicationRepository().SaveAll(medications);
            new VisitRepository().SaveAll(visits);
        }

        public static IReadOnlyList<Employee> CreateEmployees() =>
        [
            new Employee
            {
                Username = "admin1",
                Password = "Admin123!",
                EmployeeNumber = "1001",
                Email = "admin@clinic.com",
                IdNumber = "300000018",
                Role = "Secretary"
            },
            new Employee
            {
                Username = "secuser",
                Password = "Sec123!a",
                EmployeeNumber = "1002",
                Email = "sec@clinic.com",
                IdNumber = "300000027",
                Role = "Secretary"
            },
            new Employee
            {
                Username = "vetuser",
                Password = "Vet123!a",
                EmployeeNumber = "1003",
                Email = "vet@clinic.com",
                IdNumber = "300000036",
                Role = "Vet"
            },
            new Employee
            {
                Username = "sarah1",
                Password = "Pass123!",
                EmployeeNumber = "1004",
                Email = "sarah@clinic.com",
                IdNumber = "301234567",
                Role = "Secretary"
            },
            new Employee
            {
                Username = "david2",
                Password = "Pass123!",
                EmployeeNumber = "1005",
                Email = "david@clinic.com",
                IdNumber = "302345678",
                Role = "Vet"
            },
            new Employee
            {
                Username = "roni12",
                Password = "Pass123!",
                EmployeeNumber = "1006",
                Email = "roni@clinic.com",
                IdNumber = "303456789",
                Role = "Vet"
            }
        ];

        public static IReadOnlyList<Client> CreateClients() =>
        [
            new Client
            {
                FullName = "Fares Mansour",
                IdNumber = "123456782",
                Phone = "0501234567",
                Email = "fares@gmail.com",
                Gender = "זכר"
            },
            new Client
            {
                FullName = "Maya Levi",
                IdNumber = "234567891",
                Phone = "0529876543",
                Email = "maya@example.co.il",
                Gender = "נקבה"
            },
            new Client
            {
                FullName = "Yosef Cohen",
                IdNumber = "345678902",
                Phone = "0541112233",
                Email = "yosef@company.org",
                Gender = "זכר"
            },
            new Client
            {
                FullName = "Dana Shapiro",
                IdNumber = "456789013",
                Phone = "0534445566",
                Email = "dana@site.net",
                Gender = "נקבה"
            },
            new Client
            {
                FullName = "Ron Azulay",
                IdNumber = "567890124",
                Phone = "0509988776",
                Email = "ron@clinic.com",
                Gender = "זכר"
            }
        ];

        public static IReadOnlyList<Animal> CreateAnimals(IReadOnlyList<Client> clients)
        {
            var today = DateTime.Today;
            return
            [
                new Animal
                {
                    Name = "ZAZA",
                    Species = "כלב",
                    ChipNumber = "3761234",
                    Weight = 12.5,
                    BirthDate = new DateTime(2020, 3, 15),
                    OwnerIdNumber = clients[0].IdNumber,
                    LastVaccinationDate = today.AddMonths(-8)
                },
                new Animal
                {
                    Name = "Mitzi",
                    Species = "חתול",
                    ChipNumber = "3762345",
                    Weight = 4.2,
                    BirthDate = new DateTime(2021, 7, 20),
                    OwnerIdNumber = clients[1].IdNumber,
                    LastVaccinationDate = today.AddMonths(-3)
                },
                new Animal
                {
                    Name = "Rio",
                    Species = "ציפור",
                    ChipNumber = "3763456",
                    Weight = 0.3,
                    BirthDate = new DateTime(2022, 1, 10),
                    OwnerIdNumber = clients[2].IdNumber,
                    LastVaccinationDate = today.AddMonths(-11)
                },
                new Animal
                {
                    Name = "Spike",
                    Species = "זוחל",
                    ChipNumber = "3764567",
                    Weight = 1.8,
                    BirthDate = new DateTime(2019, 11, 5),
                    OwnerIdNumber = clients[3].IdNumber,
                    LastVaccinationDate = today.AddMonths(-14)
                },
                new Animal
                {
                    Name = "Buddy",
                    Species = "כלב",
                    ChipNumber = "3765678",
                    Weight = 28.0,
                    BirthDate = new DateTime(2018, 5, 22),
                    OwnerIdNumber = clients[4].IdNumber,
                    LastVaccinationDate = today.AddDays(-20)
                },
                new Animal
                {
                    Name = "Luna",
                    Species = "חתול",
                    ChipNumber = "3766789",
                    Weight = 3.9,
                    BirthDate = new DateTime(2023, 2, 8),
                    OwnerIdNumber = clients[0].IdNumber,
                    LastVaccinationDate = today.AddMonths(-1)
                }
            ];
        }

        public static IReadOnlyList<Medication> CreateMedications()
        {
            var today = DateTime.Today;
            return
            [
                new Medication
                {
                    Name = "Antibiotic",
                    StockQuantity = 45,
                    UnitPrice = 25.0,
                    ExpirationDate = today.AddMonths(10),
                    Notes = "שגרתי למסלול טיפול"
                },
                new Medication
                {
                    Name = "Pain Relief",
                    StockQuantity = 30,
                    UnitPrice = 18.5,
                    ExpirationDate = today.AddMonths(6),
                    Notes = "לאחר ניתוח"
                },
                new Medication
                {
                    Name = "Rabies Vaccine",
                    StockQuantity = 20,
                    UnitPrice = 95.0,
                    ExpirationDate = today.AddMonths(12),
                    Notes = "חיסון שנתי"
                },
                new Medication
                {
                    Name = "Flea Drops",
                    StockQuantity = 25,
                    UnitPrice = 42.0,
                    ExpirationDate = today.AddMonths(8),
                    Notes = "טיפול חיצוני"
                },
                new Medication
                {
                    Name = "Vitamin Complex",
                    StockQuantity = 50,
                    UnitPrice = 12.0,
                    ExpirationDate = today.AddMonths(14),
                    Notes = "תוסף תזונה"
                },
                new Medication
                {
                    Name = "Ear Drops",
                    StockQuantity = 15,
                    UnitPrice = 32.0,
                    ExpirationDate = today.AddDays(45),
                    Notes = "דורש מעקב מלאי"
                }
            ];
        }

        public static IReadOnlyList<Visit> CreateVisits(
            IReadOnlyList<Animal> animals,
            IReadOnlyList<Medication> medications,
            IReadOnlyList<Employee> employees)
        {
            var today = DateTime.Today;
            string vetName = employees.First(employee => employee.Role == "Vet").Username;

            var visits = new List<Visit>
            {
                BuildVisit(
                    animals[0].ChipNumber,
                    today.AddDays(4).AddHours(10),
                    "Vaccination",
                    "Scheduled",
                    vetName,
                    120,
                    medications[2],
                    1,
                    "Annual rabies booster"),
                BuildVisit(
                    animals[1].ChipNumber,
                    today.AddDays(-5).AddHours(11),
                    "Checkup",
                    "Arrived",
                    vetName,
                    90,
                    medications[4],
                    2,
                    "Routine wellness exam"),
                BuildVisit(
                    animals[2].ChipNumber,
                    today.AddDays(2).AddHours(9),
                    "Wing trim",
                    "Scheduled",
                    vetName,
                    80,
                    null,
                    0,
                    "Scheduled grooming visit"),
                BuildVisit(
                    animals[3].ChipNumber,
                    today.AddDays(-12).AddHours(14),
                    "Skin issue",
                    "Arrived",
                    vetName,
                    110,
                    medications[0],
                    3,
                    "Antibiotic course for infection"),
                BuildVisit(
                    animals[4].ChipNumber,
                    today.AddDays(-1).AddHours(16),
                    "Limping",
                    "NoShow",
                    vetName,
                    0,
                    null,
                    0,
                    "Owner did not arrive"),
                BuildVisit(
                    animals[5].ChipNumber,
                    today.AddHours(15),
                    "Vaccination",
                    "Scheduled",
                    vetName,
                    100,
                    medications[3],
                    1,
                    "Flea prevention treatment")
            };

            foreach (var visit in visits)
                visit.SyncLegacyMedicationFields();

            return visits;
        }

        private static Visit BuildVisit(
            string chipNumber,
            DateTime visitDate,
            string reason,
            string arrivalStatus,
            string veterinarianName,
            double baseCost,
            Medication? medication,
            int medicationQuantity,
            string diagnosis)
        {
            var visit = new Visit
            {
                AnimalChipNumber = chipNumber,
                VisitDate = visitDate,
                Reason = reason,
                Symptoms = arrivalStatus == "Scheduled" ? "" : "Reported by owner",
                Diagnosis = diagnosis,
                VeterinarianName = veterinarianName,
                BaseCost = baseCost,
                ArrivalStatus = arrivalStatus,
                ArrivalNote = arrivalStatus == "NoShow" ? "לא הגיע לתור" : ""
            };

            if (medication != null && medicationQuantity > 0)
            {
                double lineCost = medication.UnitPrice * medicationQuantity;
                visit.TreatmentLines.Add(new VisitTreatmentLine
                {
                    Description = diagnosis,
                    MedicationName = medication.Name,
                    MedicationQuantity = medicationQuantity,
                    LineCost = lineCost
                });
            }
            else if (baseCost > 0)
            {
                visit.TreatmentLines.Add(new VisitTreatmentLine
                {
                    Description = diagnosis,
                    MedicationName = "",
                    MedicationQuantity = 0,
                    LineCost = 0
                });
            }

            visit.SyncLegacyMedicationFields();
            return visit;
        }

        /// <summary>Validates seed payloads against ValidationService (used in tests).</summary>
        public static bool ValidateSeedData()
        {
            var clients = CreateClients();
            var animals = CreateAnimals(clients);
            var medications = CreateMedications();
            var employees = CreateEmployees();
            var visits = CreateVisits(animals, medications, employees);

            foreach (var employee in employees)
            {
                if (!ValidationService.IsValidUsername(employee.Username))
                    return false;
                if (!ValidationService.IsValidPassword(employee.Password))
                    return false;
                if (!ValidationService.IsValidEmployeeNumber(employee.EmployeeNumber))
                    return false;
                if (!ValidationService.IsValidIdNumber(employee.IdNumber))
                    return false;
                if (!ValidationService.IsValidEmail(employee.Email))
                    return false;
                if (!ValidationService.IsValidRole(employee.Role))
                    return false;
            }

            foreach (var client in clients)
            {
                if (!ValidationService.IsValidFullName(client.FullName))
                    return false;
                if (!ValidationService.IsValidIdNumber(client.IdNumber))
                    return false;
                if (!ValidationService.IsValidPhone(client.Phone))
                    return false;
                if (!ValidationService.IsValidEmail(client.Email))
                    return false;
            }

            foreach (var animal in animals)
            {
                if (!clients.Any(client => client.IdNumber == animal.OwnerIdNumber))
                    return false;
                if (!ValidationService.IsValidAnimalName(animal.Name))
                    return false;
                if (!ValidationService.IsValidAnimalSpecies(animal.Species))
                    return false;
                if (!ValidationService.IsValidChipNumber(animal.ChipNumber))
                    return false;
                if (!ValidationService.IsValidWeight(animal.Weight))
                    return false;
                if (!ValidationService.IsValidBirthDate(animal.BirthDate))
                    return false;
                if (!ValidationService.IsValidVaccinationDateForBirthDate(
                        animal.LastVaccinationDate, animal.BirthDate))
                    return false;
            }

            foreach (var medication in medications)
            {
                if (!ValidationService.IsValidStockQuantity(medication.StockQuantity) ||
                    medication.StockQuantity <= 0)
                    return false;
                if (!ValidationService.IsValidMoney(medication.UnitPrice))
                    return false;
                if (!ValidationService.IsValidExpirationDate(medication.ExpirationDate))
                    return false;
            }

            foreach (var visit in visits)
            {
                if (!animals.Any(animal => animal.ChipNumber == visit.AnimalChipNumber))
                    return false;
                if (!ValidationService.IsValidVisitDate(visit.VisitDate))
                    return false;

                foreach (var line in visit.TreatmentLines)
                {
                    if (string.IsNullOrWhiteSpace(line.MedicationName))
                        continue;

                    var medication = medications.FirstOrDefault(m => m.Name == line.MedicationName);
                    if (medication == null)
                        return false;
                    if (line.MedicationQuantity > medication.StockQuantity)
                        return false;
                }
            }

            return true;
        }
    }
}
