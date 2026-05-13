using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class ClientsView : UserControl
    {
        public Action? BackToMainMenu;

        public ClientsView()
        {
            InitializeComponent();
            RefreshClientsList();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            string fullName = FullNameInput.Text?.Trim() ?? "";
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string phone = PhoneInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(fullName) &&
                string.IsNullOrWhiteSpace(idNumber) &&
                string.IsNullOrWhiteSpace(phone) &&
                string.IsNullOrWhiteSpace(email))
            {
                ValidationText.Text = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(fullName) && !ValidationService.IsValidFullName(fullName))
            {
                SetValidationMessage("שם מלא חייב להכיל אותיות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(idNumber) && !ValidationService.IsValidIdNumber(idNumber))
            {
                SetValidationMessage("תעודת זהות חייבת להיות 9 ספרות", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(phone) && !ValidationService.IsValidPhone(phone))
            {
                SetValidationMessage("טלפון חייב להיות 9-10 ספרות", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !ValidationService.IsValidEmail(email))
            {
                SetValidationMessage("אימייל לא תקין", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void AddClient_Click(object? sender, RoutedEventArgs e)
        {
            string fullName = FullNameInput.Text?.Trim() ?? "";
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string phone = PhoneInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";

            if (!ValidateClientFields(fullName, idNumber, phone, email, shouldValidateId: true))
                return;

            bool idExists = AppData.Clients.Any(client => client.IdNumber == idNumber);
            bool phoneExists = AppData.Clients.Any(client => client.Phone == phone);

            if (idExists || phoneExists)
            {
                UIHelper.ShowMessage(this, "לקוח עם תעודת זהות או טלפון אלה כבר קיים");
                return;
            }

            AppData.Clients.Add(new Client
            {
                FullName = fullName,
                IdNumber = idNumber,
                Phone = phone,
                Email = email
            });

            AppData.SaveClientsToDatabase();

            UIHelper.ShowMessage(this, "הלקוח נוסף בהצלחה");
            ClearFields();
            RefreshClientsList();
        }

        private void SearchClient_Click(object? sender, RoutedEventArgs e)
        {
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string phone = PhoneInput.Text?.Trim() ?? "";

            var client = AppData.Clients.FirstOrDefault(c =>
                c.IdNumber == idNumber || c.Phone == phone);

            if (client == null)
            {
                UIHelper.ShowMessage(this, "לא נמצא לקוח");
                return;
            }

            FullNameInput.Text = client.FullName;
            IdNumberInput.Text = client.IdNumber;
            PhoneInput.Text = client.Phone;
            EmailInput.Text = client.Email;
        }

        private void UpdateClient_Click(object? sender, RoutedEventArgs e)
        {
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            var client = AppData.Clients.FirstOrDefault(c => c.IdNumber == idNumber);

            if (client == null)
            {
                UIHelper.ShowMessage(this, "לא נמצא לקוח לעדכון");
                return;
            }

            string fullName = FullNameInput.Text?.Trim() ?? "";
            string phone = PhoneInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";

            if (!ValidateClientFields(fullName, idNumber, phone, email, shouldValidateId: false))
                return;

            bool phoneBelongsToOtherClient = AppData.Clients.Any(c =>
                c.IdNumber != idNumber && c.Phone == phone);

            if (phoneBelongsToOtherClient)
            {
                UIHelper.ShowMessage(this, "מספר הטלפון כבר משויך ללקוח אחר");
                return;
            }

            client.FullName = fullName;
            client.Phone = phone;
            client.Email = email;

            AppData.SaveClientsToDatabase();

            UIHelper.ShowMessage(this, "הלקוח עודכן בהצלחה");
            RefreshClientsList();
        }

        private void DeleteClient_Click(object? sender, RoutedEventArgs e)
        {
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            var client = AppData.Clients.FirstOrDefault(c => c.IdNumber == idNumber);

            if (client == null)
            {
                UIHelper.ShowMessage(this, "לא נמצא לקוח למחיקה");
                return;
            }

            bool hasAnimals = AppData.Animals.Any(animal => animal.OwnerIdNumber == idNumber);

            if (hasAnimals)
            {
                UIHelper.ShowMessage(this, "לא ניתן למחוק לקוח שיש לו בעלי חיים במערכת");
                return;
            }

            AppData.Clients.Remove(client);
            AppData.SaveClientsToDatabase();

            ClearFields();
            UIHelper.ShowMessage(this, "הלקוח נמחק בהצלחה");
            RefreshClientsList();
        }

        private bool ValidateClientFields(
            string fullName,
            string idNumber,
            string phone,
            string email,
            bool shouldValidateId)
        {
            if (!ValidationService.IsValidFullName(fullName))
            {
                UIHelper.ShowMessage(this, "שם מלא חייב להכיל אותיות בלבד");
                return false;
            }

            if (shouldValidateId && !ValidationService.IsValidIdNumber(idNumber))
            {
                UIHelper.ShowMessage(this, "תעודת זהות חייבת להיות 9 ספרות");
                return false;
            }

            if (!ValidationService.IsValidPhone(phone))
            {
                UIHelper.ShowMessage(this, "טלפון חייב להיות 9-10 ספרות");
                return false;
            }

            if (!ValidationService.IsValidEmail(email))
            {
                UIHelper.ShowMessage(this, "אימייל לא תקין");
                return false;
            }

            return true;
        }

        private void ClearFields_Click(object? sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void OpenClientAnimals_Click(object? sender, RoutedEventArgs e)
        {
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            var client = AppData.Clients.FirstOrDefault(c => c.IdNumber == idNumber);

            if (client == null)
            {
                UIHelper.ShowMessage(this, "יש לחפש או להזין לקוח קיים לפי תעודת זהות");
                return;
            }

            var animals = AppData.Animals
                .Where(animal => animal.OwnerIdNumber == idNumber)
                .ToList();

            if (animals.Count == 0)
            {
                ClientsTextBlock.Text = $"ללקוח {client.FullName} אין בעלי חיים רשומים";
                return;
            }

            string text = $"בעלי החיים של {client.FullName}:\n\n";

            foreach (var animal in animals)
            {
                text += $"שם: {animal.Name}\n";
                text += $"סוג: {animal.Species}\n";
                text += $"שבב: {animal.ChipNumber}\n";
                text += $"משקל: {animal.Weight} קג\n";
                text += $"חיסון אחרון: {animal.LastVaccinationDate:dd/MM/yyyy}\n";
                text += "-----------------------------\n";
            }

            ClientsTextBlock.Text = text;
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }

        private void ClearFields()
        {
            FullNameInput.Text = "";
            IdNumberInput.Text = "";
            PhoneInput.Text = "";
            EmailInput.Text = "";
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void RefreshClientsList()
        {
            if (AppData.Clients.Count == 0)
            {
                ClientsTextBlock.Text = "אין לקוחות במערכת";
                return;
            }

            string text = "";

            foreach (var client in AppData.Clients)
            {
                text += $"שם: {client.FullName}\n";
                text += $"תעודת זהות: {client.IdNumber}\n";
                text += $"טלפון: {client.Phone}\n";
                text += $"אימייל: {client.Email}\n";
                text += "-----------------------------\n";
            }

            ClientsTextBlock.Text = text;
        }
    }
}