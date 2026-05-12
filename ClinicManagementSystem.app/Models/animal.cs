using System;

namespace ClinicVets.Models
{
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; } // e.g., Dog, Cat
        public string Breed { get; set; }
        public int Age { get; set; }
        public string OwnerName { get; set; }
        public string MedicalHistory { get; set; }
        public DateTime LastVisit { get; set; }

        public override string ToString()
        {
            return $"ID: {Id} | Name: {Name} ({Species}) | Owner: {OwnerName} | Last Visit: {LastVisit.ToShortDateString()}";
        }
    }
}
