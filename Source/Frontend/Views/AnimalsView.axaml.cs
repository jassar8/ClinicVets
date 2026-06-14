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
        private bool isAddingNewAnimal;

        public AnimalsView()
        {
            InitializeComponent();
            PopulateOwnerDropdown();
            RefreshAnimalsList();
        }

        private void PopulateOwnerDropdown()
        {
            OwnerClientDropdown.Items.Clear();
            OwnerClientDropdown.Items.Add(new ComboBoxItem
            {
                Content = "בחר לקוח לפי שם",
                Tag = ""
            });

            foreach (var client in AppData.Clients.OrderBy(c => c.FullName))
            {
                OwnerClientDropdown.Items.Add(new ComboBoxItem
                {
                    Content = client.FullName,
                    Tag = client.IdNumber
                });
            }

            OwnerClientDropdown.SelectedIndex = 0;
        }

        private string? GetSelectedOwnerIdNumber()
        {
            if (OwnerClientDropdown.SelectedItem is ComboBoxItem item &&
                item.Tag is string idNumber &&
                !string.IsNullOrWhiteSpace(idNumber))
            {
                return idNumber;
            }

            return null;
        }

        private void SelectOwnerByIdNumber(string idNumber)
        {
            for (int i = 0; i < OwnerClientDropdown.ItemCount; i++)
            {
                if (OwnerClientDropdown.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Tag?.ToString(), idNumber, StringComparison.Ordinal))
                {
                    OwnerClientDropdown.SelectedIndex = i;
                    return;
                }
            }

            OwnerClientDropdown.SelectedIndex = 0;
        }

        private void BirthDatePicker_SelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
        {
            UpdateBirthDateValidationText();
            ValidateFormFields();
        }

        private void OwnerClientDropdown_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ValidateFormFields();
        }

        private void UpdateBirthDateValidationText()
        {
            DateTime birthDate = BirthDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            BirthDateValidationText.Text = ValidationService.GetBirthDateValidationMessage(birthDate) ?? "";
        }

        private bool ValidateBirthDateBeforeSave(DateTime birthDate)
        {
            string? message = ValidationService.GetBirthDateValidationMessage(birthDate);
            if (message == null)
            {
                BirthDateValidationText.Text = "";
                return true;
            }

            BirthDateValidationText.Text = message;
            SetValidationMessage(message, isValid: false);
            return false;
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

        private void StartAddAnimal_Click(object? sender, RoutedEventArgs e)
        {
            PopulateOwnerDropdown();
            ClearFields();
            isAddingNewAnimal = true;
            ShowAnimalForm(
                "הוספת חיה חדשה",
                "מלא את פרטי החיה ואז לחץ שמור חיה");
            UpdateAnimalActionMode();
        }

        private void CloseAnimalForm_Click(object? sender, RoutedEventArgs e)
        {
            HideAnimalForm();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            ValidateFormFields();
        }

        private void ValidateFormFields()
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = GetFullChipNumberFromInput();
            string weightText = WeightInput.Text?.Trim() ?? "";
            string? ownerId = GetSelectedOwnerIdNumber();

            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(chipNumber) &&
                string.IsNullOrWhiteSpace(weightText) &&
                ownerId == null)
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
                SetValidationMessage("אחרי 376 יש להזין בדיוק 4 ספרות", isValid: false);
                return;
            }

            if (isAddingNewAnimal &&
                !string.IsNullOrWhiteSpace(chipNumber) &&
                AppData.Animals.Any(animal => animal.ChipNumber == chipNumber))
            {
                SetValidationMessage("מספר השבב כבר קיים במערכת. יש להזין שבב אחר.", isValid: false);
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

            UpdateBirthDateValidationText();
            if (!string.IsNullOrWhiteSpace(BirthDateValidationText.Text))
                return;

            if (ownerId == null && OwnerClientDropdown.SelectedIndex > 0)
            {
                SetValidationMessage("יש לבחור בעלים מהרשימה", isValid: false);
                return;
            }

            if (ownerId == null &&
                (!string.IsNullOrWhiteSpace(name) ||
                 !string.IsNullOrWhiteSpace(chipNumber) ||
                 !string.IsNullOrWhiteSpace(weightText)))
            {
                SetValidationMessage("יש לבחור בעלים מהרשימה", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void AddAnimal_Click(object? sender, RoutedEventArgs e)
        {
            if (!isAddingNewAnimal)
            {
                UpdateAnimal();
                return;
            }

            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = GetFullChipNumberFromInput();
            string? ownerId = GetSelectedOwnerIdNumber();
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
                ShowMessage("אחרי 376 יש להזין בדיוק 4 ספרות");
                return;
            }

            if (!ValidationService.IsValidWeight(weight))
            {
                ShowMessage("משקל חייב להיות בין 0.1 ל־100 קג");
                return;
            }

            if (!ValidateBirthDateBeforeSave(birthDate))
            {
                ShowMessage(BirthDateValidationText.Text ?? "תאריך לידה לא תקין");
                return;
            }

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                SetValidationMessage("יש לבחור בעלים מהרשימה", isValid: false);
                ShowMessage("יש לבחור בעלים מהרשימה");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            if (!ValidationService.IsValidVaccinationDateForBirthDate(vaccinationDate, birthDate))
            {
                ShowMessage("תאריך חיסון אחרון לא יכול להיות לפני תאריך הלידה של החיה");
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
                ShowMessage("מספר השבב כבר קיים במערכת. אי אפשר לשמור שתי חיות עם אותו שבב.");
                return;
            }

            var newAnimal = new Animal
            {
                Name = name,
                Species = species,
                ChipNumber = chipNumber,
                Weight = weight,
                BirthDate = birthDate,
                OwnerIdNumber = ownerId!,
                LastVaccinationDate = vaccinationDate
            };

            AppData.Animals.Add(newAnimal);

            try
            {
                AppData.SaveAnimalsToDatabase();
            }
            catch (Exception ex)
            {
                AppData.Animals.Remove(newAnimal);
                ShowMessage($"שמירת החיה נכשלה ולא נסגרה האפליקציה. פרטי השגיאה: {ex.Message}");
                return;
            }

            ShowMessage("החיה נוספה בהצלחה");
            ClearFields();
            HideAnimalForm();
            RefreshAnimalsList();
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = GetFullChipNumberFromInput();

            var animal = AppData.Animals.FirstOrDefault(a =>
                a.Name == name || a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה");
                return;
            }

            isAddingNewAnimal = false;
            NameInput.Text = animal.Name;
            SetChipNumberInput(animal.ChipNumber);
            WeightInput.Text = animal.Weight.ToString();
            PopulateOwnerDropdown();
            SelectOwnerByIdNumber(animal.OwnerIdNumber);
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

            ShowAnimalForm(
                "עריכת חיה קיימת",
                "הנתונים נטענו. אפשר לעדכן או למחוק לפי הצורך");
            isAddingNewAnimal = false;
            UpdateAnimalActionMode();
            UpdateAnimalVetNote(animal.ChipNumber);
            SetValidationMessage("כרטיס החיה נטען ואפשר לעדכן אותו", isValid: true);
        }

        private void DeleteAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string chipNumber = GetFullChipNumberFromInput();

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה למחיקה");
                return;
            }

            bool hasVisits = AppData.Visits.Any(visit => visit.AnimalChipNumber == chipNumber);

            if (hasVisits)
            {
                ShowMessage("לא ניתן למחוק חיה שיש לה ביקורים שמורים. אפשר לשמור את ההיסטוריה ולערוך את פרטי החיה במקום למחוק.");
                return;
            }

            try
            {
                AppData.Animals.Remove(animal);
                AppData.SaveAnimalsToDatabase();
            }
            catch (Exception)
            {
                if (!AppData.Animals.Any(a => a.ChipNumber == animal.ChipNumber))
                    AppData.Animals.Add(animal);

                ShowMessage("לא ניתן למחוק את החיה כרגע. נסה שוב או בדוק שאין נתונים קשורים לחיה.");
                RefreshAnimalsList();
                return;
            }

            ShowMessage("החיה נמחקה בהצלחה");
            ClearFields();
            HideAnimalForm();
            RefreshAnimalsList();
        }

        private void UpdateAnimal_Click(object? sender, RoutedEventArgs e)
        {
            UpdateAnimal();
        }

        private void UpdateAnimal()
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = GetFullChipNumberFromInput();
            string? ownerId = GetSelectedOwnerIdNumber();
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

            if (!ValidateBirthDateBeforeSave(birthDate))
            {
                ShowMessage(BirthDateValidationText.Text ?? "תאריך לידה לא תקין");
                return;
            }

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                SetValidationMessage("יש לבחור בעלים מהרשימה", isValid: false);
                ShowMessage("יש לבחור בעלים מהרשימה");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            if (!ValidationService.IsValidVaccinationDateForBirthDate(vaccinationDate, birthDate))
            {
                ShowMessage("תאריך חיסון אחרון לא יכול להיות לפני תאריך הלידה של החיה");
                return;
            }

            bool ownerExists = AppData.Clients.Any(client => client.IdNumber == ownerId);

            if (!ownerExists)
            {
                ShowMessage("לא נמצא לקוח עם תעודת זהות זו");
                return;
            }

            string previousName = animal.Name;
            string previousSpecies = animal.Species;
            double previousWeight = animal.Weight;
            DateTime previousBirthDate = animal.BirthDate;
            string previousOwnerId = animal.OwnerIdNumber;
            DateTime previousVaccinationDate = animal.LastVaccinationDate;

            animal.Name = name;
            animal.Species = species;
            animal.Weight = weight;
            animal.BirthDate = birthDate;
            animal.OwnerIdNumber = ownerId!;
            animal.LastVaccinationDate = vaccinationDate;

            try
            {
                AppData.SaveAnimalsToDatabase();
            }
            catch (Exception ex)
            {
                animal.Name = previousName;
                animal.Species = previousSpecies;
                animal.Weight = previousWeight;
                animal.BirthDate = previousBirthDate;
                animal.OwnerIdNumber = previousOwnerId;
                animal.LastVaccinationDate = previousVaccinationDate;

                ShowMessage($"עדכון החיה נכשל ולא נסגרה האפליקציה. פרטי השגיאה: {ex.Message}");
                RefreshAnimalsList();
                return;
            }

            ShowMessage("פרטי החיה עודכנו בהצלחה");
            ShowAnimalForm(
                "עריכת חיה קיימת",
                "הנתונים עודכנו. אפשר להמשיך לערוך או לסגור את הפרטים");
            isAddingNewAnimal = false;
            UpdateAnimalActionMode();
            RefreshAnimalsList();
        }

        private void ClearFields_Click(object? sender, RoutedEventArgs e)
        {
            ClearFields();
            isAddingNewAnimal = true;
            UpdateAnimalActionMode();
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
            SpeciesDropdown.SelectedIndex = 0;
            OwnerClientDropdown.SelectedIndex = 0;
            BirthDatePicker.SelectedDate = DateTime.Today;
            VaccinationDatePicker.SelectedDate = DateTime.Today;
            ValidationText.Text = "";
            BirthDateValidationText.Text = "";
            HideAnimalVetNote();
        }

        private void UpdateAnimalVetNote(string chipNumber)
        {
            if (AnimalVetNotePanel == null || AnimalVetNoteText == null)
                return;

            AnimalVetNoteText.Text = AnimalNoteService.GetLatestVetNoteOrPlaceholder(chipNumber);
            AnimalVetNotePanel.IsVisible = true;
        }

        private void HideAnimalVetNote()
        {
            if (AnimalVetNotePanel == null || AnimalVetNoteText == null)
                return;

            AnimalVetNoteText.Text = AnimalNoteService.NoNoteText;
            AnimalVetNotePanel.IsVisible = false;
        }

        private string GetFullChipNumberFromInput()
        {
            string chipSuffix = ChipNumberInput.Text?.Trim() ?? "";

            if (chipSuffix.StartsWith("376", StringComparison.Ordinal) &&
                chipSuffix.Length == 7)
            {
                return chipSuffix;
            }

            return $"376{chipSuffix}";
        }

        private void SetChipNumberInput(string fullChipNumber)
        {
            ChipNumberInput.Text = fullChipNumber.StartsWith("376", StringComparison.Ordinal) &&
                fullChipNumber.Length == 7
                    ? fullChipNumber.Substring(3)
                    : fullChipNumber;
        }

        private void ShowAnimalForm(string title, string hint)
        {
            AnimalStartPanel.IsVisible = false;
            AnimalFormPanel.IsVisible = true;
            AnimalFormTitle.Text = title;
            AnimalFormHint.Text = hint;
        }

        private void HideAnimalForm()
        {
            isAddingNewAnimal = false;
            AnimalFormPanel.IsVisible = false;
            AnimalStartPanel.IsVisible = true;
            ValidationText.Text = "";
            UpdateAnimalActionMode();
        }

        private void UpdateAnimalActionMode()
        {
            if (SaveAnimalButton == null || DeleteAnimalButton == null)
                return;

            SaveAnimalButton.Content = isAddingNewAnimal ? "שמור חיה" : "עדכן חיה";
            ToolTip.SetTip(
                SaveAnimalButton,
                isAddingNewAnimal ? "שומר חיה חדשה" : "מעדכן את כרטיס החיה שנבחר");

            DeleteAnimalButton.IsVisible = !isAddingNewAnimal;
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
            string accentColor = GetAnimalAccentColor(animal.Species);
            string strongColor = GetAnimalStrongColor(animal.Species);
            string statusColor = vaccinationDue ? "#D64545" : "#1E8F4D";
            string statusText = vaccinationDue ? "צריך חיסון" : "חיסון תקין";

            var card = new Button
            {
                Width = 230,
                MinHeight = 270,
                Margin = new Avalonia.Thickness(8),
                Padding = new Avalonia.Thickness(0),
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                BorderBrush = new SolidColorBrush(Color.Parse(vaccinationDue ? "#D64545" : strongColor)),
                BorderThickness = new Avalonia.Thickness(3),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Children =
                    {
                        new Border
                        {
                            Height = 94,
                            Background = new SolidColorBrush(Color.Parse(accentColor)),
                            CornerRadius = new Avalonia.CornerRadius(14, 14, 28, 28),
                            Child = new Grid
                            {
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = GetAnimalIcon(animal.Species),
                                        FontSize = 48,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    },
                                    new Border
                                    {
                                        Background = new SolidColorBrush(Color.Parse(strongColor)),
                                        CornerRadius = new Avalonia.CornerRadius(12),
                                        Padding = new Avalonia.Thickness(10, 4),
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        Margin = new Avalonia.Thickness(10),
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
                            Margin = new Avalonia.Thickness(14),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = animal.Name,
                                    FontSize = 22,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                                },
                                new Border
                                {
                                    Background = new SolidColorBrush(Color.Parse(statusColor)),
                                    CornerRadius = new Avalonia.CornerRadius(12),
                                    Padding = new Avalonia.Thickness(10, 5),
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
                                    Text = $"בעלים: {(owner != null ? owner.FullName : animal.OwnerIdNumber)}",
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
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
                                    Text = $"הערת וטרינר: {AnimalNoteService.GetLatestVetNoteOrPlaceholder(animal.ChipNumber)}",
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new TextBlock
                                {
                                    Text = "לחץ לפתיחת כרטיס",
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

            ToolTip.SetTip(card, "לחץ כדי לראות ולעדכן את כרטיס החיה");

            card.Click += (_, _) => FillAnimalFields(animal);

            return card;
        }

        private void FillAnimalFields(Animal animal)
        {
            isAddingNewAnimal = false;
            PopulateOwnerDropdown();
            ShowAnimalForm(
                "עריכת חיה קיימת",
                "הנתונים נטענו מהכרטיס. אפשר לעדכן או למחוק");
            UpdateAnimalActionMode();

            NameInput.Text = animal.Name;
            SetChipNumberInput(animal.ChipNumber);
            WeightInput.Text = animal.Weight.ToString();
            SelectOwnerByIdNumber(animal.OwnerIdNumber);
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

            UpdateAnimalVetNote(animal.ChipNumber);
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
    }
}