using ClinicVets.Desktop.Services;
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
using ClinicVets.Desktop.Helpers;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers.Stability;

namespace ClinicVets.Desktop.Views.Visits {
    public partial class VisitsView : UserControl
    {
        public Action? BackToMainMenu;
        private Visit? selectedVisit;
        private string selectedAnimalChipNumber = "";
        private string selectedSpeciesFilter = "";
        private bool showUpcomingVisitsOnly;
        private readonly DispatcherTimer clockTimer = new DispatcherTimer();
        private bool isViewReady;

        public VisitsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            await SafeViewLoader.RunSafeAsync(this, InitializeViewAsync, "Visits.Load");
        }

        private async Task InitializeViewAsync()
        {
            await VisitDataBridge.RefreshAsync();
            isViewReady = true;
            PopulateTimeDropdowns();
            VisitDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
            SetVisitTime(DateTime.Now.TimeOfDay);
            UpdateMedicationFieldsVisibility(clearFields: false);
            WatchVisitDateChanges();
            StartClock();
            RefreshMedicationDropdown();
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

            if (!string.IsNullOrWhiteSpace(chipNumber) && !UiFormValidation.IsValidChipNumber(chipNumber))
            {
                SetValidationMessage("Î×ÎíÎñÎ¿ Î®ÎæÎæ ÎùÎÖÎÖÎæ Î£ÎöÎÖÎòÎ¬ 7 ÎíÎñÎ¿ÎòÎ¬ ÎòÎ£ÎöÎ¬ÎùÎÖÎ£ Îæ-376", isValid: false);
                return;
            }

            if (IsSelectedVisitDateTimeInPast(out _))
            {
                SetValidationMessage("Î£ÎÉ ÎáÎÖÎ¬Î Î£Î®Î×ÎòÎ¿ ÎæÎÖÎºÎòÎ¿ ÎæÎ¬ÎÉÎ¿ÎÖÎÜ ÎÉÎò Î®ÎóÎö Î®ÎøÎæÎ¿ ÎóÎæÎ¿Îò", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(baseCostText))
            {
                if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
                {
                    SetValidationMessage("Î×ÎùÎÖÎ¿ ÎæÎÖÎºÎòÎ¿ ÎùÎÖÎÖÎæ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿", isValid: false);
                    return;
                }

                if (!UiFormValidation.IsValidMoney(baseCost))
                {
                    SetValidationMessage("Î×ÎùÎÖÎ¿ ÎæÎÖÎºÎòÎ¿ Î£ÎÉ ÎÖÎøÎòÎ£ Î£ÎöÎÖÎòÎ¬ Î®Î£ÎÖÎ£ÎÖ", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(medicationQuantityText))
            {
                if (!int.TryParse(medicationQuantityText, out int medicationQuantity))
                {
                    SetValidationMessage("ÎøÎ×ÎòÎ¬ Î¬Î¿ÎòÎñÎö ÎùÎÖÎÖÎæÎ¬ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿ Î®Î£ÎØ", isValid: false);
                    return;
                }

                if (medicationQuantity < 0)
                {
                    SetValidationMessage("ÎøÎ×ÎòÎ¬ Î¬Î¿ÎòÎñÎö Î£ÎÉ ÎÖÎøÎòÎ£Îö Î£ÎöÎÖÎòÎ¬ Î®Î£ÎÖÎ£ÎÖÎ¬", isValid: false);
                    return;
                }

                string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";
                var medication = VisitDataBridge.Medications.FirstOrDefault(m => m.Name == medicationName);

                if (medication != null && medicationQuantity > GetAvailableMedicationStock(medication))
                {
                    SetValidationMessage("ÎÉÎÖÎ Î×ÎíÎñÎÖÎº Î×Î£ÎÉÎÖ Î£ÎøÎ×ÎòÎ¬ ÎöÎ¬Î¿ÎòÎñÎö Î®ÎöÎòÎûÎáÎö", isValid: false);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(reason) ||
                string.IsNullOrWhiteSpace(diagnosis))
            {
                SetValidationMessage("ÎÖÎ® Î£Î×Î£ÎÉ ÎíÎÖÎæÎ¬ ÎæÎÖÎºÎòÎ¿ ÎòÎÉÎæÎùÎáÎö / ÎÿÎÖÎñÎòÎ£", isValid: false);
                return;
            }

            SetValidationMessage("ÎöÎñÎ¿ÎÿÎÖÎØ ÎáÎ¿ÎÉÎÖÎØ Î¬ÎºÎÖÎáÎÖÎØ", isValid: true);
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string search = AnimalSearchInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(search))
                search = AnimalChipInput.Text?.Trim() ?? "";

            string normalizedSearchChip = NormalizeChipNumber(search);

            var animal = VisitDataBridge.Animals.FirstOrDefault(a =>
                NormalizeChipNumber(a.ChipNumber) == normalizedSearchChip ||
                a.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (animal == null)
            {
                SelectedAnimalPanel.IsVisible = false;
                UIHelper.ShowMessage(this, "Î£ÎÉ ÎáÎ×ÎªÎÉÎö ÎùÎÖÎö Î£ÎñÎÖ Î®ÎØ ÎÉÎò Î×ÎíÎñÎ¿ Î®ÎæÎæ");
                return;
            }

            AnimalChipInput.Text = animal.ChipNumber;
            AnimalSearchInput.Text = animal.Name;
            ShowSelectedAnimalCard(animal);
            RefreshVisitsList();

            if (UiFormValidation.IsVaccinationDue(animal.LastVaccinationDate))
            {
                ShowInlineWarning("Î¬ÎûÎøÎòÎ¿Î¬: ÎöÎùÎÖÎö ÎªÎ¿ÎÖÎøÎö ÎùÎÖÎíÎòÎ Î®ÎáÎ¬ÎÖ, ÎÉÎñÎ®Î¿ Î£ÎöÎ×Î®ÎÖÎÜ ÎòÎ£Î®Î×ÎòÎ¿ ÎæÎÖÎºÎòÎ¿");
                return;
            }

            SetValidationMessage("ÎöÎùÎÖÎö ÎáÎ×ÎªÎÉÎö ÎòÎÉÎñÎ®Î¿ Î£ÎöÎ×Î®ÎÖÎÜ ÎæÎæÎÖÎºÎòÎ¿", isValid: true);
        }

        private string NormalizeChipNumber(string value)
        {
            return new string((value ?? "").Where(char.IsDigit).ToArray());
        }

        private void CalculateCost_Click(object? sender, RoutedEventArgs e)
        {
            if (TryCalculateTotalCost(out double totalCost, out _, out _))
                TotalCostText.Text = $"ÎóÎ£ÎòÎ¬ ÎøÎòÎ£Î£Î¬: {totalCost:0.00}";
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

            var animal = VisitDataBridge.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "ÎÖÎ® Î£ÎæÎùÎòÎ¿ ÎùÎÖÎö ÎºÎÖÎÖÎ×Î¬ Î£ÎñÎáÎÖ Î®Î×ÎÖÎ¿Î¬ ÎæÎÖÎºÎòÎ¿");
                return;
            }

            if (!TryGetVisitDate(out DateTime visitDate))
                return;

            if (!UiFormValidation.IsValidVisitDate(visitDate))
            {
                UIHelper.ShowMessage(this, "Î¬ÎÉÎ¿ÎÖÎÜ ÎæÎÖÎºÎòÎ¿ Î£ÎÉ Î¬ÎºÎÖÎ");
                return;
            }

            if (IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "Î£ÎÉ ÎáÎÖÎ¬Î Î£Î®Î×ÎòÎ¿ ÎæÎÖÎºÎòÎ¿ ÎæÎ¬ÎÉÎ¿ÎÖÎÜ ÎÉÎò Î®ÎóÎö Î®ÎøÎæÎ¿ ÎóÎæÎ¿Îò");
                SetValidationMessage("ÎæÎùÎ¿ Î¬ÎÉÎ¿ÎÖÎÜ ÎòÎ®ÎóÎö ÎóÎ¬ÎÖÎôÎÖÎÖÎØ ÎÉÎò ÎÉÎ¬ ÎöÎôÎºÎö ÎöÎáÎòÎøÎùÎÖÎ¬", isValid: false);
                return;
            }

            if (!UiFormValidation.IsRequiredText(reason) ||
                !UiFormValidation.IsRequiredText(diagnosis))
            {
                UIHelper.ShowMessage(this, "ÎÖÎ® Î£Î×Î£ÎÉ ÎíÎÖÎæÎ¬ ÎæÎÖÎºÎòÎ¿ ÎòÎÉÎæÎùÎáÎö / ÎÿÎÖÎñÎòÎ£");
                return;
            }

            if (!TryCalculateTotalCost(out double totalCost, out Medication? medication, out int medicationQuantity))
                return;

            if (medication != null && medicationQuantity > 0)
            {
                medication.StockQuantity -= medicationQuantity;
                VisitDataBridge.SaveMedicationsToDatabase();
            }

            VisitDataBridge.Visits.Add(new Visit
            {
                AnimalChipNumber = chipNumber,
                VisitDate = visitDate,
                Reason = reason,
                Symptoms = "",
                Diagnosis = diagnosis,
                VeterinarianName = "",
                BaseCost = double.Parse(BaseCostInput.Text?.Trim() ?? "0", CultureInfo.InvariantCulture),
                MedicationName = medication?.Name ?? "",
                MedicationQuantity = medicationQuantity,
                TotalCost = totalCost,
                ArrivalStatus = GetSelectedArrivalStatus(),
                ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? ""
            });

            VisitDataBridge.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "ÎöÎæÎÖÎºÎòÎ¿ ÎáÎ®Î×Î¿ ÎæÎöÎªÎ£ÎùÎö");
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
                UIHelper.ShowMessage(this, "ÎºÎòÎôÎØ Î£ÎùÎÑ ÎóÎ£ ÎøÎ¿ÎÿÎÖÎí ÎæÎÖÎºÎòÎ¿ ÎºÎÖÎÖÎØ Î×Î¬ÎòÎÜ ÎöÎÖÎòÎ×Î");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                UIHelper.ShowMessage(this, "ÎöÎæÎÖÎºÎòÎ¿ ÎøÎæÎ¿ ÎáÎíÎÆÎ¿ ÎòÎ£ÎÉ ÎáÎÖÎ¬Î Î£ÎóÎ¿ÎòÎÜ ÎÉÎòÎ¬Îò. ÎöÎñÎ¿ÎÿÎÖÎØ ÎáÎ®Î×Î¿ÎÖÎØ ÎæÎøÎ¿ÎÿÎÖÎí ÎöÎæÎÖÎºÎòÎ¿.");
                return;
            }

            if (!TryReadVisitFields(
                selectedVisit,
                out DateTime visitDate,
                out string chipNumber,
                out string reason,
                out string diagnosis,
                out double baseCost,
                out double totalCost,
                out Medication? newMedication,
                out int newMedicationQuantity))
            {
                return;
            }

            string selectedArrivalStatus = GetSelectedArrivalStatus();

            if (selectedArrivalStatus != "Scheduled" && !IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "ÎÉÎÖ ÎÉÎñÎ®Î¿ Î£ÎíÎ×Î ÎöÎÆÎÖÎó ÎÉÎò Î£ÎÉ ÎöÎÆÎÖÎó Î£ÎñÎáÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎöÎíÎ¬ÎÖÎÖÎ×Îö.");
                SetValidationMessage("ÎíÎÿÎÿÎòÎí ÎöÎÆÎóÎö ÎáÎÖÎ¬Î Î£ÎóÎôÎøÎòÎ Î¿Îº ÎÉÎùÎ¿ÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎóÎæÎ¿Îö", isValid: false);
                return;
            }

            RestoreMedicationStock(selectedVisit);

            if (newMedication != null && newMedicationQuantity > 0)
                newMedication.StockQuantity -= newMedicationQuantity;

            selectedVisit.AnimalChipNumber = chipNumber;
            selectedVisit.VisitDate = visitDate;
            selectedVisit.Reason = reason;
            selectedVisit.Diagnosis = diagnosis;
            selectedVisit.BaseCost = baseCost;
            selectedVisit.MedicationName = newMedication?.Name ?? "";
            selectedVisit.MedicationQuantity = newMedicationQuantity;
            selectedVisit.TotalCost = totalCost;
            selectedVisit.ArrivalStatus = selectedArrivalStatus;
            selectedVisit.ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? "";

            VisitDataBridge.SaveMedicationsToDatabase();
            VisitDataBridge.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "ÎöÎæÎÖÎºÎòÎ¿ ÎóÎòÎôÎøÎ ÎæÎöÎªÎ£ÎùÎö");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private async void DeleteVisit_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "ÎºÎòÎôÎØ Î£ÎùÎÑ ÎóÎ£ ÎøÎ¿ÎÿÎÖÎí ÎæÎÖÎºÎòÎ¿ ÎºÎÖÎÖÎØ Î×Î¬ÎòÎÜ ÎöÎÖÎòÎ×Î");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                UIHelper.ShowMessage(this, "ÎöÎæÎÖÎºÎòÎ¿ ÎøÎæÎ¿ ÎáÎíÎÆÎ¿ ÎòÎ£ÎÉ ÎáÎÖÎ¬Î Î£Î×ÎùÎòÎº ÎÉÎòÎ¬Îò ÎøÎôÎÖ Î£Î®Î×ÎòÎ¿ ÎöÎÖÎíÎÿÎòÎ¿ÎÖÎÖÎ¬ ÎÿÎÖÎñÎòÎ£.");
                return;
            }

            bool confirmed = await UIHelper.ShowConfirmation(
                this,
                "ÎöÎÉÎØ ÎÉÎ¬Îö ÎæÎÿÎòÎù Î®ÎæÎ¿ÎªÎòÎáÎÜ Î£Î×ÎùÎòÎº ÎÉÎ¬ ÎöÎæÎÖÎºÎòÎ¿ Î®ÎáÎæÎùÎ¿? ÎñÎóÎòÎ£Îö ÎûÎò Î£ÎÉ ÎáÎÖÎ¬ÎáÎ¬ Î£ÎæÎÖÎÿÎòÎ£.");

            if (!confirmed)
                return;

            RestoreMedicationStock(selectedVisit);
            VisitDataBridge.Visits.Remove(selectedVisit);
            VisitDataBridge.SaveMedicationsToDatabase();
            VisitDataBridge.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "ÎöÎæÎÖÎºÎòÎ¿ ÎáÎ×ÎùÎº ÎæÎöÎªÎ£ÎùÎö");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private void MarkArrived_Click(object? sender, RoutedEventArgs e)
        {
            if (selectedVisit == null)
            {
                UIHelper.ShowMessage(this, "ÎºÎòÎôÎØ Î£ÎùÎÑ ÎóÎ£ ÎøÎ¿ÎÿÎÖÎí ÎæÎÖÎºÎòÎ¿ ÎºÎÖÎÖÎØ Î×Î¬ÎòÎÜ ÎöÎÖÎòÎ×Î");
                return;
            }

            if (!CanRecordArrivalForSelectedVisit("ÎÉÎñÎ®Î¿ Î£ÎÉÎ®Î¿ ÎöÎÆÎóÎö Î¿Îº ÎÉÎùÎ¿ÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎöÎíÎ¬ÎÖÎÖÎ×Îö."))
                return;

            selectedVisit.ArrivalStatus = "Arrived";
            selectedVisit.ArrivalNote = ArrivalNoteInput.Text?.Trim() ?? "";
            VisitDataBridge.SaveVisitsToDatabase();

            SelectArrivalStatus("Arrived");
            UIHelper.ShowMessage(this, "ÎöÎÆÎóÎ¬ ÎöÎ£ÎºÎòÎù ÎÉÎòÎ®Î¿Îö ÎòÎáÎ®Î×Î¿Îö ÎóÎØ ÎöÎöÎóÎ¿Îö");
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
                UIHelper.ShowMessage(this, "ÎºÎòÎôÎØ Î£ÎùÎÑ ÎóÎ£ ÎøÎ¿ÎÿÎÖÎí ÎæÎÖÎºÎòÎ¿ ÎºÎÖÎÖÎØ Î×Î¬ÎòÎÜ ÎöÎÖÎòÎ×Î");
                return;
            }

            if (!CanRecordArrivalForSelectedVisit("ÎÉÎñÎ®Î¿ Î£ÎíÎ×Î Î£ÎÉ ÎöÎÆÎÖÎó Î¿Îº ÎÉÎùÎ¿ÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎöÎíÎ¬ÎÖÎÖÎ×Îö."))
                return;

            if (!TryGetVisitDate(out DateTime newVisitDate))
                return;

            if (IsVisitDateTimeInPast(newVisitDate))
            {
                UIHelper.ShowMessage(this, "Î£Î¬ÎòÎ¿ ÎùÎôÎ® ÎÉÎùÎ¿ÎÖ ÎÉÎÖ ÎöÎÆÎóÎö ÎÖÎ® Î£ÎæÎùÎòÎ¿ Î¬ÎÉÎ¿ÎÖÎÜ ÎòÎ®ÎóÎö ÎóÎ¬ÎÖÎôÎÖÎÖÎØ");
                return;
            }

            string note = ArrivalNoteInput.Text?.Trim() ?? "";
            string originalMedicationName = selectedVisit.MedicationName;
            int originalMedicationQuantity = selectedVisit.MedicationQuantity;
            double originalTotalCost = selectedVisit.TotalCost;

            Medication? reservedMedication = null;

            if (!string.IsNullOrWhiteSpace(originalMedicationName) && originalMedicationQuantity > 0)
            {
                reservedMedication = VisitDataBridge.Medications.FirstOrDefault(m => m.Name == originalMedicationName);

                if (reservedMedication == null)
                {
                    UIHelper.ShowMessage(this, "Î£ÎÉ ÎáÎÖÎ¬Î Î£ÎºÎæÎòÎó Î¬ÎòÎ¿ ÎùÎôÎ® ÎøÎÖ ÎöÎ¬Î¿ÎòÎñÎö Î®Î£ ÎöÎ¬ÎòÎ¿ ÎöÎÖÎ®Î ÎøÎæÎ¿ Î£ÎÉ ÎºÎÖÎÖÎ×Î¬ ÎæÎ×Î£ÎÉÎÖ.");
                    return;
                }

                int availableAfterCancel = reservedMedication.StockQuantity + originalMedicationQuantity;

                if (originalMedicationQuantity > availableAfterCancel)
                {
                    UIHelper.ShowMessage(this, $"ÎÉÎÖÎ Î×ÎíÎñÎÖÎº Î×Î£ÎÉÎÖ Î£ÎºÎæÎÖÎóÎ¬ Î¬ÎòÎ¿ ÎùÎôÎ® ÎóÎØ ÎÉÎòÎ¬Îö Î¬Î¿ÎòÎñÎö. ÎªÎ¿ÎÖÎÜ {originalMedicationQuantity}, ÎûÎ×ÎÖÎ {availableAfterCancel}.");
                    return;
                }
            }

            selectedVisit.ArrivalStatus = "NoShow";
            selectedVisit.ArrivalNote = string.IsNullOrWhiteSpace(note)
                ? "ÎöÎ£ÎºÎòÎù Î£ÎÉ ÎöÎÆÎÖÎó Î£Î¬ÎòÎ¿"
                : note;

            RestoreMedicationStock(selectedVisit);
            selectedVisit.MedicationName = "";
            selectedVisit.MedicationQuantity = 0;
            selectedVisit.TotalCost = selectedVisit.BaseCost;

            if (reservedMedication != null && originalMedicationQuantity > 0)
                reservedMedication.StockQuantity -= originalMedicationQuantity;

            VisitDataBridge.Visits.Add(new Visit
            {
                AnimalChipNumber = selectedVisit.AnimalChipNumber,
                VisitDate = newVisitDate,
                Reason = selectedVisit.Reason,
                Symptoms = "",
                Diagnosis = selectedVisit.Diagnosis,
                VeterinarianName = "",
                BaseCost = selectedVisit.BaseCost,
                MedicationName = originalMedicationName,
                MedicationQuantity = originalMedicationQuantity,
                TotalCost = originalTotalCost,
                ArrivalStatus = "Scheduled",
                ArrivalNote = $"Î¬ÎòÎ¿ ÎùÎôÎ® ÎæÎóÎºÎæÎòÎ¬ ÎÉÎÖ ÎöÎÆÎóÎö. ÎöÎóÎ¿Îö ÎºÎòÎôÎ×Î¬: {selectedVisit.ArrivalNote}"
            });

            VisitDataBridge.SaveMedicationsToDatabase();
            VisitDataBridge.SaveVisitsToDatabase();
            UIHelper.ShowMessage(this, "ÎíÎòÎ×Î Î®Î£ÎÉ ÎöÎÆÎÖÎó ÎòÎáÎºÎæÎó Î¬ÎòÎ¿ ÎùÎôÎ® Î£ÎñÎÖ ÎöÎ¬ÎÉÎ¿ÎÖÎÜ ÎòÎöÎ®ÎóÎö Î®ÎæÎÿÎòÎñÎí");
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
            SetValidationMessage("ÎñÎóÎòÎ£Î¬ ÎöÎÆÎóÎö Î×ÎòÎ¬Î¿Î¬ Î¿Îº ÎÉÎùÎ¿ÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎóÎæÎ¿Îö", isValid: false);
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

        private bool TryCalculateTotalCost(
            out double totalCost,
            out Medication? selectedMedication,
            out int medicationQuantity,
            Visit? visitBeingUpdated = null)
        {
            totalCost = 0;
            selectedMedication = null;
            medicationQuantity = 0;

            string baseCostText = BaseCostInput.Text?.Trim() ?? "";

            if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
            {
                UIHelper.ShowMessage(this, "Î×ÎùÎÖÎ¿ ÎæÎÖÎºÎòÎ¿ ÎùÎÖÎÖÎæ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿");
                return false;
            }

            if (!UiFormValidation.IsValidMoney(baseCost))
            {
                UIHelper.ShowMessage(this, "Î×ÎùÎÖÎ¿ ÎæÎÖÎºÎòÎ¿ Î£ÎÉ ÎÖÎøÎòÎ£ Î£ÎöÎÖÎòÎ¬ Î®Î£ÎÖÎ£ÎÖ");
                return false;
            }

            totalCost = baseCost;

            if (!IsMedicationTreatmentSelected())
                return true;

            string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(medicationName) || medicationName == "Î£Î£ÎÉ Î¬Î¿ÎòÎñÎö")
            {
                UIHelper.ShowMessage(this, "ÎæÎùÎ¿ Î¬Î¿ÎòÎñÎö ÎÉÎò Î®ÎáÎö ÎÉÎ¬ ÎíÎòÎÆ ÎöÎÿÎÖÎñÎòÎ£ Î£ÎæÎ£ÎÖ Î¬Î¿ÎòÎñÎö");
                return false;
            }

            if (string.IsNullOrWhiteSpace(MedicationQuantityInput.Text))
            {
                UIHelper.ShowMessage(this, "ÎÖÎ® Î£ÎöÎûÎÖÎ ÎøÎ×ÎòÎ¬ Î¬Î¿ÎòÎñÎö");
                return false;
            }

            selectedMedication = VisitDataBridge.Medications.FirstOrDefault(m => m.Name == medicationName);

            if (selectedMedication == null)
            {
                UIHelper.ShowMessage(this, "ÎöÎ¬Î¿ÎòÎñÎö Î®ÎáÎæÎùÎ¿Îö Î£ÎÉ ÎºÎÖÎÖÎ×Î¬ ÎæÎ×Î£ÎÉÎÖ");
                return false;
            }

            if (!int.TryParse(MedicationQuantityInput.Text?.Trim(), out medicationQuantity))
            {
                UIHelper.ShowMessage(this, "ÎøÎ×ÎòÎ¬ Î¬Î¿ÎòÎñÎö ÎùÎÖÎÖÎæÎ¬ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿ Î®Î£ÎØ");
                return false;
            }

            if (medicationQuantity <= 0)
            {
                UIHelper.ShowMessage(this, "ÎøÎ×ÎòÎ¬ Î¬Î¿ÎòÎñÎö ÎùÎÖÎÖÎæÎ¬ Î£ÎöÎÖÎòÎ¬ ÎÆÎôÎòÎ£Îö Î×ÎÉÎñÎí");
                return false;
            }

            int availableStock = selectedMedication.StockQuantity;

            if (visitBeingUpdated != null &&
                visitBeingUpdated.MedicationName == selectedMedication.Name)
            {
                availableStock += visitBeingUpdated.MedicationQuantity;
            }

            if (medicationQuantity > availableStock)
            {
                UIHelper.ShowMessage(this, $"ÎÉÎÖÎ Î×ÎíÎñÎÖÎº Î×Î£ÎÉÎÖ Î£Î¬Î¿ÎòÎñÎö Î®ÎáÎæÎùÎ¿Îö. ÎæÎÖÎºÎ®Î¬ {medicationQuantity}, ÎÉÎæÎ£ ÎÖÎ® ÎæÎ×Î£ÎÉÎÖ {availableStock}.");
                return false;
            }

            totalCost += selectedMedication.UnitPrice * medicationQuantity;
            return true;
        }

        private bool TryReadVisitFields(
            Visit? visitBeingUpdated,
            out DateTime visitDate,
            out string chipNumber,
            out string reason,
            out string diagnosis,
            out double baseCost,
            out double totalCost,
            out Medication? medication,
            out int medicationQuantity)
        {
            chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            reason = ReasonInput.Text?.Trim() ?? "";
            diagnosis = DiagnosisInput.Text?.Trim() ?? "";
            baseCost = 0;
            totalCost = 0;
            medication = null;
            medicationQuantity = 0;

            string selectedChipNumber = chipNumber;
            var animal = VisitDataBridge.Animals.FirstOrDefault(a => a.ChipNumber == selectedChipNumber);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "ÎÖÎ® Î£ÎæÎùÎòÎ¿ ÎùÎÖÎö ÎºÎÖÎÖÎ×Î¬ Î£ÎñÎáÎÖ ÎóÎôÎøÎòÎ ÎæÎÖÎºÎòÎ¿");
                visitDate = DateTime.Today;
                return false;
            }

            if (!TryGetVisitDate(out visitDate))
                return false;

            if (!UiFormValidation.IsValidVisitDate(visitDate))
            {
                UIHelper.ShowMessage(this, "Î¬ÎÉÎ¿ÎÖÎÜ ÎæÎÖÎºÎòÎ¿ Î£ÎÉ Î¬ÎºÎÖÎ");
                return false;
            }

            if (IsVisitDateTimeInPast(visitDate))
            {
                UIHelper.ShowMessage(this, "Î£ÎÉ ÎáÎÖÎ¬Î Î£ÎóÎôÎøÎ ÎæÎÖÎºÎòÎ¿ Î£Î¬ÎÉÎ¿ÎÖÎÜ ÎÉÎò Î®ÎóÎö Î®ÎøÎæÎ¿ ÎóÎæÎ¿Îò");
                SetValidationMessage("ÎæÎùÎ¿ Î¬ÎÉÎ¿ÎÖÎÜ ÎòÎ®ÎóÎö ÎóÎ¬ÎÖÎôÎÖÎÖÎØ ÎÉÎò ÎÉÎ¬ ÎöÎôÎºÎö ÎöÎáÎòÎøÎùÎÖÎ¬", isValid: false);
                return false;
            }

            if (!UiFormValidation.IsRequiredText(reason) ||
                !UiFormValidation.IsRequiredText(diagnosis))
            {
                UIHelper.ShowMessage(this, "ÎÖÎ® Î£Î×Î£ÎÉ ÎíÎÖÎæÎ¬ ÎæÎÖÎºÎòÎ¿ ÎòÎÉÎæÎùÎáÎö / ÎÿÎÖÎñÎòÎ£");
                return false;
            }

            if (!double.TryParse(BaseCostInput.Text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out baseCost))
            {
                UIHelper.ShowMessage(this, "Î×ÎùÎÖÎ¿ ÎæÎÖÎºÎòÎ¿ ÎùÎÖÎÖÎæ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿");
                return false;
            }

            if (!TryCalculateTotalCost(out totalCost, out medication, out medicationQuantity, visitBeingUpdated))
                return false;

            return true;
        }

        private void RestoreMedicationStock(Visit visit)
        {
            if (string.IsNullOrWhiteSpace(visit.MedicationName) || visit.MedicationQuantity <= 0)
                return;

            var medication = VisitDataBridge.Medications.FirstOrDefault(m => m.Name == visit.MedicationName);

            if (medication != null)
                medication.StockQuantity += visit.MedicationQuantity;
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
            VisitsJournalTitleText.Text = "ÎÖÎòÎ×Î ÎæÎÖÎºÎòÎ¿ÎÖÎØ";
            VisitsJournalSubtitleText.Text = "Î¿Î®ÎÖÎ×Î¬ ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î×ÎòÎ¿ÎÖÎØ Î£ÎñÎÖ Î¬ÎÉÎ¿ÎÖÎÜ, ÎùÎÖÎö, ÎÉÎæÎùÎáÎö ÎòÎóÎ£ÎòÎ¬";
            AnimalQuickCardsTitleText.Text = "ÎæÎùÎ¿ ÎíÎòÎÆ ÎùÎÖÎö Î£ÎñÎ¬ÎÖÎùÎ¬ Î¿Î®ÎÖÎ×Î¬ ÎöÎùÎÖÎòÎ¬";
            RefreshVisitsList();
        }

        private void ShowUpcomingVisits_Click(object? sender, RoutedEventArgs e)
        {
            showUpcomingVisitsOnly = true;
            selectedSpeciesFilter = "";
            selectedAnimalChipNumber = "";
            selectedVisit = null;
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "Î¬ÎòÎ¿ÎÖÎØ ÎóÎ¬ÎÖÎôÎÖÎÖÎØ";
            VisitsJournalSubtitleText.Text = "Î¿Î®ÎÖÎ×Î¬ ÎöÎ¬ÎòÎ¿ÎÖÎØ Î®ÎóÎòÎô Î£ÎÉ ÎöÎÆÎÖÎó ÎûÎ×ÎáÎØ, Î×ÎöÎºÎ¿ÎòÎæ Î£Î¿ÎùÎòÎº";
            AnimalQuickCardsTitleText.Text = "ÎæÎùÎ¿ ÎíÎòÎÆ ÎùÎÖÎö ÎÉÎò Î£ÎùÎÑ ÎóÎ£ ÎøÎ¿ÎÿÎÖÎí Î¬ÎòÎ¿ ÎóÎ¬ÎÖÎôÎÖ";
            UpdateArrivalActionsAvailability();
            UpdateSaveButtonMode();
            UpdateVisitEditingState();
            RefreshVisitsList();
        }

        private void FilterDogs_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("ÎøÎ£Îæ");
        }

        private void FilterCats_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("ÎùÎ¬ÎòÎ£");
        }

        private void FilterReptiles_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("ÎûÎòÎùÎ£");
        }

        private void FilterBirds_Click(object? sender, RoutedEventArgs e)
        {
            ShowSpeciesAnimalList("ÎªÎÖÎñÎòÎ¿");
        }

        private void ShowSpeciesAnimalList(string species)
        {
            showUpcomingVisitsOnly = false;
            selectedSpeciesFilter = species;
            selectedAnimalChipNumber = "";
            selectedVisit = null;
            SelectedAnimalPanel.IsVisible = false;
            VisitsJournalTitleText.Text = "ÎÖÎòÎ×Î ÎæÎÖÎºÎòÎ¿ÎÖÎØ";
            VisitsJournalSubtitleText.Text = "ÎæÎùÎ¿ ÎùÎÖÎö Î×Î¬ÎòÎÜ ÎöÎ¿Î®ÎÖÎ×Îö ÎøÎôÎÖ Î£ÎñÎ¬ÎòÎù ÎÉÎ¬ Î¬ÎÖÎº ÎöÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î£Îö";
            AnimalQuickCardsTitleText.Text = $"Î¿Î®ÎÖÎ×Î¬ {GetSpeciesPluralText(species)}";
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
            VisitsJournalTitleText.Text = "ÎÖÎòÎ×Î ÎæÎÖÎºÎòÎ¿ÎÖÎØ";
            VisitsJournalSubtitleText.Text = "Î¿Î®ÎÖÎ×Î¬ ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î×ÎòÎ¿ÎÖÎØ Î£ÎñÎÖ Î¬ÎÉÎ¿ÎÖÎÜ, ÎùÎÖÎö, ÎÉÎæÎùÎáÎö ÎòÎóÎ£ÎòÎ¬";
            VisitDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
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
            TotalCostText.Text = "ÎóÎ£ÎòÎ¬ ÎøÎòÎ£Î£Î¬: 0";
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
                SetValidationMessage("Î£ÎÉ ÎáÎÖÎ¬Î Î£Î®Î×ÎòÎ¿ ÎæÎÖÎºÎòÎ¿ ÎæÎ¬ÎÉÎ¿ÎÖÎÜ ÎÉÎò Î®ÎóÎö Î®ÎøÎæÎ¿ ÎóÎæÎ¿Îò", isValid: false);
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
                ? "ÎÉÎñÎ®Î¿ Î£ÎóÎôÎøÎ ÎöÎÆÎóÎö ÎøÎÖ ÎûÎ×Î ÎöÎ¬ÎòÎ¿ ÎøÎæÎ¿ ÎóÎæÎ¿"
                : "ÎÉÎñÎ®Î¿ Î£ÎóÎôÎøÎ ÎöÎÆÎóÎö Î¿Îº ÎÉÎùÎ¿ÎÖ Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ ÎöÎíÎ¬ÎÖÎÖÎ×Îö";

            ToolTip.SetTip(MarkArrivedButton, tip);
            ToolTip.SetTip(MarkNoShowButton, tip);
        }

        private bool IsVisitClosed(Visit visit)
        {
            return visit.ArrivalStatus is "Arrived" or "NoShow";
        }

        private void RefreshMedicationDropdown()
        {
            var items = VisitDataBridge.Medications
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
                MedicationStockText.Text = "ÎÉÎÖÎ Î¬Î¿ÎòÎñÎòÎ¬ ÎûÎ×ÎÖÎáÎòÎ¬ ÎæÎ×Î£ÎÉÎÖ";
                return;
            }

            var medication = VisitDataBridge.Medications.FirstOrDefault(m => m.Name == medicationName);

            if (medication == null)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = "ÎöÎ¬Î¿ÎòÎñÎö Î®ÎáÎæÎùÎ¿Îö Î£ÎÉ ÎºÎÖÎÖÎ×Î¬ ÎæÎ×Î£ÎÉÎÖ";
                return;
            }

            int availableStock = GetAvailableMedicationStock(medication);
            string quantityText = MedicationQuantityInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(quantityText))
            {
                MedicationStockText.Foreground = availableStock <= 5 ? Brushes.DarkOrange : Brushes.ForestGreen;
                MedicationStockText.Text = $"ÎæÎ×Î£ÎÉÎÖ ÎóÎøÎ®ÎÖÎò: {availableStock} ÎÖÎùÎÖÎôÎòÎ¬";
                return;
            }

            if (!int.TryParse(quantityText, out int requestedQuantity) || requestedQuantity <= 0)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = $"ÎæÎ×Î£ÎÉÎÖ ÎóÎøÎ®ÎÖÎò: {availableStock}. ÎöÎøÎ×ÎòÎ¬ ÎùÎÖÎÖÎæÎ¬ Î£ÎöÎÖÎòÎ¬ Î×ÎíÎñÎ¿ ÎùÎÖÎòÎæÎÖ";
                return;
            }

            int remainingStock = availableStock - requestedQuantity;

            if (remainingStock < 0)
            {
                MedicationStockText.Foreground = Brushes.Firebrick;
                MedicationStockText.Text = $"ÎÉÎÖÎ Î×ÎíÎñÎÖÎº Î×Î£ÎÉÎÖ: ÎæÎÖÎºÎ®Î¬ {requestedQuantity}, ÎûÎ×ÎÖÎ {availableStock}";
                return;
            }

            MedicationStockText.Foreground = remainingStock <= 5 ? Brushes.DarkOrange : Brushes.ForestGreen;
            MedicationStockText.Text = $"ÎæÎ×Î£ÎÉÎÖ ÎóÎøÎ®ÎÖÎò: {availableStock} | ÎÉÎùÎ¿ÎÖ Î®Î×ÎÖÎ¿Îö ÎÖÎÖÎ®ÎÉÎ¿: {remainingStock}";
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
                VisitDetailsText.Text = $"ÎæÎùÎ¿ ÎùÎÖÎö Î×Î¬ÎòÎÜ Î¿Î®ÎÖÎ×Î¬ {GetSpeciesPluralText(selectedSpeciesFilter)} ÎøÎôÎÖ Î£Î¿ÎÉÎòÎ¬ ÎÉÎ¬ ÎöÎ¬ÎòÎ¿ÎÖÎØ Î®Î£Îö";
                return;
            }

            IEnumerable<Visit> visitsToShow = string.IsNullOrWhiteSpace(selectedAnimalChipNumber)
                ? VisitDataBridge.Visits
                : VisitDataBridge.Visits.Where(visit => visit.AnimalChipNumber == selectedAnimalChipNumber);
            var now = DateTime.Now;

            if (showUpcomingVisitsOnly)
            {
                visitsToShow = visitsToShow.Where(visit =>
                    visit.ArrivalStatus == "Scheduled" &&
                    visit.VisitDate >= now);
            }

            var visitsList = visitsToShow.ToList();

            if (VisitDataBridge.Visits.Count == 0)
            {
                VisitDetailsText.Text = "ÎÉÎÖÎ ÎæÎÖÎºÎòÎ¿ÎÖÎØ ÎæÎ×ÎóÎ¿ÎøÎ¬";
                return;
            }

            if (visitsList.Count == 0)
            {
                VisitDetailsText.Text = showUpcomingVisitsOnly
                    ? "ÎÉÎÖÎ Î¬ÎòÎ¿ÎÖÎØ ÎóÎ¬ÎÖÎôÎÖÎÖÎØ ÎøÎ¿ÎÆÎó"
                    : "ÎÉÎÖÎ ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î×ÎòÎ¿ÎÖÎØ Î£ÎùÎÖÎö Î®ÎáÎæÎùÎ¿Îö";
                return;
            }

            VisitDetailsText.Text = selectedVisit != null
                ? BuildVisitDetailsText(selectedVisit)
                : showUpcomingVisitsOnly
                    ? "Î¬ÎòÎ¿ÎÖÎØ ÎóÎ¬ÎÖÎôÎÖÎÖÎØ Î×ÎòÎªÎÆÎÖÎØ ÎøÎÉÎ Î×ÎöÎºÎ¿ÎòÎæ Î£Î¿ÎùÎòÎº"
                : string.IsNullOrWhiteSpace(selectedAnimalChipNumber)
                    ? "ÎæÎÖÎºÎòÎ¿ÎÖÎØ ÎóÎ¬ÎÖÎôÎÖÎÖÎØ Î×ÎòÎªÎÆÎÖÎØ ÎºÎòÎôÎØ Î×ÎöÎºÎ¿ÎòÎæ Î£Î¿ÎùÎòÎº. ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®ÎóÎæÎ¿Îò Î×ÎòÎñÎÖÎóÎÖÎØ ÎÉÎùÎ¿ÎÖÎöÎØ ÎøÎöÎÖÎíÎÿÎòÎ¿ÎÖÎö."
                    : "Î×ÎòÎªÎÆÎÖÎØ Î¿Îº ÎöÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î£ ÎöÎùÎÖÎö Î®ÎáÎæÎùÎ¿Îö";

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
                    Text = "ÎæÎùÎ¿ ÎíÎòÎÆ ÎùÎÖÎö Î×ÎöÎøÎñÎ¬ÎòÎ¿ÎÖÎØ ÎæÎªÎô ÎøÎôÎÖ Î£Î¿ÎÉÎòÎ¬ ÎÉÎ¬ ÎöÎùÎÖÎòÎ¬",
                    Foreground = new SolidColorBrush(Color.Parse("#526172")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8)
                });
                return;
            }

            if (VisitDataBridge.Animals.Count == 0)
            {
                AnimalQuickCardsPanel.Children.Add(new TextBlock
                {
                    Text = "ÎÉÎÖÎ ÎóÎôÎÖÎÖÎ ÎæÎóÎ£ÎÖ ÎùÎÖÎÖÎØ ÎæÎ×ÎóÎ¿ÎøÎ¬",
                    Foreground = new SolidColorBrush(Color.Parse("#526172")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8)
                });
                return;
            }

            var animalsBySpecies = VisitDataBridge.Animals
                .Where(animal => SpeciesMatchesFilter(animal.Species, selectedSpeciesFilter))
                .OrderBy(animal => animal.Name)
                .ToList();

            if (animalsBySpecies.Count == 0)
            {
                AnimalQuickCardsPanel.Children.Add(new TextBlock
                {
                    Text = $"ÎÉÎÖÎ {GetSpeciesPluralText(selectedSpeciesFilter)} ÎæÎ×ÎóÎ¿ÎøÎ¬",
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
                "ÎøÎ£Îæ" => animalSpecies is "ÎøÎ£Îæ" or "Dog",
                "ÎùÎ¬ÎòÎ£" => animalSpecies is "ÎùÎ¬ÎòÎ£" or "Cat",
                "ÎûÎòÎùÎ£" => animalSpecies is "ÎûÎòÎùÎ£" or "Reptile",
                "ÎªÎÖÎñÎòÎ¿" => animalSpecies is "ÎªÎÖÎñÎòÎ¿" or "Bird",
                _ => true
            };
        }

        private string GetSpeciesPluralText(string species)
        {
            return species switch
            {
                "ÎøÎ£Îæ" => "ÎøÎ£ÎæÎÖÎØ",
                "ÎùÎ¬ÎòÎ£" => "ÎùÎ¬ÎòÎ£ÎÖÎØ",
                "ÎûÎòÎùÎ£" => "ÎûÎòÎùÎ£ÎÖÎØ",
                "ÎªÎÖÎñÎòÎ¿" => "ÎªÎÖÎñÎòÎ¿ÎÖÎØ",
                _ => "ÎæÎóÎ£ÎÖ ÎùÎÖÎÖÎØ"
            };
        }

        private Button CreateAnimalQuickCard(Animal animal)
        {
            bool isSelected = selectedAnimalChipNumber == animal.ChipNumber;
            int visitsCount = VisitDataBridge.Visits.Count(visit => visit.AnimalChipNumber == animal.ChipNumber);
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
                            Text = $"{visitsCount} ÎæÎÖÎºÎòÎ¿ÎÖÎØ",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#526172")),
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            };

            ToolTip.SetTip(card, $"ÎñÎ¬Îù Î¬ÎÖÎº ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î£ {animal.Name}");

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
            var animal = VisitDataBridge.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);
            string animalName = animal != null ? animal.Name : visit.AnimalChipNumber;
            string animalSpecies = animal?.Species ?? "";
            string accentColor = GetVisitAnimalAccentColor(animalSpecies);
            string strongColor = GetVisitAnimalStrongColor(animalSpecies);
            string medicationText = string.IsNullOrWhiteSpace(visit.MedicationName)
                ? "Î£Î£ÎÉ Î¬Î¿ÎòÎñÎö"
                : $"{visit.MedicationName} x {visit.MedicationQuantity}";
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
                                        Text = $"{GetVisitAnimalIcon(animalSpecies)} ­®¦",
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
                                        Text = $"ÎóÎ£ÎòÎ¬: {visit.TotalCost:0.00}",
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
            VisitDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
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
            TotalCostText.Text = "ÎóÎ£ÎòÎ¬ ÎøÎòÎ£Î£Î¬: 0";
            ValidationText.Text = "";
            RefreshVisitsList();
        }

        private string GetSelectedVisitHelpText(Visit visit)
        {
            if (IsVisitClosed(visit))
                return "ÎöÎæÎÖÎºÎòÎ¿ ÎíÎÆÎòÎ¿ ÎòÎáÎ®Î×Î¿ ÎæÎøÎ¿ÎÿÎÖÎí. ÎÉÎñÎ®Î¿ Î£ÎªÎñÎòÎ¬ ÎæÎñÎ¿ÎÿÎÖÎØ ÎÉÎÜ Î£ÎÉ Î£ÎóÎ¿ÎòÎÜ ÎÉÎòÎ¬Îò";

            if (IsVisitDateTimeInPast(visit.VisitDate))
                return "ÎöÎ¬ÎòÎ¿ ÎóÎæÎ¿. ÎÉÎñÎ®Î¿ Î£ÎÉÎ®Î¿ ÎöÎÆÎóÎö ÎÉÎò Î£ÎíÎ×Î Î£ÎÉ ÎöÎÆÎÖÎó";

            return "ÎöÎæÎÖÎºÎòÎ¿ ÎáÎÿÎóÎ. ÎÉÎñÎ®Î¿ Î£ÎóÎôÎøÎ ÎÉÎòÎ¬Îò ÎóÎô Î®Î®ÎóÎ¬ ÎöÎ¬ÎòÎ¿ Î¬ÎóÎæÎòÎ¿";
        }

        private void UpdateSaveButtonMode()
        {
            if (selectedVisit == null)
            {
                SaveVisitButton.Content = "Î®Î×ÎòÎ¿ ÎæÎÖÎºÎòÎ¿";
                SaveVisitButton.IsEnabled = true;
                DeleteVisitButton.IsEnabled = false;
                ToolTip.SetTip(SaveVisitButton, "Î®ÎòÎ×Î¿ ÎæÎÖÎºÎòÎ¿ ÎùÎôÎ®");
                ToolTip.SetTip(DeleteVisitButton, "Î×ÎùÎÖÎºÎö ÎûÎ×ÎÖÎáÎö Î¿Îº ÎÉÎùÎ¿ÎÖ ÎæÎùÎÖÎ¿Î¬ ÎæÎÖÎºÎòÎ¿ ÎñÎ¬ÎòÎù");
                return;
            }

            if (IsVisitClosed(selectedVisit))
            {
                SaveVisitButton.Content = "ÎæÎÖÎºÎòÎ¿ ÎíÎÆÎòÎ¿";
                SaveVisitButton.IsEnabled = false;
                DeleteVisitButton.IsEnabled = false;
                ToolTip.SetTip(SaveVisitButton, "ÎöÎæÎÖÎºÎòÎ¿ ÎáÎíÎÆÎ¿ ÎòÎáÎ®Î×Î¿ ÎæÎøÎ¿ÎÿÎÖÎí, ÎÉÎÖ ÎÉÎñÎ®Î¿ Î£ÎóÎ¿ÎòÎÜ ÎÉÎòÎ¬Îò");
                ToolTip.SetTip(DeleteVisitButton, "ÎæÎÖÎºÎòÎ¿ ÎíÎÆÎòÎ¿ Î£ÎÉ ÎáÎÖÎ¬Î Î£Î×ÎùÎÖÎºÎö");
                return;
            }

            SaveVisitButton.Content = "ÎóÎôÎøÎ ÎæÎÖÎºÎòÎ¿";
            SaveVisitButton.IsEnabled = true;
            DeleteVisitButton.IsEnabled = true;
            ToolTip.SetTip(SaveVisitButton, "Î×ÎóÎôÎøÎ ÎÉÎ¬ ÎöÎæÎÖÎºÎòÎ¿ Î®ÎáÎæÎùÎ¿");
            ToolTip.SetTip(DeleteVisitButton, "Î×ÎòÎùÎº ÎæÎÖÎºÎòÎ¿ ÎñÎ¬ÎòÎù ÎÉÎùÎ¿ÎÖ ÎÉÎÖÎ®ÎòÎ¿");
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
            BaseCostInput.IsEnabled = canEdit;
            TreatmentModeDropdown.IsEnabled = canEdit;
            MedicationDropdown.IsEnabled = canEdit;
            MedicationQuantityInput.IsEnabled = canEdit;
            ArrivalStatusDropdown.IsEnabled = canEdit;
            ArrivalNoteInput.IsEnabled = canEdit;
        }

        private void FillVisitFields(Visit visit)
        {
            var animal = VisitDataBridge.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);

            AnimalSearchInput.Text = animal?.Name ?? visit.AnimalChipNumber;
            AnimalChipInput.Text = visit.AnimalChipNumber;
            if (animal != null)
                ShowSelectedAnimalCard(animal);
            else
                SelectedAnimalPanel.IsVisible = false;
            VisitDatePicker.SelectedDate = new DateTimeOffset(visit.VisitDate);
            SetVisitTime(visit.VisitDate.TimeOfDay);
            ReasonInput.Text = visit.Reason;
            DiagnosisInput.Text = visit.Diagnosis;
            BaseCostInput.Text = visit.BaseCost.ToString(CultureInfo.InvariantCulture);
            bool hasMedication = !string.IsNullOrWhiteSpace(visit.MedicationName);
            TreatmentModeDropdown.SelectedIndex = hasMedication ? 1 : 0;
            MedicationFieldsPanel.IsVisible = hasMedication;
            MedicationQuantityInput.Text = visit.MedicationQuantity > 0
                ? visit.MedicationQuantity.ToString()
                : "";
            SelectMedication(visit.MedicationName);
            UpdateMedicationStockHint();
            SelectArrivalStatus(visit.ArrivalStatus);
            ArrivalNoteInput.Text = visit.ArrivalNote;
            TotalCostText.Text = $"ÎóÎ£ÎòÎ¬ ÎøÎòÎ£Î£Î¬: {visit.TotalCost:0.00}";
        }

        private void ShowSelectedAnimalCard(Animal animal)
        {
            var owner = VisitDataBridge.Clients.FirstOrDefault(client => client.NationalId == animal.OwnerIdNumber);
            bool vaccinationDue = UiFormValidation.IsVaccinationDue(animal.LastVaccinationDate);
            int visitsCount = VisitDataBridge.Visits.Count(visit => visit.AnimalChipNumber == animal.ChipNumber);
            DateTime? nextVisitDate = VisitDataBridge.Visits
                .Where(visit => visit.AnimalChipNumber == animal.ChipNumber &&
                    visit.ArrivalStatus == "Scheduled" &&
                    visit.VisitDate >= DateTime.Now)
                .OrderBy(visit => visit.VisitDate)
                .Select(visit => (DateTime?)visit.VisitDate)
                .FirstOrDefault();

            selectedAnimalChipNumber = animal.ChipNumber;
            VisitsJournalTitleText.Text = $"Î¬ÎÖÎº ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î£ {animal.Name}";
            VisitsJournalSubtitleText.Text = nextVisitDate.HasValue
                ? $"ÎøÎ£ ÎöÎ¬ÎòÎ¿ÎÖÎØ Î®Î£ ÎöÎùÎÖÎö ÎöÎûÎò. ÎöÎ¬ÎòÎ¿ ÎöÎºÎ¿ÎòÎæ: {nextVisitDate.Value:dd/MM/yyyy HH:mm}"
                : "ÎøÎ£ ÎöÎ¬ÎòÎ¿ÎÖÎØ Î®Î£ ÎöÎùÎÖÎö ÎöÎûÎò, ÎøÎòÎ£Î£ ÎóÎ¬ÎÖÎôÎÖÎÖÎØ ÎòÎöÎÖÎíÎÿÎòÎ¿ÎÖÎö";
            SelectedAnimalIconText.Text = GetVisitAnimalIcon(animal.Species);
            SelectedAnimalNameText.Text = animal.Name;
            SelectedAnimalChipText.Text = $"Î®ÎæÎæ: {animal.ChipNumber}";
            SelectedAnimalOwnerText.Text = $"ÎæÎóÎ£ÎÖÎØ: {(owner != null ? owner.FullName : animal.OwnerIdNumber)}";
            SelectedAnimalVaccineText.Text = vaccinationDue
                ? "ÎùÎÖÎíÎòÎ: ÎªÎ¿ÎÖÎÜ ÎùÎÖÎíÎòÎ Î®ÎáÎ¬ÎÖ"
                : "ÎùÎÖÎíÎòÎ: Î¬ÎºÎÖÎ";
            SelectedAnimalVisitsCountText.Text = $"ÎæÎÖÎºÎòÎ¿ÎÖÎØ Î®Î×ÎòÎ¿ÎÖÎØ: {visitsCount}";
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

                if (statusText.Contains("ÎöÎÆÎÖÎó") && !statusText.Contains("Î£ÎÉ"))
                    return "Arrived";

                if (statusText.Contains("Î£ÎÉ"))
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
            var animal = VisitDataBridge.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);
            var owner = animal == null
                ? null
                : VisitDataBridge.Clients.FirstOrDefault(c => c.NationalId == animal.OwnerIdNumber);

            string medicationText = string.IsNullOrWhiteSpace(visit.MedicationName)
                ? "Î£Î£ÎÉ Î¬Î¿ÎòÎñÎö"
                : $"{visit.MedicationName} x {visit.MedicationQuantity}";
            string statusText = GetVisitStatusText(visit.VisitDate);
            string arrivalStatusText = GetArrivalStatusText(visit.ArrivalStatus);

            return $"""
                ÎíÎÿÎÿÎòÎí: {statusText}
                ÎöÎÆÎóÎö: {arrivalStatusText}
                Î¬ÎÉÎ¿ÎÖÎÜ: {visit.VisitDate:dd/MM/yyyy HH:mm}
                ÎùÎÖÎö: {(animal != null ? animal.Name : visit.AnimalChipNumber)}
                Î×ÎíÎñÎ¿ Î®ÎæÎæ: {visit.AnimalChipNumber}
                ÎæÎóÎ£ÎÖÎØ: {(owner != null ? owner.FullName : "Î£ÎÉ ÎáÎ×ÎªÎÉ")}
                ÎíÎÖÎæÎ¬ ÎöÎÆÎóÎö: {visit.Reason}
                ÎÉÎæÎùÎáÎö / ÎÿÎÖÎñÎòÎ£: {visit.Diagnosis}
                Î¬Î¿ÎòÎñÎö: {medicationText}
                ÎóÎ£ÎòÎ¬ ÎæÎíÎÖÎíÎÖÎ¬: {visit.BaseCost:0.00}
                ÎóÎ£ÎòÎ¬ ÎøÎòÎ£Î£Î¬: {visit.TotalCost:0.00}
                ÎöÎóÎ¿Î¬ ÎòÎÿÎ¿ÎÖÎáÎ¿: {(string.IsNullOrWhiteSpace(visit.ArrivalNote) ? "ÎÉÎÖÎ ÎöÎóÎ¿Îö" : visit.ArrivalNote)}
                """;
        }

        private string GetDisplayVisitStatusText(Visit visit)
        {
            return visit.ArrivalStatus switch
            {
                "Arrived" => "ÎöÎÆÎÖÎó",
                "NoShow" => "Î£ÎÉ ÎöÎÆÎÖÎó",
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
                "Arrived" => "ÎöÎÆÎÖÎó",
                "NoShow" => "Î£ÎÉ ÎöÎÆÎÖÎó",
                _ => "Î×Î×Î¬ÎÖÎ Î£ÎöÎÆÎóÎö"
            };
        }

        private string GetVisitStatusText(DateTime visitDate)
        {
            DateTime now = DateTime.Now;

            if (visitDate < now)
                return "ÎóÎæÎ¿";

            if (visitDate.Date == now.Date)
                return "ÎöÎÖÎòÎØ";

            if (visitDate <= now.AddDays(7))
                return "ÎºÎ¿ÎòÎæ";

            return "ÎóÎ¬ÎÖÎôÎÖ";
        }

        private string GetVisitStatusColor(DateTime visitDate)
        {
            return GetVisitStatusText(visitDate) switch
            {
                "ÎóÎæÎ¿" => "#8A94A6",
                "ÎöÎÖÎòÎØ" => "#D97706",
                "ÎºÎ¿ÎòÎæ" => "#D64545",
                _ => "#0797C9"
            };
        }

        private string GetVisitAnimalIcon(string species)
        {
            return species switch
            {
                "ÎøÎ£Îæ" or "Dog" => "­ÉÂ",
                "ÎùÎ¬ÎòÎ£" or "Cat" => "­É¦",
                "ÎûÎòÎùÎ£" or "Reptile" => "­ªÄ",
                "ÎªÎÖÎñÎòÎ¿" or "Bird" => "­Éª",
                _ => "­É¥"
            };
        }

        private string GetVisitAnimalAccentColor(string species)
        {
            return species switch
            {
                "ÎøÎ£Îæ" or "Dog" => "#E9F8FC",
                "ÎùÎ¬ÎòÎ£" or "Cat" => "#FFF1D6",
                "ÎûÎòÎùÎ£" or "Reptile" => "#E5F6E8",
                "ÎªÎÖÎñÎòÎ¿" or "Bird" => "#E8ECFF",
                _ => "#F1F5F9"
            };
        }

        private string GetVisitAnimalStrongColor(string species)
        {
            return species switch
            {
                "ÎøÎ£Îæ" or "Dog" => "#0797C9",
                "ÎùÎ¬ÎòÎ£" or "Cat" => "#D9822B",
                "ÎûÎòÎùÎ£" or "Reptile" => "#2E9D59",
                "ÎªÎÖÎñÎòÎ¿" or "Bird" => "#5865C7",
                _ => "#476A88"
            };
        }
    }
}
