namespace ClinicManagementSystem.app.Models
{
    public class Employee
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmployeeNumber { get; set; }
        public string Email { get; set; }

        // Secretary / Vet
        public string Role { get; set; }
    }
}
