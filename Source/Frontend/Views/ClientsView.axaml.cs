using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
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
            string gender = GetSelectedGender();

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

            UpdateEmailValidationText(email);

            if (!string.IsNullOrWhiteSpace(email) && !ValidationService.IsValidEmail(email))
                return;

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void UpdateEmailValidationText(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailValidationText.Text = "";
                return;
            }

            string? message = ValidationService.GetEmailValidationMessage(email);
            EmailValidationText.Text = message ?? "";
        }

        private void AddClient_Click(object? sender, RoutedEventArgs e)
        {
            string fullName = FullNameInput.Text?.Trim() ?? "";
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string phone = PhoneInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";
            string gender = GetSelectedGender();

            if (!ValidateClientFields(fullName, idNumber, phone, email, shouldValidateId: true))
                return;

            bool idExists = AppData.Clients.Any(client => client.IdNumber == idNumber);
            bool phoneExists = AppData.Clients.Any(client => client.Phone == phone);

            if (idExists || phoneExists)
            {
                UIHelper.ShowMessage(this, "לקוח עם תעודת זהות או טלפון אלה כבר קיים");
                return;
            }

            var newClient = new Client
            {
                FullName = fullName,
                IdNumber = idNumber,
                Phone = phone,
                Email = email,
                Gender = gender
            };

            AppData.Clients.Add(newClient);

            try
            {
                AppData.SaveClientsToDatabase();
            }
            catch (Exception ex)
            {
                AppData.Clients.Remove(newClient);
                UIHelper.ShowMessage(this, $"שמירת הלקוח נכשלה ולא נסגרה האפליקציה. פרטי השגיאה: {ex.Message}");
                return;
            }

            UIHelper.ShowMessage(this, "הלקוח נוסף בהצלחה");
            ClearFields();
            RefreshClientsList();
        }

        private void SearchClient_Click(object? sender, RoutedEventArgs e)
        {
            string searchValue = ClientSearchInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                ClientSearchStatusText.Foreground = Brushes.Firebrick;
                ClientSearchStatusText.Text = "יש להזין שם מלא או מספר טלפון לחיפוש";
                return;
            }

            var client = AppData.Clients.FirstOrDefault(c => MatchesPhoneOrName(c, searchValue));

            if (client == null)
            {
                ClientSearchStatusText.Foreground = Brushes.Firebrick;
                ClientSearchStatusText.Text = "לא נמצא לקוח לפי שם או טלפון";
                return;
            }

            FillClientFields(client);
            ClientSearchStatusText.Foreground = Brushes.ForestGreen;
            ClientSearchStatusText.Text = $"נמצא: {client.FullName}";
        }

        private static bool MatchesPhoneOrName(Client client, string searchValue)
        {
            return client.Phone == searchValue ||
                   client.FullName.Contains(searchValue, StringComparison.OrdinalIgnoreCase);
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
            string gender = GetSelectedGender();

            if (!ValidateClientFields(fullName, idNumber, phone, email, shouldValidateId: false))
                return;

            bool phoneBelongsToOtherClient = AppData.Clients.Any(c =>
                c.IdNumber != idNumber && c.Phone == phone);

            if (phoneBelongsToOtherClient)
            {
                UIHelper.ShowMessage(this, "מספר הטלפון כבר משויך ללקוח אחר");
                return;
            }

            string previousFullName = client.FullName;
            string previousPhone = client.Phone;
            string previousEmail = client.Email;
            string previousGender = client.Gender;

            client.FullName = fullName;
            client.Phone = phone;
            client.Email = email;
            client.Gender = gender;

            try
            {
                AppData.SaveClientsToDatabase();
            }
            catch (Exception ex)
            {
                client.FullName = previousFullName;
                client.Phone = previousPhone;
                client.Email = previousEmail;
                client.Gender = previousGender;

                UIHelper.ShowMessage(this, $"עדכון הלקוח נכשל ולא נסגרה האפליקציה. פרטי השגיאה: {ex.Message}");
                RefreshClientsList();
                return;
            }

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

            try
            {
                AppData.SaveClientsToDatabase();
            }
            catch (Exception ex)
            {
                if (!AppData.Clients.Any(c => c.IdNumber == client.IdNumber))
                    AppData.Clients.Add(client);

                UIHelper.ShowMessage(this, $"מחיקת הלקוח נכשלה ולא נסגרה האפליקציה. פרטי השגיאה: {ex.Message}");
                RefreshClientsList();
                return;
            }

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
                UpdateEmailValidationText(email);
                SetValidationMessage("יש לתקן את האימייל לפני שמירה", isValid: false);
                return false;
            }

            EmailValidationText.Text = "";
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
                ClientDetailsText.Text = $"ללקוח {client.FullName} אין בעלי חיים רשומים";
                ClientCardsPanel.Children.Clear();
                BackToClientsButton.IsVisible = true;
                return;
            }

            ClientDetailsText.Text = $"בעלי החיים של {client.FullName}: לחץ על כרטיס חיה כדי לראות פרטים";
            ClientCardsPanel.Children.Clear();
            BackToClientsButton.IsVisible = true;

            foreach (var animal in animals)
            {
                ClientCardsPanel.Children.Add(CreateOwnedAnimalCard(animal));
            }
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }

        private void BackToClients_Click(object? sender, RoutedEventArgs e)
        {
            RefreshClientsList();
        }

        private void ClearFields()
        {
            FullNameInput.Text = "";
            IdNumberInput.Text = "";
            PhoneInput.Text = "";
            EmailInput.Text = "";
            ClientSearchInput.Text = "";
            ClientSearchStatusText.Text = "";
            GenderDropdown.SelectedIndex = 0;
            ValidationText.Text = "";
            EmailValidationText.Text = "";
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
            if (ClientCardsPanel == null || ClientDetailsText == null)
                return;

            BackToClientsButton.IsVisible = false;
            ClientCardsPanel.Children.Clear();

            if (AppData.Clients.Count == 0)
            {
                ClientDetailsText.Text = "אין לקוחות במערכת";
                return;
            }

            ClientDetailsText.Text = "לחץ על כרטיס לקוח כדי לראות את כל הפרטים";

            foreach (var client in AppData.Clients.OrderBy(c => c.FullName))
            {
                ClientCardsPanel.Children.Add(CreateClientCard(client));
            }
        }

        private Button CreateClientCard(Client client)
        {
            string accentColor = GetClientAccentColor(client.Gender);
            string strongColor = GetClientStrongColor(client.Gender);
            string icon = GetClientIcon(client.Gender);
            int animalsCount = AppData.Animals.Count(animal => animal.OwnerIdNumber == client.IdNumber);

            var card = new Button
            {
                Width = 230,
                MinHeight = 235,
                Margin = new Thickness(8),
                Padding = new Thickness(0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse(strongColor)),
                BorderThickness = new Thickness(3),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Children =
                    {
                        new Border
                        {
                            Width = 224,
                            Height = 86,
                            CornerRadius = new CornerRadius(14, 14, 26, 26),
                            Background = new SolidColorBrush(Color.Parse(accentColor)),
                            Child = new Grid
                            {
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = icon,
                                        FontSize = 42,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    },
                                    new Border
                                    {
                                        Background = new SolidColorBrush(Color.Parse(strongColor)),
                                        CornerRadius = new CornerRadius(12),
                                        Padding = new Thickness(10, 4),
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        Margin = new Thickness(10),
                                        Child = new TextBlock
                                        {
                                            Text = client.Gender,
                                            FontSize = 12,
                                            FontWeight = FontWeight.Bold,
                                            Foreground = Brushes.White,
                                            TextAlignment = TextAlignment.Center
                                        }
                                    }
                                }
                            }
                        },

                        new StackPanel
                        {
                            Spacing = 8,
                            Margin = new Thickness(14),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = client.FullName,
                                    FontSize = 21,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                                },
                                new TextBlock
                                {
                                    Text = $"ת.ז: {client.IdNumber}",
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new Border
                                {
                                    Background = new SolidColorBrush(Color.Parse("#E9F8FC")),
                                    CornerRadius = new CornerRadius(12),
                                    Padding = new Thickness(10, 5),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Child = new TextBlock
                                    {
                                        Text = $"חיות: {animalsCount}",
                                        FontSize = 13,
                                        FontWeight = FontWeight.Bold,
                                        Foreground = new SolidColorBrush(Color.Parse(strongColor)),
                                        TextAlignment = TextAlignment.Center
                                    }
                                },
                                new TextBlock
                                {
                                    Text = "לחץ לפתיחת לקוח",
                                    FontSize = 12,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse(strongColor))
                                }
                            }
                        }
                    }
                }
            };

            card.Classes.Add("dataCard");
            card.Click += (_, _) => FillClientFields(client);

            return card;
        }

        private void FillClientFields(Client client)
        {
            FullNameInput.Text = client.FullName;
            IdNumberInput.Text = client.IdNumber;
            PhoneInput.Text = client.Phone;
            EmailInput.Text = client.Email;
            SelectGender(client.Gender);
            UpdateEmailValidationText(client.Email);
            ClientDetailsText.Text = BuildClientDetailsText(client);
            SetValidationMessage("כרטיס הלקוח נטען ואפשר לעדכן אותו", isValid: true);
        }

        private string BuildClientDetailsText(Client client)
        {
            int animalsCount = AppData.Animals.Count(animal => animal.OwnerIdNumber == client.IdNumber);

            return $"""
                שם: {client.FullName}
                מין: {client.Gender}
                תעודת זהות: {client.IdNumber}
                טלפון: {client.Phone}
                אימייל: {client.Email}
                מספר חיות רשומות: {animalsCount}
                """;
        }

        private Button CreateOwnedAnimalCard(Animal animal)
        {
            bool vaccinationDue = ValidationService.IsVaccinationDue(animal.LastVaccinationDate);
            string accentColor = GetAnimalAccentColor(animal.Species);
            string strongColor = GetAnimalStrongColor(animal.Species);
            string statusColor = vaccinationDue ? "#D64545" : "#1E8F4D";
            string statusText = vaccinationDue ? "צריך חיסון" : "חיסון תקין";

            var card = new Button
            {
                Width = 230,
                MinHeight = 250,
                Margin = new Thickness(8),
                Padding = new Thickness(0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse(vaccinationDue ? "#D64545" : strongColor)),
                BorderThickness = new Thickness(3),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Children =
                    {
                        new Border
                        {
                            Width = 224,
                            Height = 86,
                            CornerRadius = new CornerRadius(14, 14, 26, 26),
                            Background = new SolidColorBrush(Color.Parse(accentColor)),
                            Child = new Grid
                            {
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = GetAnimalIcon(animal.Species),
                                        FontSize = 44,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    },
                                    new Border
                                    {
                                        Background = new SolidColorBrush(Color.Parse(strongColor)),
                                        CornerRadius = new CornerRadius(12),
                                        Padding = new Thickness(10, 4),
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        Margin = new Thickness(10),
                                        Child = new TextBlock
                                        {
                                            Text = animal.Species,
                                            FontSize = 12,
                                            FontWeight = FontWeight.Bold,
                                            Foreground = Brushes.White,
                                            TextAlignment = TextAlignment.Center
                                        }
                                    }
                                }
                            }
                        },

                        new StackPanel
                        {
                            Spacing = 8,
                            Margin = new Thickness(14),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = animal.Name,
                                    FontSize = 21,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                                },
                                new Border
                                {
                                    Background = new SolidColorBrush(Color.Parse(statusColor)),
                                    CornerRadius = new CornerRadius(12),
                                    Padding = new Thickness(10, 5),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Child = new TextBlock
                                    {
                                        Text = statusText,
                                        FontSize = 13,
                                        FontWeight = FontWeight.Bold,
                                        Foreground = Brushes.White,
                                        TextAlignment = TextAlignment.Center
                                    }
                                },
                                new TextBlock
                                {
                                    Text = $"שבב: {animal.ChipNumber}",
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new TextBlock
                                {
                                    Text = $"משקל: {animal.Weight:0.##} קג",
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new TextBlock
                                {
                                    Text = "לחץ לפרטי חיה",
                                    FontSize = 12,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse(strongColor))
                                }
                            }
                        }
                    }
                }
            };

            card.Classes.Add("dataCard");
            card.Click += (_, _) => ShowOwnedAnimalDetails(animal);

            return card;
        }

        private void ShowOwnedAnimalDetails(Animal animal)
        {
            ClientDetailsText.Text = $"""
                שם החיה: {animal.Name}
                סוג: {animal.Species}
                מספר שבב: {animal.ChipNumber}
                משקל: {animal.Weight:0.##} קג
                תאריך לידה: {animal.BirthDate:dd/MM/yyyy}
                חיסון אחרון: {animal.LastVaccinationDate:dd/MM/yyyy}
                סטטוס חיסון: {(ValidationService.IsVaccinationDue(animal.LastVaccinationDate) ? "צריך חיסון שנתי" : "חיסון תקין")}
                """;
        }

        private string GetAnimalIcon(string species)
        {
            return species switch
            {
                "כלב" or "Dog" => "🐶",
                "חתול" or "Cat" => "🐱",
                "זוחל" or "Reptile" => "🦎",
                "ציפור" or "Bird" => "🐦",
                _ => "🐾"
            };
        }

        private string GetAnimalAccentColor(string species)
        {
            return species switch
            {
                "כלב" or "Dog" => "#E9F8FC",
                "חתול" or "Cat" => "#FFF1D6",
                "זוחל" or "Reptile" => "#E5F6E8",
                "ציפור" or "Bird" => "#E8ECFF",
                _ => "#F1F5F9"
            };
        }

        private string GetAnimalStrongColor(string species)
        {
            return species switch
            {
                "כלב" or "Dog" => "#0797C9",
                "חתול" or "Cat" => "#D9822B",
                "זוחל" or "Reptile" => "#2E9D59",
                "ציפור" or "Bird" => "#5865C7",
                _ => "#476A88"
            };
        }

        private string GetSelectedGender()
        {
            if (GenderDropdown.SelectedItem is ComboBoxItem item && item.Content != null)
                return item.Content.ToString() ?? "זכר";

            return "זכר";
        }

        private void SelectGender(string gender)
        {
            for (int i = 0; i < GenderDropdown.ItemCount; i++)
            {
                if (GenderDropdown.Items[i] is ComboBoxItem item &&
                    item.Content?.ToString() == gender)
                {
                    GenderDropdown.SelectedIndex = i;
                    return;
                }
            }

            GenderDropdown.SelectedIndex = 0;
        }

        private string GetClientIcon(string gender)
        {
            return gender == "נקבה" ? "👩" : "👨";
        }

        private string GetClientAccentColor(string gender)
        {
            return gender == "נקבה" ? "#FDE7F3" : "#E9F8FC";
        }

        private string GetClientStrongColor(string gender)
        {
            return gender == "נקבה" ? "#C7478A" : "#0797C9";
        }
    }
}