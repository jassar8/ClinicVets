using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Data;
using ClinicManagementSystem.app.Helpers;
using ClinicManagementSystem.app.Models;

namespace ClinicManagementSystem.app.Views
{
    public class ClientsView : NSView
    {
        public Action BackToMainMenu;
        public Action OpenClientAnimalsPage;

        private NSTextView clientsTextView;

        public ClientsView() : base(new CGRect(0, 0, 900, 650))
        {
            BuildUI();
        }

        private void BuildUI()
        {
            AddSubview(UIHelper.CreateLabel("ניהול לקוחות", 300, 560, 300, 50, true));

            var fullNameInput = UIHelper.CreateInput("שם מלא", 80, 480, 240, 35);
            var idNumberInput = UIHelper.CreateInput("תעודת זהות", 80, 430, 240, 35);
            var phoneInput = UIHelper.CreateInput("טלפון", 80, 380, 240, 35);
            var emailInput = UIHelper.CreateInput("אימייל", 80, 330, 240, 35);

            AddSubview(fullNameInput);
            AddSubview(idNumberInput);
            AddSubview(phoneInput);
            AddSubview(emailInput);

            clientsTextView = new NSTextView(new CGRect(380, 180, 430, 310))
            {
                Editable = false,
                Font = NSFont.SystemFontOfSize(14)
            };

            var scrollView = new NSScrollView(new CGRect(380, 180, 430, 310))
            {
                HasVerticalScroller = true,
                DocumentView = clientsTextView
            };

            AddSubview(scrollView);
            RefreshClientsList();

            AddSubview(UIHelper.CreateButton("הוסף לקוח", 80, 270, 110, 35, (sender, e) =>
            {
                string fullName = fullNameInput.StringValue.Trim();
                string idNumber = idNumberInput.StringValue.Trim();
                string phone = phoneInput.StringValue.Trim();
                string email = emailInput.StringValue.Trim();

                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(idNumber) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(email))
                {
                    UIHelper.ShowMessage("יש למלא את כל השדות");
                    return;
                }

                bool exists = AppData.Clients.Any(client => client.IdNumber == idNumber);

                if (exists)
                {
                    UIHelper.ShowMessage("לקוח עם תעודת זהות זו כבר קיים");
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

                UIHelper.ShowMessage("הלקוח נוסף בהצלחה");
                RefreshClientsList();
            }));

            AddSubview(UIHelper.CreateButton("חפש לקוח", 210, 270, 110, 35, (sender, e) =>
            {
                string idNumber = idNumberInput.StringValue.Trim();
                string phone = phoneInput.StringValue.Trim();

                var client = AppData.Clients.FirstOrDefault(c =>
                    c.IdNumber == idNumber || c.Phone == phone);

                if (client == null)
                {
                    UIHelper.ShowMessage("לא נמצא לקוח");
                    return;
                }

                fullNameInput.StringValue = client.FullName;
                idNumberInput.StringValue = client.IdNumber;
                phoneInput.StringValue = client.Phone;
                emailInput.StringValue = client.Email;
            }));

            AddSubview(UIHelper.CreateButton("עדכן לקוח", 80, 220, 110, 35, (sender, e) =>
            {
                string idNumber = idNumberInput.StringValue.Trim();

                var client = AppData.Clients.FirstOrDefault(c => c.IdNumber == idNumber);

                if (client == null)
                {
                    UIHelper.ShowMessage("לא נמצא לקוח לעדכון");
                    return;
                }

                client.FullName = fullNameInput.StringValue.Trim();
                client.Phone = phoneInput.StringValue.Trim();
                client.Email = emailInput.StringValue.Trim();
                AppData.SaveClientsToDatabase();

                UIHelper.ShowMessage("הלקוח עודכן בהצלחה");
                RefreshClientsList();
            }));

            AddSubview(UIHelper.CreateButton("מחק לקוח", 210, 220, 110, 35, (sender, e) =>
            {
                string idNumber = idNumberInput.StringValue.Trim();

                var client = AppData.Clients.FirstOrDefault(c => c.IdNumber == idNumber);

                if (client == null)
                {
                    UIHelper.ShowMessage("לא נמצא לקוח למחיקה");
                    return;
                }

                AppData.Clients.Remove(client);

                fullNameInput.StringValue = "";
                idNumberInput.StringValue = "";
                phoneInput.StringValue = "";
                emailInput.StringValue = "";
                AppData.SaveClientsToDatabase();

                UIHelper.ShowMessage("הלקוח נמחק בהצלחה");
                RefreshClientsList();
            }));

            AddSubview(UIHelper.CreateButton("נקה שדות", 80, 170, 110, 35, (sender, e) =>
            {
                fullNameInput.StringValue = "";
                idNumberInput.StringValue = "";
                phoneInput.StringValue = "";
                emailInput.StringValue = "";
            }));

            AddSubview(UIHelper.CreateButton("הצג חיות של לקוח", 210, 170, 130, 35, (sender, e) =>
            {
                OpenClientAnimalsPage?.Invoke();
            }));

            AddSubview(UIHelper.CreateButton("חזרה", 80, 100, 240, 40, (sender, e) =>
            {
                BackToMainMenu?.Invoke();
            }));
        }

        private void RefreshClientsList()
        {
            if (AppData.Clients.Count == 0)
            {
                clientsTextView.Value = "אין לקוחות במערכת";
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

            clientsTextView.Value = text;
        }
    }
}
