using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class VisitsView : UserControl
    {
        public Action? BackToMainMenu;
        private Visit? selectedVisit;
        private string selectedAnimalChipNumber = "";
        private string selectedSpeciesFilter = "";
        private bool showUpcomingVisitsOnly;
        private readonly DispatcherTimer clockTimer = new DispatcherTimer();
        private readonly List<VisitTreatmentLine> pendingTreatmentLines = new();
        private bool isViewReady;

        public VisitsView()
        {
            InitializeComponent();
            isViewReady = true;
            PopulateTimeDropdowns();
            VisitDatePicker.SelectedDate = DateTime.Today;
            SetVisitTime(DateTime.Now.TimeOfDay);
            UpdateMedicationFieldsVisibility(clearFields: false);
            WatchVisitDateChanges();
            StartClock();
            RefreshMedicationDropdown();
            RefreshTreatmentLinesPanel();
            RefreshVisitsList();
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
        }

        private void WatchVisitDateChanges()
        {
            VisitDatePicker.PropertyChanged += (_, e) =>
            {
                if (e.Property == DatePicker.SelectedDateProperty)
                    ValidateVisitInputs();
            };
        }

        private void PopulateTimeDropdowns()
        {
            VisitHourDropdown.ItemsSource = Enumerable.Range(0, 24)
                .Select(hour => hour.ToString("00"))
                .ToList();

            VisitMinuteDropdown.ItemsSource = Enumerable.Range(0, 60)
                .Select(minute => minute.ToString("00"))
                .ToList();
        }

        private void VisitTimeDropdown_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (VisitHourDropdown?.SelectedItem == null || VisitMinuteDropdown?.SelectedItem == null)
                return;

            ValidateVisitInputs();
        }

        private void MedicationDropdown_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateMedicationStockHint();
            ValidateVisitInputs();
        }

        private void TreatmentMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (!isViewReady)
                return;

            UpdateMedicationFieldsVisibility(clearFields: true);
            ValidateVisitInputs();
        }

        private void UpdateMedicationFieldsVisibility(bool clearFields)
        {
            bool withMedication = IsMedicationTreatmentSelected();

            MedicationFieldsPanel.IsVisible = withMedication;

            if (withMedication)
            {
                if (MedicationDropdown.SelectedIndex < 0 && MedicationDropdown.ItemCount > 0)
                    MedicationDropdown.SelectedIndex = 0;

                UpdateMedicationStockHint();
                return;
            }

            if (!clearFields)
                return;

            MedicationDropdown.SelectedIndex = -1;
            MedicationQuantityInput.Text = "";
            UpdateMedicationStockHint();
        }

        private bool IsMedicationTreatmentSelected()
        {
            return TreatmentModeDropdown?.SelectedIndex == 1;
        }

        private void StartClock()
        {
            UpdateClock();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += (_, _) =>
            {
                UpdateClock();
                RefreshVisitTimeWarning();
            };
            clockTimer.Start();
        }

        private void UpdateClock()
        {
            VisitClockText.Text = DateTime.Now.ToString("dddd dd/MM/yyyy HH:mm:ss");
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            UpdateMedicationStockHint();
            ValidateVisitInputs();
        }

        private void ValidateVisitInputs()
        {
            string chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            string reason = ReasonInput.Text?.Trim() ?? "";
            string diagnosis = DiagnosisInput.Text?.Trim() ?? "";
            string baseCostText = BaseCostInput.Text?.Trim() ?? "";
            string medicationQuantityText = IsMedicationTreatmentSelected()
                ? MedicationQuantityInput.Text?.Trim() ?? ""
                : "";

            if (string.IsNullOrWhiteSpace(chipNumber) &&
                string.IsNullOrWhiteSpace(reason) &&
                string.IsNullOrWhiteSpace(diagnosis) &&
                string.IsNullOrWhiteSpace(baseCostText) &&
                string.IsNullOrWhiteSpace(medicationQuantityText))
            {
                ValidationText.Text = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(chipNumber) && !ValidationService.IsValidChipNumber(chipNumber))
            {
                SetValidationMessage("מספר שבב חייב להיות 7 ספרות ולהתחיל ב-376", isValid: false);
                return;
            }

            if (IsSelectedVisitDateTimeInPast(out _))
            {
                SetValidationMessage("לא ניתן לשמור ביקור בתאריך או שעה שכבר עברו", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(baseCostText))
            {
                if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
                {
                    SetValidationMessage("מחיר ביקור חייב להיות מספר", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidMoney(baseCost))
                {
                    SetValidationMessage("מחיר ביקור לא יכול להיות שלילי", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(medicationQuantityText))
            {
                if (!int.TryParse(medicationQuantityText, out int medicationQuantity))
                {
                    SetValidationMessage("כמות תרופה חייבת להיות מספר שלם", isValid: false);
                    return;
                }

                if (medicationQuantity < 0)
                {
                    SetValidationMessage("כמות תרופה לא יכולה להיות שלילית", isValid: false);
                    return;
                }

                string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";
                var medication = AppData.Medications.FirstOrDefault(m => m.Name == medicationName);

                if (medication != null && medicationQuantity > GetAvailableMedicationStock(medication))
                {
                    SetValidationMessage("אין מספיק מלאי לכמות התרופה שהוזנה", isValid: false);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                SetValidationMessage("יש למלא סיבת ביקור", isValid: false);
                return;
            }

            if (pendingTreatmentLines.Count == 0)
            {
                SetValidationMessage("יש להוסיף לפחות טיפול / קורס אחד", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string search = AnimalSearchInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(search))
                search = AnimalChipInput.Text?.Trim() ?? "";

            string normalizedSearchChip = NormalizeChipNumber(search);

            var animal = AppData.Animals.FirstOrDefault(a =>
                NormalizeChipNumber(a.ChipNumber) == normalizedSearchChip ||
                a.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (animal == null)
            {
                SelectedAnimalPanel.IsVisible = false;
                UIHelper.ShowMessage(this, "לא נמצאה חיה לפי שם או מספר שבב");
                return;
            }

            AnimalChipInput.Text = animal.ChipNumber;
            AnimalSearchInput.Text = animal.Name;
            ShowSelectedAnimalCard(animal);
            RefreshVisitsList();

            if (ValidationService.IsVaccinationDue(animal.LastVaccinationDate))
            {
                ShowInlineWarning("תזכורת: החיה צריכה חיסון שנתי, אפשר להמשיך ולשמור ביקור");
                return;
            }

            SetValidationMessage("החיה נמצאה ואפשר להמשיך בביקור", isValid: true);
        }

        private string NormalizeChipNumber(string value)
        {
            return new string((value ?? "").Where(char.IsDigit).ToArray());
        }

        private void CalculateCost_Click(object? sender, RoutedEventArgs e)
        {
            if (TryCalculateTotalCost(out double totalCost))
                TotalCostText.Text = $"עלות כוללת: {totalCost:0.00}";
        }

        private void AddTreatmentLine_Click(object? sender, RoutedEventArgs e)
        {
            if (!TryBuildTreatmentLineFromForm(out VisitTreatmentLine line))
                return;

            pendingTreatmentLines.Add(line);
            ClearTreatmentLineInputs();
            RefreshTreatmentLinesPanel();
            UpdateTotalCostDisplay();
            SetValidationMessage("הטיפול נוסף לרשימה", isValid: true);
        }

        private bool TryBuildTreatmentLineFromForm(out VisitTreatmentLine line)
        {
            line = new VisitTreatmentLine();
            string description = TreatmentLineDescriptionInput.Text?.Trim() ?? "";

            if (!ValidationService.IsRequiredText(description))
            {
                UIHelper.ShowMessage(this, "יש להזין תיאור לטיפול / קורס");
                return false;
            }

            line.Description = description;

            if (!IsMedicationTreatmentSelected())
                return true;

            string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(medicationName))
            {
                UIHelper.ShowMessage(this, "בחר תרופה או שנה את סוג הטיפול לבלי תרופה");
                return false;
            }

            if (!int.TryParse(MedicationQuantityInput.Text?.Trim(), out int quantity) || quantity <= 0)
            {
                UIHelper.ShowMessage(this, "כמות תרופה חייבת להיות מספר גדול מאפס");
                return false;
            }

            var medication = AppData.Medications.FirstOrDefault(m => m.Name == medicationName);

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "התרופה שנבחרה לא קיימת במלאי");
                return false;
            }

            int reservedInPending = pendingTreatmentLines
                .Where(existing => existing.MedicationName == medicationName)
                .Sum(existing => existing.MedicationQuantity);

            int reservedInVisit = selectedVisit?.TreatmentLines
                .Where(existing => existing.MedicationName == medicationName)
                .Sum(existing => existing.MedicationQuantity) ?? 0;

            int availableStock = medication.StockQuantity + reservedInVisit - reservedInPending;

            if (quantity > availableStock)
            {
                UIHelper.ShowMessage(this, $"אין מספיק מלאי לתרופה {medicationName}. זמין: {availableStock}");
                return false;
            }

            line.MedicationName = medicationName;
            line.MedicationQuantity = quantity;
            line.LineCost = medication.UnitPrice * quantity;
            return true;
        }

        private void ClearTreatmentLineInputs()
        {
            TreatmentLineDescriptionInput.Text = "";
            TreatmentModeDropdown.SelectedIndex = 0;
            MedicationQuantityInput.Text = "";
            UpdateMedicationFieldsVisibility(clearFields: true);
        }

        private void RefreshTreatmentLinesPanel()
        {
            TreatmentLinesPanel.Children.Clear();

            if (pendingTreatmentLines.Count == 0)
            {
                TreatmentLinesPanel.Children.Add(new TextBlock
                {
                    Text = "עדיין לא נוספו טיפולים לביקור",
                    FontSize = 13,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                return;
            }

            for (int index = 0; index < pendingTreatmentLines.Count; index++)
            {
                var line = pendingTreatmentLines[index];
                string medicationText = string.IsNullOrWhiteSpace(line.MedicationName)
                    ? "ללא תרופה"
                    : $"{line.MedicationName} x {line.MedicationQuantity} ({line.LineCost:0.00})";

                var removeButton = new Button
                {
                    Content = "הסר",
                    Width = 60,
                    Height = 30,
                    Background = Brushes.Firebrick,
                    BorderBrush = Brushes.Firebrick,
                    Tag = index
                };
                removeButton.Click += RemoveTreatmentLine_Click;

                TreatmentLinesPanel.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#F8FCFD")),
                    BorderBrush = new SolidColorBrush(Color.Parse("#D3EEF4")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(10),
                    Child = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                        Children =
                        {
                            new StackPanel
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = line.Description,
                                        FontWeight = FontWeight.Bold,
                                        TextWrapping = TextWrapping.Wrap
                                    },
                                    new TextBlock
                                    {
                                        Text = medicationText,
                                        FontSize = 12,
                                        Foreground = new SolidColorBrush(Color.Parse("#526172")),
                                        TextWrapping = TextWrapping.Wrap
                                    }
                                }
                            },
                            removeButton
                        }
                    }
                });

                Grid.SetColumn(removeButton, 1);
            }
        }

        private void RemoveTreatmentLine_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int index)
                return;

            if (index < 0 || index >= pendingTreatmentLines.Count)
                return;

            pendingTreatmentLines.RemoveAt(index);
            RefreshTreatmentLinesPanel();
            UpdateTotalCostDisplay();
        }

        private void UpdateTotalCostDisplay()
        {
            if (TryCalculateTotalCost(out double totalCost))
                TotalCostText.Text = $"עלות כוללת: {totalCost:0.00}";
            else
                TotalCostText.Text = "עלות כוללת: 0";
        }

        private List<VisitTreatmentLine> CloneTreatmentLines(IEnumerable<VisitTreatmentLine> lines)
        {
            return lines.Select(line => new VisitTreatmentLine
            {
                Id = line.Id,
                VisitId = line.VisitId,
                Description = line.Description,
                MedicationName = line.MedicationName,
                MedicationQuantity = line.MedicationQuantity,
                LineCost = line.LineCost
            }).ToList();
        }

        private void LoadPendingTreatmentLinesFromVisit(Visit visit)
        {
            pendingTreatmentLines.Clear();
            pendingTreatmentLines.AddRange(CloneTreatmentLines(visit.TreatmentLines));
            RefreshTreatmentLinesPanel();
            UpdateTotalCostDisplay();
        }

        private void ClearPendingTreatmentLines()
        {
            pendingTreatmentLines.Clear();
            RefreshTreatmentLinesPanel();
        }

        private void SaveVisit_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit != null)
            {
                UpdateSelectedVisit();
                return;
            }

            string chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            string reason = ReasonInput.Text?.Trim() ?? "";
            string diagnosis = DiagnosisInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "יש לבחור חיה קיימת לפני שמירת ביקור");
                return;
            }

            if (!TryGetVisitDate(out DateTime visitDate))
                return;

            if (!ValidationService.IsValidVisitDate(visitDate))
            {
                UIHelper.ShowMessage(this, "תאריך ביקור לא תקין");
                return;
            }

            if (IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "לא ניתן לשמור ביקור בתאריך או שעה שכבר עברו");
                SetValidationMessage("בחר תאריך ושעה עתידיים או את הדקה הנוכחית", isValid: false);
                return;
            }

            if (!ValidationService.IsRequiredText(reason))
            {
                UIHelper.ShowMessage(this, "יש למלא סיבת ביקור");
                return;
            }

            if (pendingTreatmentLines.Count == 0)
            {
                UIHelper.ShowMessage(this, "יש להוסיף לפחות טיפול / קורס אחד לביקור");
                return;
            }

            if (!TryCalculateTotalCost(out double totalCost))
                return;

            ApplyPendingTreatmentLinesStock(null);

            var newVisit = new Visit
            {
                AnimalChipNumber = chipNumber,
                VisitDate = visitDate,
                Reason = reason,
                Symptoms = "",
                Diagnosis = diagnosis,
                VeterinarianName = "",
                BaseCost = double.Parse(BaseCostInput.Text?.Trim() ?? "0", CultureInfo.InvariantCulture),
                ArrivalStatus = GetSelectedArrivalStatus(),
                ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? "",
                TreatmentLines = CloneTreatmentLines(pendingTreatmentLines)
            };
            newVisit.SyncLegacyMedicationFields();

            AppData.Visits.Add(newVisit);

            AppData.SaveMedicationsToDatabase();
            AppData.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "הביקור נשמר בהצלחה");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private void UpdateVisit_Click(object? sender, RoutedEventArgs e)
        {
            UpdateSelectedVisit();
        }

        private void UpdateSelectedVisit()
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "קודם לחץ על כרטיס ביקור קיים מתוך היומן");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                UIHelper.ShowMessage(this, "הביקור כבר נסגר ולא ניתן לערוך אותו. הפרטים נשמרים בכרטיס הביקור.");
                return;
            }

            if (!TryReadVisitFields(
                selectedVisit,
                out DateTime visitDate,
                out string chipNumber,
                out string reason,
                out string diagnosis,
                out double baseCost,
                out double totalCost))
            {
                return;
            }

            string selectedArrivalStatus = GetSelectedArrivalStatus();

            if (selectedArrivalStatus != "Scheduled" && !IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "אי אפשר לסמן הגיע או לא הגיע לפני ששעת התור הסתיימה.");
                SetValidationMessage("סטטוס הגעה ניתן לעדכון רק אחרי ששעת התור עברה", isValid: false);
                return;
            }

            ApplyPendingTreatmentLinesStock(selectedVisit);

            selectedVisit.AnimalChipNumber = chipNumber;
            selectedVisit.VisitDate = visitDate;
            selectedVisit.Reason = reason;
            selectedVisit.Diagnosis = diagnosis;
            selectedVisit.BaseCost = baseCost;
            selectedVisit.ArrivalStatus = selectedArrivalStatus;
            selectedVisit.ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? "";
            selectedVisit.TreatmentLines = CloneTreatmentLines(pendingTreatmentLines);
            selectedVisit.SyncLegacyMedicationFields();

            AppData.SaveMedicationsToDatabase();
            AppData.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "הביקור עודכן בהצלחה");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private async void DeleteVisit_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "קודם לחץ על כרטיס ביקור קיים מתוך היומן");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                UIHelper.ShowMessage(this, "הביקור כבר נסגר ולא ניתן למחוק אותו כדי לשמור היסטוריית טיפול.");
                return;
            }

            bool confirmed = await UIHelper.ShowConfirmation(
                this,
                "האם אתה בטוח שברצונך למחוק את הביקור שנבחר? פעולה זו לא ניתנת לביטול.");

            if (!confirmed)
                return;

            RestoreMedicationStock(selectedVisit);
            AppData.Visits.Remove(selectedVisit);
            AppData.SaveMedicationsToDatabase();
            AppData.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "הביקור נמחק בהצלחה");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private void MarkArrived_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "קודם לחץ על כרטיס ביקור קיים מתוך היומן");
                return;
            }

            if (!CanRecordArrivalForSelectedVisit("אפשר לאשר הגעה רק אחרי ששעת התור הסתיימה."))
                return;

            selectedVisit.ArrivalStatus = "Arrived";
            selectedVisit.ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? "";
            AppData.SaveVisitsToDatabase();

            SelectArrivalStatus("Arrived");
            UIHelper.ShowMessage(this, "הגעת הלקוח אושרה ונשמרה עם ההערה");
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            RefreshVisitsList();
            VisitDetailsText.Text = BuildVisitDetailsText(selectedVisit);
        }

        private void MarkNoShowAndCreateNew_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "קודם לחץ על כרטיס ביקור קיים מתוך היומן");
                return;
            }

            if (!CanRecordArrivalForSelectedVisit("אפשר לסמן לא הגיע רק אחרי ששעת התור הסתיימה."))
                return;

            if (!TryGetVisitDate(out DateTime newVisitDate))
                return;

            if (IsVisitDateTimeInPast(newVisitDate))
            {
                UIHelper.ShowMessage(this, "לתור חדש אחרי אי הגעה יש לבחור תאריך ושעה עתידיים");
                return;
            }

            string note = ArrivalNoteInput.Text?.Trim() ?? "";
            var originalTreatmentLines = CloneTreatmentLines(selectedVisit.TreatmentLines);
            double originalTotalCost = selectedVisit.TotalCost;

            foreach (var line in originalTreatmentLines.Where(line =>
                         !string.IsNullOrWhiteSpace(line.MedicationName) && line.MedicationQuantity > 0))
            {
                var medication = AppData.Medications.FirstOrDefault(m => m.Name == line.MedicationName);

                if (medication == null)
                {
                    UIHelper.ShowMessage(this, "לא ניתן לקבוע תור חדש כי אחת התרופות של התור הישן כבר לא קיימת במלאי.");
                    return;
                }

                int availableAfterCancel = medication.StockQuantity + line.MedicationQuantity;

                if (line.MedicationQuantity > availableAfterCancel)
                {
                    UIHelper.ShowMessage(this, $"אין מספיק מלאי לקביעת תור חדש עם התרופה {line.MedicationName}.");
                    return;
                }
            }

            selectedVisit.ArrivalStatus = "NoShow";
            selectedVisit.ArrivalNote = string.IsNullOrWhiteSpace(note)
                ? "הלקוח לא הגיע לתור"
                : note;

            RestoreMedicationStock(selectedVisit);
            selectedVisit.TreatmentLines.Clear();
            selectedVisit.SyncLegacyMedicationFields();

            foreach (var line in originalTreatmentLines.Where(line =>
                         !string.IsNullOrWhiteSpace(line.MedicationName) && line.MedicationQuantity > 0))
            {
                var medication = AppData.Medications.FirstOrDefault(m => m.Name == line.MedicationName);
                medication!.StockQuantity -= line.MedicationQuantity;
            }

            var newVisit = new Visit
            {
                AnimalChipNumber = selectedVisit.AnimalChipNumber,
                VisitDate = newVisitDate,
                Reason = selectedVisit.Reason,
                Symptoms = "",
                Diagnosis = selectedVisit.Diagnosis,
                VeterinarianName = "",
                BaseCost = selectedVisit.BaseCost,
                ArrivalStatus = "Scheduled",
                ArrivalNote = $"תור חדש בעקבות אי הגעה. הערה קודמת: {selectedVisit.ArrivalNote}",
                TreatmentLines = CloneTreatmentLines(originalTreatmentLines)
            };
            newVisit.SyncLegacyMedicationFields();
            newVisit.TotalCost = originalTotalCost;

            AppData.Visits.Add(newVisit);

            AppData.SaveMedicationsToDatabase();
            AppData.SaveVisitsToDatabase();
            UIHelper.ShowMessage(this, "סומן שלא הגיע ונקבע תור חדש לפי התאריך והשעה שבטופס");
            ClearFields();
            RefreshVisitsList();
        }

        private bool TryGetVisitDate(out DateTime visitDate)
        {
            DateTime selectedDate = VisitDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            TimeSpan time = GetSelectedVisitTime();

            visitDate = selectedDate.Date.Add(time);
            return true;
        }

        private bool IsSelectedVisitDateTimeInPast(out DateTime visitDate)
        {
            DateTime selectedDate = VisitDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            visitDate = selectedDate.Date.Add(GetSelectedVisitTime());

            return IsVisitDateTimeInPast(visitDate);
        }

        private bool IsVisitDateTimeInPast(DateTime visitDate)
        {
            DateTime currentMinute = GetCurrentMinute();

            return visitDate < currentMinute;
        }

        private bool IsVisitDateTimeInFuture(DateTime visitDate)
        {
            return visitDate > GetCurrentMinute();
        }

        private bool CanRecordArrivalForSelectedVisit(string message)
        {
            if (selectedVisit == null)
                return false;

            if (IsVisitDateTimeInPast(selectedVisit.VisitDate))
                return true;

            UIHelper.ShowMessage(this, message);
            SetValidationMessage("פעולת הגעה מותרת רק אחרי ששעת התור עברה", isValid: false);
            return false;
        }

        private DateTime GetCurrentMinute()
        {
            return new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                0);
        }

        private TimeSpan GetSelectedVisitTime()
        {
            int hour = int.TryParse(VisitHourDropdown.SelectedItem?.ToString(), out int selectedHour)
                ? selectedHour
                : DateTime.Now.Hour;

            int minute = int.TryParse(VisitMinuteDropdown.SelectedItem?.ToString(), out int selectedMinute)
                ? selectedMinute
                : DateTime.Now.Minute;

            return new TimeSpan(hour, minute, 0);
        }

        private void SetVisitTime(TimeSpan time)
        {
            VisitHourDropdown.SelectedIndex = Math.Clamp(time.Hours, 0, 23);
            VisitMinuteDropdown.SelectedIndex = Math.Clamp(time.Minutes, 0, 59);
        }

        private bool TryCalculateTotalCost(out double totalCost, Visit? visitBeingUpdated = null)
        {
            totalCost = 0;
            string baseCostText = BaseCostInput.Text?.Trim() ?? "";

            if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
            {
                UIHelper.ShowMessage(this, "מחיר ביקור חייב להיות מספר");
                return false;
            }

            if (!ValidationService.IsValidMoney(baseCost))
            {
                UIHelper.ShowMessage(this, "מחיר ביקור לא יכול להיות שלילי");
                return false;
            }

            totalCost = baseCost + pendingTreatmentLines.Sum(line => line.LineCost);
            return true;
        }

        private bool TryReadVisitFields(
            Visit? visitBeingUpdated,
            out DateTime visitDate,
            out string chipNumber,
            out string reason,
            out string diagnosis,
            out double baseCost,
            out double totalCost)
        {
            chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            reason = ReasonInput.Text?.Trim() ?? "";
            diagnosis = DiagnosisInput.Text?.Trim() ?? "";
            baseCost = 0;
            totalCost = 0;

            string selectedChipNumber = chipNumber;
            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == selectedChipNumber);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "יש לבחור חיה קיימת לפני עדכון ביקור");
                visitDate = DateTime.Today;
                return false;
            }

            if (!TryGetVisitDate(out visitDate))
                return false;

            if (!ValidationService.IsValidVisitDate(visitDate))
            {
                UIHelper.ShowMessage(this, "תאריך ביקור לא תקין");
                return false;
            }

            if (IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "לא ניתן לעדכן ביקור לתאריך או שעה שכבר עברו");
                SetValidationMessage("בחר תאריך ושעה עתידיים או את הדקה הנוכחית", isValid: false);
                return false;
            }

            if (!ValidationService.IsRequiredText(reason))
            {
                UIHelper.ShowMessage(this, "יש למלא סיבת ביקור");
                return false;
            }

            if (pendingTreatmentLines.Count == 0)
            {
                UIHelper.ShowMessage(this, "יש להוסיף לפחות טיפול / קורס אחד לביקור");
                return false;
            }

            if (!double.TryParse(BaseCostInput.Text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out baseCost))
            {
                UIHelper.ShowMessage(this, "מחיר ביקור חייב להיות מספר");
                return false;
            }

            if (!TryCalculateTotalCost(out totalCost, visitBeingUpdated))
                return false;

            return true;
        }

        private void RestoreMedicationStock(Visit visit)
        {
            foreach (var line in visit.TreatmentLines)
            {
                if (string.IsNullOrWhiteSpace(line.MedicationName) || line.MedicationQuantity <= 0)
                    continue;

                var medication = AppData.Medications.FirstOrDefault(m => m.Name == line.MedicationName);

                if (medication != null)
                    medication.StockQuantity += line.MedicationQuantity;
            }
        }

        private void ApplyPendingTreatmentLinesStock(Visit? visitBeingUpdated)
        {
            if (visitBeingUpdated != null)
                RestoreMedicationStock(visitBeingUpdated);

            foreach (var line in pendingTreatmentLines)
            {
                if (string.IsNullOrWhiteSpace(line.MedicationName) || line.MedicationQuantity <= 0)
                    continue;

                var medication = AppData.Medications.FirstOrDefault(m => m.Name == line.MedicationName);

                if (medication != null)
                    medication.StockQuantity -= line.MedicationQuantity;
            }
        }

        private void ClearFields_Click(object? sender, RoutedEventArgs e)
        {
            ClearFields();
            RefreshVisitsList();
        }

        private void ShowAllVisits_Click(object? sender, RoutedEventArgs e)
        {
            showUpcomingVisitsOnly = false;
            selectedSpeciesFilter = "";
            selectedAnimalChipNumber = "";
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "יומן ביקורים";
            VisitsJournalSubtitleText.Text = "רשימת ביקורים שמורים לפי תאריך, חיה, אבחנה ועלות";
            AnimalQuickCardsTitleText.Text = "בחר סוג חיה לפתיחת רשימת החיות";
            RefreshVisitsList();
        }

        private void ShowUpcomingVisits_Click(object? sender, RoutedEventArgs e)
        {
            showUpcomingVisitsOnly = true;
            selectedSpeciesFilter = "";
            selectedAnimalChipNumber = "";
            selectedVisit = null;
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "תורים עתידיים";
            VisitsJournalSubtitleText.Text = "רשימת התורים שעוד לא הגיע זמנם, מהקרוב לרחוק";
            AnimalQuickCardsTitleText.Text = "בחר סוג חיה או לחץ על כרטיס תור עתידי";
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            RefreshVisitsList();
        }

        private void FilterDogs_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("כלב");
        }

        private void FilterCats_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("חתול");
        }

        private void FilterReptiles_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("זוחל");
        }

        private void FilterBirds_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("ציפור");
        }

        private void ShowSpeciesAnimalList(string species)
        {
            showUpcomingVisitsOnly = false;
            selectedSpeciesFilter = species;
            selectedAnimalChipNumber = "";
            selectedVisit = null;
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "יומן ביקורים";
            VisitsJournalSubtitleText.Text = "בחר חיה מתוך הרשימה כדי לפתוח את תיק הביקורים שלה";
            AnimalQuickCardsTitleText.Text = $"רשימת {GetSpeciesPluralText(species)}";
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            RefreshVisitsList();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }

        private void ClearFields()
        {
            selectedVisit = null;
            selectedAnimalChipNumber = "";
            AnimalSearchInput.Text = "";
            AnimalChipInput.Text = "";
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "יומן ביקורים";
            VisitsJournalSubtitleText.Text = "רשימת ביקורים שמורים לפי תאריך, חיה, אבחנה ועלות";
            VisitDatePicker.SelectedDate = DateTime.Today;
            SetVisitTime(DateTime.Now.TimeOfDay);
            ReasonInput.Text = "";
            DiagnosisInput.Text = "";
            TreatmentLineDescriptionInput.Text = "";
            BaseCostInput.Text = "";
            TreatmentModeDropdown.SelectedIndex = 0;
            MedicationFieldsPanel.IsVisible = false;
            MedicationQuantityInput.Text = "";
            MedicationDropdown.SelectedIndex = -1;
            MedicationStockText.Text = "";
            ClearPendingTreatmentLines();
            ArrivalStatusDropdown.SelectedIndex = 0;
            ArrivalNoteInput.Text = "";
            ArrivalPanel.IsVisible = false;
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            TotalCostText.Text = "עלות כוללת: 0";
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void ShowInlineWarning(string message)
        {
            ValidationText.Foreground = Avalonia.Media.Brushes.DarkOrange;
            ValidationText.Text = message;
        }

        private void RefreshVisitTimeWarning()
        {
            UpdateArrivalActionsAvailability();

            bool hasFormData =
                !string.IsNullOrWhiteSpace(AnimalChipInput.Text) ||
                !string.IsNullOrWhiteSpace(ReasonInput.Text) ||
                !string.IsNullOrWhiteSpace(DiagnosisInput.Text) ||
                !string.IsNullOrWhiteSpace(BaseCostInput.Text);

            if (hasFormData && IsSelectedVisitDateTimeInPast(out _))
                SetValidationMessage("לא ניתן לשמור ביקור בתאריך או שעה שכבר עברו", isValid: false);
        }

        private void UpdateArrivalActionsAvailability()
        {
            bool canRecordArrival = selectedVisit != null &&
                !IsVisitClosed(selectedVisit) &&
                IsVisitDateTimeInPast(selectedVisit.VisitDate);

            ArrivalPanel.IsVisible = canRecordArrival;
            MarkArrivedButton.IsVisible = canRecordArrival;
            MarkNoShowButton.IsVisible = canRecordArrival;
            MarkArrivedButton.IsEnabled = canRecordArrival;
            MarkNoShowButton.IsEnabled = canRecordArrival;

            string tip = canRecordArrival
                ? "אפשר לעדכן הגעה כי זמן התור כבר עבר"
                : "אפשר לעדכן הגעה רק אחרי ששעת התור הסתיימה";

            ToolTip.SetTip(MarkArrivedButton, tip);
            ToolTip.SetTip(MarkNoShowButton, tip);
        }

        private bool IsVisitClosed(Visit visit)
        {
            return visit.ArrivalStatus is "Arrived" or "NoShow";
        }

        private void RefreshMedicationDropdown()
        {
            var items = AppData.Medications
                .Select(m => m.Name)
                .ToList();

            MedicationDropdown.ItemsSource = items;
            MedicationDropdown.SelectedIndex = items.Count > 0 ? 0 : -1;
            UpdateMedicationStockHint();
        }

        private void UpdateMedicationStockHint()
        {
            if (!IsMedicationTreatmentSelected())
            {
                MedicationStockText.Text = "";
                return;
            }

            string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(medicationName))
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = "אין תרופות זמינות במלאי";
                return;
            }

            var medication = AppData.Medications.FirstOrDefault(m => m.Name == medicationName);

            if (medication == null)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = "התרופה שנבחרה לא קיימת במלאי";
                return;
            }

            int availableStock = GetAvailableMedicationStock(medication);
            string quantityText = MedicationQuantityInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(quantityText))
            {
                MedicationStockText.Foreground = availableStock <= 5 ? Brushes.DarkOrange : Brushes.ForestGreen;
                MedicationStockText.Text = $"במלאי עכשיו: {availableStock} יחידות";
                return;
            }

            if (!int.TryParse(quantityText, out int requestedQuantity) || requestedQuantity <= 0)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = $"במלאי עכשיו: {availableStock}. הכמות חייבת להיות מספר חיובי";
                return;
            }

            int remainingStock = availableStock - requestedQuantity;

            if (remainingStock < 0)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = $"אין מספיק מלאי: ביקשת {requestedQuantity}, זמין {availableStock}";
                return;
            }

            MedicationStockText.Foreground = remainingStock <= 5 ? Brushes.DarkOrange : Brushes.ForestGreen;
            MedicationStockText.Text = $"במלאי עכשיו: {availableStock} | אחרי שמירה יישאר: {remainingStock}";
        }

        private int GetAvailableMedicationStock(Medication medication)
        {
            int availableStock = medication.StockQuantity;

            if (selectedVisit != null &&
                selectedVisit.MedicationName == medication.Name &&
                selectedVisit.MedicationQuantity > 0)
            {
                availableStock += selectedVisit.MedicationQuantity;
            }

            return availableStock;
        }

        private void RefreshVisitsList()
        {
            RefreshAnimalQuickCards();
            VisitCardsPanel.Children.Clear();

            if (!string.IsNullOrWhiteSpace(selectedSpeciesFilter) &&
                string.IsNullOrWhiteSpace(selectedAnimalChipNumber))
            {
                VisitDetailsText.Text = $"בחר חיה מתוך רשימת {GetSpeciesPluralText(selectedSpeciesFilter)} כדי לראות את התורים שלה";
                return;
            }

            IEnumerable<Visit> visitsToShow = string.IsNullOrWhiteSpace(selectedAnimalChipNumber)
                ? AppData.Visits
                : AppData.Visits.Where(visit => visit.AnimalChipNumber == selectedAnimalChipNumber);
            var now = DateTime.Now;

            if (showUpcomingVisitsOnly)
            {
                visitsToShow = visitsToShow.Where(visit =>
                    visit.ArrivalStatus == "Scheduled" &&
                    visit.VisitDate >= now);
            }

            var visitsList = visitsToShow.ToList();

            if (AppData.Visits.Count == 0)
            {
                VisitDetailsText.Text = "אין ביקורים במערכת";
                return;
            }

            if (visitsList.Count == 0)
            {
                VisitDetailsText.Text = showUpcomingVisitsOnly
                    ? "אין תורים עתידיים כרגע"
                    : "אין ביקורים שמורים לחיה שנבחרה";
                return;
            }

            VisitDetailsText.Text = selectedVisit != null
                ? BuildVisitDetailsText(selectedVisit)
                : showUpcomingVisitsOnly
                    ? "תורים עתידיים מוצגים כאן מהקרוב לרחוק"
                : string.IsNullOrWhiteSpace(selectedAnimalChipNumber)
                    ? "ביקורים עתידיים מוצגים קודם מהקרוב לרחוק. ביקורים שעברו מופיעים אחריהם כהיסטוריה."
                    : "מוצגים רק הביקורים של החיה שנבחרה";

            var sortedVisits = visitsList
                .OrderBy(visit => visit.VisitDate >= now ? 0 : 1)
                .ThenBy(visit => visit.VisitDate >= now ? visit.VisitDate.Ticks : -visit.VisitDate.Ticks);

            foreach (var visit in sortedVisits)
            {
                VisitCardsPanel.Children.Add(CreateVisitCard(visit));
            }
        }

        private void RefreshAnimalQuickCards()
        {
            AnimalQuickCardsPanel.Children.Clear();

            if (string.IsNullOrWhiteSpace(selectedSpeciesFilter))
            {
                AnimalQuickCardsPanel.Children.Add(new TextBlock
                {
                    Text = "בחר סוג חיה מהכפתורים בצד כדי לראות את החיות",
                    Foreground = new SolidColorBrush(Color.Parse("#526172")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8)
                });
                return;
            }

            if (AppData.Animals.Count == 0)
            {
                AnimalQuickCardsPanel.Children.Add(new TextBlock
                {
                    Text = "אין עדיין בעלי חיים במערכת",
                    Foreground = new SolidColorBrush(Color.Parse("#526172")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8)
                });
                return;
            }

            var animalsBySpecies = AppData.Animals
                .Where(animal => SpeciesMatchesFilter(animal.Species, selectedSpeciesFilter))
                .OrderBy(animal => animal.Name)
                .ToList();

            if (animalsBySpecies.Count == 0)
            {
                AnimalQuickCardsPanel.Children.Add(new TextBlock
                {
                    Text = $"אין {GetSpeciesPluralText(selectedSpeciesFilter)} במערכת",
                    Foreground = new SolidColorBrush(Color.Parse("#526172")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8)
                });
                return;
            }

            foreach (var animal in animalsBySpecies)
            {
                AnimalQuickCardsPanel.Children.Add(CreateAnimalQuickCard(animal));
            }
        }

        private bool SpeciesMatchesFilter(string animalSpecies, string filter)
        {
            return filter switch
            {
                "כלב" => animalSpecies is "כלב" or "Dog",
                "חתול" => animalSpecies is "חתול" or "Cat",
                "זוחל" => animalSpecies is "זוחל" or "Reptile",
                "ציפור" => animalSpecies is "ציפור" or "Bird",
                _ => true
            };
        }

        private string GetSpeciesPluralText(string species)
        {
            return species switch
            {
                "כלב" => "כלבים",
                "חתול" => "חתולים",
                "זוחל" => "זוחלים",
                "ציפור" => "ציפורים",
                _ => "בעלי חיים"
            };
        }

        private Button CreateAnimalQuickCard(Animal animal)
        {
            bool isSelected = selectedAnimalChipNumber == animal.ChipNumber;
            int visitsCount = AppData.Visits.Count(visit => visit.AnimalChipNumber == animal.ChipNumber);
            string accentColor = GetVisitAnimalAccentColor(animal.Species);
            string strongColor = isSelected ? GetVisitAnimalStrongColor(animal.Species) : "#B7D6E3";

            var card = new Button
            {
                Width = 132,
                Height = 92,
                Margin = new Thickness(6),
                Padding = new Thickness(0),
                Background = new SolidColorBrush(Color.Parse(accentColor)),
                BorderBrush = new SolidColorBrush(Color.Parse(strongColor)),
                BorderThickness = new Thickness(isSelected ? 3 : 1),
                Content = new StackPanel
                {
                    Spacing = 3,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = GetVisitAnimalIcon(animal.Species),
                            FontSize = 26,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = animal.Name,
                            FontSize = 15,
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Color.Parse("#13293D")),
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            MaxWidth = 110
                        },
                        new TextBlock
                        {
                            Text = $"{visitsCount} ביקורים",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#526172")),
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            };

            ToolTip.SetTip(card, $"פתח תיק ביקורים של {animal.Name}");

            card.Click += (_, _) =>
            {
                selectedVisit = null;
                ShowSelectedAnimalCard(animal);
                UpdateArrivalActionsAvailability();
                UpdateSaveButtonMode();
                UpdateVisitEditingState();
                RefreshVisitsList();
            };

            return card;
        }

        private Button CreateVisitCard(Visit visit)
        {
            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);
            string animalName = animal != null ? animal.Name : visit.AnimalChipNumber;
            string animalSpecies = animal?.Species ?? "";
            string accentColor = GetVisitAnimalAccentColor(animalSpecies);
            string strongColor = GetVisitAnimalStrongColor(animalSpecies);
            string medicationText = BuildTreatmentLinesSummary(visit);
            string visitStatusText = GetDisplayVisitStatusText(visit);
            string visitStatusColor = GetDisplayVisitStatusColor(visit);

            var card = new Button
            {
                Width = 240,
                MinHeight = 190,
                Margin = new Thickness(8),
                Padding = new Thickness(0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse(visitStatusColor)),
                BorderThickness = new Thickness(3),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Spacing = 0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new Border
                        {
                            Width = 234,
                            Height = 78,
                            CornerRadius = new CornerRadius(14, 14, 26, 26),
                            Background = new SolidColorBrush(Color.Parse(accentColor)),
                            Child = new Grid
                            {
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = $"{GetVisitAnimalIcon(animalSpecies)} 🩺",
                                        FontSize = 34,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    },
                                    new Border
                                    {
                                        Background = new SolidColorBrush(Color.Parse(visitStatusColor)),
                                        CornerRadius = new CornerRadius(12),
                                        Padding = new Thickness(10, 4),
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        Margin = new Thickness(10),
                                        Child = new TextBlock
                                        {
                                            Text = visitStatusText,
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
                            Spacing = 7,
                            Margin = new Thickness(14),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = animalName,
                                    FontSize = 20,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                                },
                                new TextBlock
                                {
                                    Text = visit.VisitDate.ToString("dd/MM/yyyy HH:mm"),
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new TextBlock
                                {
                                    Text = medicationText,
                                    FontSize = 13,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Foreground = new SolidColorBrush(Color.Parse("#526172"))
                                },
                                new Border
                                {
                                    Background = new SolidColorBrush(Color.Parse("#EAF8EF")),
                                    CornerRadius = new CornerRadius(12),
                                    Padding = new Thickness(10, 5),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Child = new TextBlock
                                    {
                                        Text = $"עלות: {visit.TotalCost:0.00}",
                                        FontSize = 13,
                                        FontWeight = FontWeight.Bold,
                                        TextAlignment = TextAlignment.Center,
                                        Foreground = Brushes.ForestGreen
                                    }
                                }
                            }
                        }
                    }
                }
            };

            card.Click += (_, _) => ShowVisitDetails(visit);

            return card;
        }

        private void ShowVisitDetails(Visit visit)
        {
            if (selectedVisit == visit)
            {
                CloseSelectedVisitDetails();
                return;
            }

            selectedVisit = visit;
            FillVisitFields(visit);
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            RefreshVisitsList();
            VisitDetailsText.Text = BuildVisitDetailsText(visit);
            SetValidationMessage(GetSelectedVisitHelpText(visit), isValid: true);
        }

        private void CloseSelectedVisitDetails()
        {
            selectedVisit = null;
            VisitDatePicker.SelectedDate = DateTime.Today;
            SetVisitTime(DateTime.Now.TimeOfDay);
            ReasonInput.Text = "";
            DiagnosisInput.Text = "";
            BaseCostInput.Text = "";
            TreatmentModeDropdown.SelectedIndex = 0;
            MedicationFieldsPanel.IsVisible = false;
            MedicationQuantityInput.Text = "";
            MedicationDropdown.SelectedIndex = -1;
            MedicationStockText.Text = "";
            ArrivalStatusDropdown.SelectedIndex = 0;
            ArrivalNoteInput.Text = "";
            ArrivalPanel.IsVisible = false;
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            TotalCostText.Text = "עלות כוללת: 0";
            ValidationText.Text = "";
            RefreshVisitsList();
        }

        private string GetSelectedVisitHelpText(Visit visit)
        {
            if (IsVisitClosed(visit))
                return "הביקור סגור ונשמר בכרטיס. אפשר לצפות בפרטים אך לא לערוך אותו";

            if (IsVisitDateTimeInPast(visit.VisitDate))
                return "התור עבר. אפשר לאשר הגעה או לסמן לא הגיע";

            return "הביקור נטען. אפשר לעדכן אותו עד ששעת התור תעבור";
        }

        private void UpdateSaveButtonMode()
        {
            if (selectedVisit == null)
            {
                SaveVisitButton.Content = "שמור ביקור";
                SaveVisitButton.IsEnabled = true;
                DeleteVisitButton.IsEnabled = false;
                ToolTip.SetTip(SaveVisitButton, "שומר ביקור חדש");
                ToolTip.SetTip(DeleteVisitButton, "מחיקה זמינה רק אחרי בחירת ביקור פתוח");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                SaveVisitButton.Content = "ביקור סגור";
                SaveVisitButton.IsEnabled = false;
                DeleteVisitButton.IsEnabled = false;
                ToolTip.SetTip(SaveVisitButton, "הביקור נסגר ונשמר בכרטיס, אי אפשר לערוך אותו");
                ToolTip.SetTip(DeleteVisitButton, "ביקור סגור לא ניתן למחיקה");
                return;
            }

            SaveVisitButton.Content = "עדכן ביקור";
            SaveVisitButton.IsEnabled = true;
            DeleteVisitButton.IsEnabled = true;
            ToolTip.SetTip(SaveVisitButton, "מעדכן את הביקור שנבחר");
            ToolTip.SetTip(DeleteVisitButton, "מוחק ביקור פתוח אחרי אישור");
        }

        private void UpdateVisitEditingState()
        {
            bool canEdit = selectedVisit == null || !IsVisitClosed(selectedVisit);

            AnimalSearchInput.IsEnabled = canEdit;
            AnimalChipInput.IsEnabled = canEdit;
            VisitDatePicker.IsEnabled = canEdit;
            VisitHourDropdown.IsEnabled = canEdit;
            VisitMinuteDropdown.IsEnabled = canEdit;
            ReasonInput.IsEnabled = canEdit;
            DiagnosisInput.IsEnabled = canEdit;
            TreatmentLineDescriptionInput.IsEnabled = canEdit;
            BaseCostInput.IsEnabled = canEdit;
            TreatmentModeDropdown.IsEnabled = canEdit;
            MedicationDropdown.IsEnabled = canEdit;
            MedicationQuantityInput.IsEnabled = canEdit;
            ArrivalStatusDropdown.IsEnabled = canEdit;
            ArrivalNoteInput.IsEnabled = canEdit;
        }

        private void FillVisitFields(Visit visit)
        {
            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);

            AnimalSearchInput.Text = animal?.Name ?? visit.AnimalChipNumber;
            AnimalChipInput.Text = visit.AnimalChipNumber;
            if (animal != null)
                ShowSelectedAnimalCard(animal);
            else
                SelectedAnimalPanel.IsVisible = false;
            VisitDatePicker.SelectedDate = visit.VisitDate.Date;
            SetVisitTime(visit.VisitDate.TimeOfDay);
            ReasonInput.Text = visit.Reason;
            DiagnosisInput.Text = visit.Diagnosis;
            BaseCostInput.Text = visit.BaseCost.ToString(CultureInfo.InvariantCulture);
            ClearTreatmentLineInputs();
            LoadPendingTreatmentLinesFromVisit(visit);
            SelectArrivalStatus(visit.ArrivalStatus);
            ArrivalNoteInput.Text = visit.ArrivalNote;
            TotalCostText.Text = $"עלות כוללת: {visit.TotalCost:0.00}";
        }

        private string BuildTreatmentLinesSummary(Visit visit)
        {
            if (visit.TreatmentLines.Count == 0)
            {
                return string.IsNullOrWhiteSpace(visit.MedicationName)
                    ? "ללא טיפולים"
                    : $"{visit.MedicationName} x {visit.MedicationQuantity}";
            }

            return string.Join(" | ", visit.TreatmentLines.Select(line =>
            {
                string medicationText = string.IsNullOrWhiteSpace(line.MedicationName)
                    ? "ללא תרופה"
                    : $"{line.MedicationName} x {line.MedicationQuantity}";

                return $"{line.Description} ({medicationText})";
            }));
        }

        private void ShowSelectedAnimalCard(Animal animal)
        {
            var owner = AppData.Clients.FirstOrDefault(client => client.IdNumber == animal.OwnerIdNumber);
            bool vaccinationDue = ValidationService.IsVaccinationDue(animal.LastVaccinationDate);
            int visitsCount = AppData.Visits.Count(visit => visit.AnimalChipNumber == animal.ChipNumber);
            DateTime? nextVisitDate = AppData.Visits
                .Where(visit => visit.AnimalChipNumber == animal.ChipNumber &&
                    visit.ArrivalStatus == "Scheduled" &&
                    visit.VisitDate >= DateTime.Now)
                .OrderBy(visit => visit.VisitDate)
                .Select(visit => (DateTime?)visit.VisitDate)
                .FirstOrDefault();

            selectedAnimalChipNumber = animal.ChipNumber;
            VisitsJournalTitleText.Text = $"תיק ביקורים של {animal.Name}";
            VisitsJournalSubtitleText.Text = nextVisitDate.HasValue
                ? $"כל התורים של החיה הזו. התור הקרוב: {nextVisitDate.Value:dd/MM/yyyy HH:mm}"
                : "כל התורים של החיה הזו, כולל עתידיים והיסטוריה";
            SelectedAnimalIconText.Text = GetVisitAnimalIcon(animal.Species);
            SelectedAnimalNameText.Text = animal.Name;
            SelectedAnimalChipText.Text = $"שבב: {animal.ChipNumber}";
            SelectedAnimalOwnerText.Text = $"בעלים: {(owner != null ? owner.FullName : animal.OwnerIdNumber)}";
            SelectedAnimalVaccineText.Text = vaccinationDue
                ? "חיסון: צריך חיסון שנתי"
                : "חיסון: תקין";
            SelectedAnimalVisitsCountText.Text = $"ביקורים שמורים: {visitsCount}";
            SelectedAnimalVaccineText.Foreground = vaccinationDue
                ? Brushes.Firebrick
                : Brushes.ForestGreen;
            SelectedAnimalPanel.IsVisible = true;
        }

        private void SelectMedication(string medicationName)
        {
            string target = string.IsNullOrWhiteSpace(medicationName)
                ? ""
                : medicationName;

            if (string.IsNullOrWhiteSpace(target))
            {
                MedicationDropdown.SelectedIndex = -1;
                return;
            }

            for (int i = 0; i < MedicationDropdown.ItemCount; i++)
            {
                if (MedicationDropdown.Items[i]?.ToString() == target)
                {
                    MedicationDropdown.SelectedIndex = i;
                    return;
                }
            }

            MedicationDropdown.SelectedIndex = MedicationDropdown.ItemCount > 0 ? 0 : -1;
        }

        private string GetSelectedArrivalStatus()
        {
            if (ArrivalStatusDropdown.SelectedItem is ComboBoxItem item &&
                item.Content != null)
            {
                string statusText = item.Content.ToString() ?? "";

                if (statusText.Contains("הגיע") && !statusText.Contains("לא"))
                    return "Arrived";

                if (statusText.Contains("לא"))
                    return "NoShow";
            }

            return "Scheduled";
        }

        private void SelectArrivalStatus(string status)
        {
            ArrivalStatusDropdown.SelectedIndex = status switch
            {
                "Arrived" => 1,
                "NoShow" => 2,
                _ => 0
            };
        }

        private string BuildVisitDetailsText(Visit visit)
        {
            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);
            var owner = animal == null
                ? null
                : AppData.Clients.FirstOrDefault(c => c.IdNumber == animal.OwnerIdNumber);

            string medicationText = BuildTreatmentLinesSummary(visit);
            string statusText = GetVisitStatusText(visit.VisitDate);
            string arrivalStatusText = GetArrivalStatusText(visit.ArrivalStatus);

            return $"""
                סטטוס: {statusText}
                הגעה: {arrivalStatusText}
                תאריך: {visit.VisitDate:dd/MM/yyyy HH:mm}
                חיה: {(animal != null ? animal.Name : visit.AnimalChipNumber)}
                מספר שבב: {visit.AnimalChipNumber}
                בעלים: {(owner != null ? owner.FullName : "לא נמצא")}
                סיבת הגעה: {visit.Reason}
                אבחנה / טיפול: {visit.Diagnosis}
                תרופה: {medicationText}
                עלות בסיסית: {visit.BaseCost:0.00}
                עלות כוללת: {visit.TotalCost:0.00}
                הערת וטרינר: {(string.IsNullOrWhiteSpace(visit.ArrivalNote) ? "אין הערה" : visit.ArrivalNote)}
                """;
        }

        private string GetDisplayVisitStatusText(Visit visit)
        {
            return visit.ArrivalStatus switch
            {
                "Arrived" => "הגיע",
                "NoShow" => "לא הגיע",
                _ => GetVisitStatusText(visit.VisitDate)
            };
        }

        private string GetDisplayVisitStatusColor(Visit visit)
        {
            return visit.ArrivalStatus switch
            {
                "Arrived" => "#1E8F4D",
                "NoShow" => "#D64545",
                _ => GetVisitStatusColor(visit.VisitDate)
            };
        }

        private string GetArrivalStatusText(string status)
        {
            return status switch
            {
                "Arrived" => "הגיע",
                "NoShow" => "לא הגיע",
                _ => "ממתין להגעה"
            };
        }

        private string GetVisitStatusText(DateTime visitDate)
        {
            DateTime now = DateTime.Now;

            if (visitDate < now)
                return "עבר";

            if (visitDate.Date == now.Date)
                return "היום";

            if (visitDate <= now.AddDays(7))
                return "קרוב";

            return "עתידי";
        }

        private string GetVisitStatusColor(DateTime visitDate)
        {
            return GetVisitStatusText(visitDate) switch
            {
                "עבר" => "#8A94A6",
                "היום" => "#D97706",
                "קרוב" => "#D64545",
                _ => "#0797C9"
            };
        }

        private string GetVisitAnimalIcon(string species)
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

        private string GetVisitAnimalAccentColor(string species)
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

        private string GetVisitAnimalStrongColor(string species)
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
