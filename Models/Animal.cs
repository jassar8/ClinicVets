using System;

namespace ClinicVetsAvalonia.Models
{
    public class Animal
    {
        public int Id { get; set; }

        // Animal name - letters only
        public string Name { get; set; } = "";

        // Dog / Cat / Reptile / Bird
        public string Species { get; set; } = "";

        // Unique serial/chip number for search
        public string ChipNumber { get; set; } = "";

        // Decimal positive weight: 0.1 - 100 kg
        public double Weight { get; set; }

        // Cannot be future date and not before year 2000
        public DateTime BirthDate { get; set; }

        // Link to existing client by IdNumber
        public string OwnerIdNumber { get; set; } = "";

        // Last vaccination date
        public DateTime LastVaccinationDate { get; set; }

        public override string ToString()
        {
            return $"Name: {Name} | Type: {Species} | Chip: {ChipNumber} | Owner ID: {OwnerIdNumber}";
        }
    }
}