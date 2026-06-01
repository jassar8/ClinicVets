namespace ClinicVetsAvalonia.Models
{
    public class Employee
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string EmployeeNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string IdNumber { get; set; } = "";

        // Secretary / Vet
        public string Role { get; set; } = "";

        public bool IsApproved { get; set; } = true;
    }
}