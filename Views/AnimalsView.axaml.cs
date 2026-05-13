using System;
using System.Linq;
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
    public partial class AnimalsView : UserControl
    {
        public Action? BackToMainMenu;

        public AnimalsView()
        {
            InitializeComponent();
            RefreshAnimalsList();
        }

        private void ShowMessage(string message)
        {
            UIHelper.ShowMessage(this, message);
        }

        private void AnimalSearch_Changed(object? sender, TextChangedEventArgs e)
        {
            RefreshAnimalsList();
        }

        private void AnimalFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RefreshAnimalsList();
        }

        private void ClearAnimalSearch_Click(object? sender, RoutedEventArgs e)
        {
            AnimalSearchInput.Text = "";
            AnimalFilterDropdown.SelectedIndex = 0;
            RefreshAnimalsList();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(chipNumber) &&
                string.IsNullOrWhiteSpace(weightText) &&
                string.IsNullOrWhiteSpace(ownerId))
            {
                ValidationText.Text = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(name) && !ValidationService.IsValidAnimalName(name))
            {
                SetValidationMessage("שם החיה חייב להכיל אותיות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(chipNumber) && !ValidationService.IsValidChipNumber(chipNumber))
            {
                SetValidationMessage("מספר שבב חייב להכיל ספרות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(weightText))
            {
                if (!double.TryParse(weightText, out double weight))
                {
                    SetValidationMessage("משקל חייב להיות מספר", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidWeight(weight))
                {
                    SetValidationMessage("משקל חייב להיות בין 0.1 ל-100 קג", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(ownerId) && !ValidationService.IsValidIdNumber(ownerId))
            {
                SetValidationMessage("תעודת זהות בעלים חייבת להיות 9 ספרות", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void AddAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";

            string species = "כלב";

            if (SpeciesDropdown.SelectedItem is ComboBoxItem selectedSpecies &&
                selectedSpecies.Content != null)
            {
                species = selectedSpecies.Content.ToString() ?? "כלב";
            }

            if (!double.TryParse(weightText, out double weight))
            {
                ShowMessage("משקל חייב להיות מספר");
                return;
            }

            DateTime birthDate = BirthDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            DateTime vaccinationDate = VaccinationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (!ValidationService.IsValidAnimalName(name))
            {
                ShowMessage("שם החיה חייב להכיל אותיות בלבד");
                return;
            }

            if (!ValidationService.IsValidAnimalSpecies(species))
            {
                ShowMessage("סוג חיה לא תקין");
                return;
            }

            if (!ValidationService.IsValidChipNumber(chipNumber))
            {
                ShowMessage("מספר שבב חייב להכיל ספרות בלבד");
                return;
            }

            if (!ValidationService.IsValidWeight(weight))
            {
                ShowMessage("משקל חייב להיות בין 0.1 ל־100 קג");
                return;
            }

            if (!ValidationService.IsValidBirthDate(birthDate))
            {
                ShowMessage("תאריך לידה לא יכול להיות עתידי או לפני שנת 2000");
                return;
            }

            if (!ValidationService.IsValidIdNumber(ownerId))
            {
                ShowMessage("תעודת זהות בעלים חייבת להיות 9 ספרות");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            bool ownerExists = AppData.Clients.Any(client => client.IdNumber == ownerId);

            if (!ownerExists)
            {
                ShowMessage("לא נמצא לקוח עם תעודת זהות זו. קודם צריך להוסיף לקוח");
                return;
            }

            bool chipExists = AppData.Animals.Any(animal => animal.ChipNumber == chipNumber);

            if (chipExists)
            {
                ShowMessage("מספר שבב כבר קיים במערכת");
                return;
            }

            AppData.Animals.Add(new Animal
            {
                Name = name,
                Species = species,
                ChipNumber = chipNumber,
                Weight = weight,
                BirthDate = birthDate,
                OwnerIdNumber = ownerId,
                LastVaccinationDate = vaccinationDate
            });

            AppData.SaveAnimalsToDatabase();

            ShowMessage("החיה נוספה בהצלחה");
            ClearFields();
            RefreshAnimalsList();
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a =>
                a.Name == name || a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה");
                return;
            }

            NameInput.Text = animal.Name;
            ChipNumberInput.Text = animal.ChipNumber;
            WeightInput.Text = animal.Weight.ToString();
            OwnerIdInput.Text = animal.OwnerIdNumber;
            BirthDatePicker.SelectedDate = animal.BirthDate;
            VaccinationDatePicker.SelectedDate = animal.LastVaccinationDate;

            for (int i = 0; i < SpeciesDropdown.ItemCount; i++)
            {
                if (SpeciesDropdown.Items[i] is ComboBoxItem item &&
                    item.Content?.ToString() == animal.Species)
                {
                    SpeciesDropdown.SelectedIndex = i;
                    break;
                }
            }
        }

        private void DeleteAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה למחיקה");
                return;
            }

            AppData.Animals.Remove(animal);
            AppData.SaveAnimalsToDatabase();

            ShowMessage("החיה נמחקה בהצלחה");
            ClearFields();
            RefreshAnimalsList();
        }

        private void UpdateAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה לעדכון לפי מספר השבב");
                return;
            }

            string species = "כלב";

            if (SpeciesDropdown.SelectedItem is ComboBoxItem selectedSpecies &&
                selectedSpecies.Content != null)
            {
                species = selectedSpecies.Content.ToString() ?? "כלב";
            }

            if (!double.TryParse(weightText, out double weight))
            {
                ShowMessage("משקל חייב להיות מספר");
                return;
            }

            DateTime birthDate = BirthDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            DateTime vaccinationDate = VaccinationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (!ValidationService.IsValidAnimalName(name))
            {
                ShowMessage("שם החיה חייב להכיל אותיות בלבד");
                return;
            }

            if (!ValidationService.IsValidAnimalSpecies(species))
            {
                ShowMessage("סוג חיה לא תקין");
                return;
            }

            if (!ValidationService.IsValidWeight(weight))
            {
                ShowMessage("משקל חייב להיות בין 0.1 ל־100 קג");
                return;
            }

            if (!ValidationService.IsValidBirthDate(birthDate))
            {
                ShowMessage("תאריך לידה לא יכול להיות עתידי או לפני שנת 2000");
                return;
            }

            if (!ValidationService.IsValidIdNumber(ownerId))
            {
                ShowMessage("תעודת זהות בעלים חייבת להיות 9 ספרות");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            bool ownerExists = AppData.Clients.Any(client => client.IdNumber == ownerId);

            if (!ownerExists)
            {
                ShowMessage("לא נמצא לקוח עם תעודת זהות זו");
                return;
            }

            animal.Name = name;
            animal.Species = species;
            animal.Weight = weight;
            animal.BirthDate = birthDate;
            animal.OwnerIdNumber = ownerId;
            animal.LastVaccinationDate = vaccinationDate;

            AppData.SaveAnimalsToDatabase();

            ShowMessage("פרטי החיה עודכנו בהצלחה");
            RefreshAnimalsList();
        }

        private void ClearFields_Click(object? sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }

        private void ClearFields()
        {
            NameInput.Text = "";
            ChipNumberInput.Text = "";
            WeightInput.Text = "";
            OwnerIdInput.Text = "";
            SpeciesDropdown.SelectedIndex = 0;
            BirthDatePicker.SelectedDate = DateTime.Today;
            VaccinationDatePicker.SelectedDate = DateTime.Today;
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void RefreshAnimalsList()
        {
            if (AnimalCardsPanel == null || AnimalResultsText == null)
                return;

            AnimalCardsPanel.Children.Clear();

            var filteredAnimals = AppData.Animals
                .Where(MatchesSearchAndFilter)
                .OrderBy(a => a.Name)
                .ToList();

            AnimalResultsText.Text =
                $"מציג {filteredAnimals.Count} מתוך {AppData.Animals.Count} בעלי חיים";

            if (AppData.Animals.Count == 0)
            {
                AnimalCardsPanel.Children.Add(new TextBlock
                {
                    Text = "אין בעלי חיים במערכת",
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Avalonia.Thickness(20)
                });
                return;
            }

            if (filteredAnimals.Count == 0)
            {
                AnimalCardsPanel.Children.Add(new TextBlock
                {
                    Text = "לא נמצאו בעלי חיים שמתאימים לחיפוש",
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Avalonia.Thickness(20)
                });
                return;
            }

            foreach (var animal in filteredAnimals)
            {
                AnimalCardsPanel.Children.Add(CreateAnimalCard(animal));
            }
        }

        private bool MatchesSearchAndFilter(Animal animal)
        {
            string searchText = AnimalSearchInput?.Text?.Trim() ?? "";
            string filter = GetSelectedAnimalFilter();

            bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                                 animal.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                 animal.ChipNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            bool matchesFilter = filter switch
            {
                "כלב" => animal.Species == "כלב" || animal.Species == "Dog",
                "חתול" => animal.Species == "חתול" || animal.Species == "Cat",
                "זוחל" => animal.Species == "זוחל" || animal.Species == "Reptile",
                "ציפור" => animal.Species == "ציפור" || animal.Species == "Bird",
                "צריך חיסון" => ValidationService.IsVaccinationDue(animal.LastVaccinationDate),
                _ => true
            };

            return matchesSearch && matchesFilter;
        }

        private string GetSelectedAnimalFilter()
        {
            if (AnimalFilterDropdown?.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content != null)
            {
                return selectedItem.Content.ToString() ?? "הכל";
            }

            return "הכל";
        }

        private Button CreateAnimalCard(Animal animal)
        {
            var owner = AppData.Clients.FirstOrDefault(c => c.IdNumber == animal.OwnerIdNumber);
            bool vaccinationDue = ValidationService.IsVaccinationDue(animal.LastVaccinationDate);

            var card = new Button
            {
                Width = 190,
                MinHeight = 230,
                Margin = new Avalonia.Thickness(8),
                Padding = new Avalonia.Thickness(12),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse(vaccinationDue ? "#E57373" : "#69C5D8")),
                BorderThickness = new Avalonia.Thickness(2),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Spacing = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new Border
                        {
                            Width = 72,
                            Height = 72,
                            CornerRadius = new Avalonia.CornerRadius(36),
                            Background = new SolidColorBrush(Color.Parse(GetAnimalAccentColor(animal.Species))),
                            Child = new TextBlock
                            {
                                Text = GetAnimalIcon(animal.Species),
                                FontSize = 36,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center
                            }
                        },
                        new TextBlock
                        {
                            Text = animal.Name,
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                        },
                        new TextBlock
                        {
                            Text = $"סוג: {animal.Species}",
                            FontSize = 14,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Color.Parse("#526172"))
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
                            Text = $"בעלים: {(owner != null ? owner.FullName : animal.OwnerIdNumber)}",
                            FontSize = 13,
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.Parse("#526172"))
                        },
                        new TextBlock
                        {
                            Text = vaccinationDue ? "צריך חיסון שנתי" : "חיסון תקין",
                            FontSize = 13,
                            FontWeight = FontWeight.Bold,
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = vaccinationDue ? Brushes.Firebrick : Brushes.ForestGreen
                        }
                    }
                }
            };

            ToolTip.SetTip(card, "לחץ כדי לראות ולעדכן את כרטיס החיה");

            card.Click += (_, _) => FillAnimalFields(animal);

            return card;
        }

        private void FillAnimalFields(Animal animal)
        {
            NameInput.Text = animal.Name;
            ChipNumberInput.Text = animal.ChipNumber;
            WeightInput.Text = animal.Weight.ToString();
            OwnerIdInput.Text = animal.OwnerIdNumber;
            BirthDatePicker.SelectedDate = animal.BirthDate;
            VaccinationDatePicker.SelectedDate = animal.LastVaccinationDate;

            for (int i = 0; i < SpeciesDropdown.ItemCount; i++)
            {
                if (SpeciesDropdown.Items[i] is ComboBoxItem item &&
                    item.Content?.ToString() == animal.Species)
                {
                    SpeciesDropdown.SelectedIndex = i;
                    break;
                }
            }

            SetValidationMessage("כרטיס החיה נטען ואפשר לעדכן אותו", isValid: true);
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
    }
}