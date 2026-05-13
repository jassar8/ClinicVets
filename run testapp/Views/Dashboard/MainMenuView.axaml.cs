using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Views.Dashboard
{
    public partial class MainMenuView : UserControl
    {
        private readonly Employee currentEmployee;

        public Action? OpenClients;
        public Action? OpenAnimals;
        public Action? OpenVisits;
        public Action? OpenMedicines;
        public Action? Logout;

        public MainMenuView()
        {
            InitializeComponent();

            currentEmployee = new Employee
            {
                Username = "Unknown",
                Role = "Secretary"
            };

            ApplyEmployeeData();
        }

        public MainMenuView(Employee employee)
        {
            InitializeComponent();

            currentEmployee = employee;
            ApplyEmployeeData();
        }

        private void ApplyEmployeeData()
        {
            LoggedInText.Text =
                $"מחובר כ: {currentEmployee.Username} | תפקיד: {GetRoleText(currentEmployee.Role)}";

            if (currentEmployee.Role == "Secretary")
            {
                ClientsButton.IsEnabled = true;
                ClientsCardButton.IsEnabled = true;
                AnimalsButton.IsEnabled = true;
                AnimalsCardButton.IsEnabled = true;
                VisitsButton.IsEnabled = false;
                VisitsCardButton.IsEnabled = false;
                MedicinesButton.IsEnabled = false;
                MedicinesCardButton.IsEnabled = false;
            }
            else if (currentEmployee.Role == "Vet")
            {
                ClientsButton.IsEnabled = false;
                ClientsCardButton.IsEnabled = false;
                AnimalsButton.IsEnabled = true;
                AnimalsCardButton.IsEnabled = true;
                VisitsButton.IsEnabled = true;
                VisitsCardButton.IsEnabled = true;
                MedicinesButton.IsEnabled = true;
                MedicinesCardButton.IsEnabled = true;
            }
            else
            {
                ClientsButton.IsEnabled = false;
                ClientsCardButton.IsEnabled = false;
                AnimalsButton.IsEnabled = false;
                AnimalsCardButton.IsEnabled = false;
                VisitsButton.IsEnabled = false;
                VisitsCardButton.IsEnabled = false;
                MedicinesButton.IsEnabled = false;
                MedicinesCardButton.IsEnabled = false;
            }
        }

        private void Clients_Click(object? sender, RoutedEventArgs e)
        {
            OpenClients?.Invoke();
        }

        private void Animals_Click(object? sender, RoutedEventArgs e)
        {
            OpenAnimals?.Invoke();
        }

        private void Visits_Click(object? sender, RoutedEventArgs e)
        {
            OpenVisits?.Invoke();
        }

        private void Medicines_Click(object? sender, RoutedEventArgs e)
        {
            OpenMedicines?.Invoke();
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            Logout?.Invoke();
        }

        private string GetRoleText(string role)
        {
            return role == "Vet" ? "וטרינר/ית" : "מזכיר/ה";
        }
    }
}