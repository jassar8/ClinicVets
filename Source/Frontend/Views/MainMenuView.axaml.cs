using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Views
{
    public partial class MainMenuView : UserControl
    {
        private readonly Employee currentEmployee;

        public Action? OpenClients;
        public Action? OpenAnimals;
        public Action? OpenVisits;
        public Action? OpenMedicines;
        public Action? Logout;
        private readonly DispatcherTimer clockTimer = new DispatcherTimer();

        public MainMenuView()
        {
            InitializeComponent();

            currentEmployee = new Employee
            {
                Username = "Unknown",
                Role = "Secretary"
            };

            StartClock();
            ApplyEmployeeData();
        }

        public MainMenuView(Employee employee)
        {
            InitializeComponent();

            currentEmployee = employee;
            StartClock();
            ApplyEmployeeData();
        }

        private void StartClock()
        {
            UpdateClock();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += (_, _) => UpdateClock();
            clockTimer.Start();
        }

        private void UpdateClock()
        {
            LiveClockText.Text = DateTime.Now.ToString("dddd dd/MM/yyyy HH:mm:ss");
        }

        private void ApplyEmployeeData()
        {
            LoggedInText.Text =
                $"מחובר כ: {currentEmployee.Username} | תפקיד: {GetRoleText(currentEmployee.Role)}";

            ClientsCountText.Text = AppData.Clients.Count.ToString();
            AnimalsCountText.Text = AppData.Animals.Count.ToString();
            VisitsCountText.Text = AppData.Visits.Count.ToString();
            MedicationsCountText.Text = AppData.Medications.Count.ToString();

            if (currentEmployee.Role == "Secretary")
            {
                ApplySecretaryDashboard();
            }
            else if (currentEmployee.Role == "Vet")
            {
                ApplyVetDashboard();
            }
            else
            {
                ApplyNoAccessDashboard();
            }
        }

        private void ApplySecretaryDashboard()
        {
            RoleBadgeText.Text = "מזכיר/ה | ניהול לקוחות ובעלי חיים";
            HeroTitleText.Text = $"שלום {currentEmployee.Username}";
            HeroSubtitleText.Text = "זה אזור המזכירות. מתחילים בלקוח, ואז עוברים לבעלי החיים שלו.";

            RoleGuideTitle.Text = "מסלול עבודה מומלץ למזכיר/ה";
            RoleGuideText.Text = "כדי לא ללכת לאיבוד, עובדים בשני שלבים פשוטים:";
            StepOneTitle.Text = "פתח לקוחות";
            StepOneText.Text = "הוסף לקוח חדש, חפש לקוח קיים או עדכן פרטים.";
            StepTwoTitle.Text = "עבור לבעלי חיים";
            StepTwoText.Text = "אחרי שבחרת לקוח, הוסף או עדכן את החיות המשויכות אליו.";

            SetSectionAvailability(ClientsButton, ClientsCardButton, ClientsStatCard, true);
            SetSectionAvailability(AnimalsButton, AnimalsCardButton, AnimalsStatCard, true);
            SetSectionAvailability(VisitsButton, VisitsCardButton, VisitsStatCard, false);
            SetSectionAvailability(MedicinesButton, MedicinesCardButton, MedicationsStatCard, false);
        }

        private void ApplyVetDashboard()
        {
            RoleBadgeText.Text = "וטרינר/ית | טיפול, ביקורים ותרופות";
            HeroTitleText.Text = $"שלום {currentEmployee.Username}";
            HeroSubtitleText.Text = "זה אזור הווטרינר. מתחילים בזיהוי החיה, ואז רושמים ביקור או טיפול.";

            RoleGuideTitle.Text = "מסלול עבודה מומלץ לווטרינר/ית";
            RoleGuideText.Text = "כדי להתקדם נכון, עובדים לפי הסדר הזה:";
            StepOneTitle.Text = "פתח בעלי חיים";
            StepOneText.Text = "מצא חיה לפי שם או שבב ובדוק את הפרטים הרפואיים שלה.";
            StepTwoTitle.Text = "שמור ביקור";
            StepTwoText.Text = "רשום סיבת הגעה, אבחנה, טיפול, תרופה ועלות.";

            SetSectionAvailability(ClientsButton, ClientsCardButton, ClientsStatCard, false);
            SetSectionAvailability(AnimalsButton, AnimalsCardButton, AnimalsStatCard, true);
            SetSectionAvailability(VisitsButton, VisitsCardButton, VisitsStatCard, true);
            SetSectionAvailability(MedicinesButton, MedicinesCardButton, MedicationsStatCard, true);
        }

        private void ApplyNoAccessDashboard()
        {
            RoleBadgeText.Text = "אין הרשאות פעילות";
            HeroTitleText.Text = "אין הרשאות לתפקיד הזה";
            HeroSubtitleText.Text = "פנה למנהל המערכת כדי להגדיר תפקיד תקין.";
            RoleGuideTitle.Text = "לא ניתן להתקדם";
            RoleGuideText.Text = "המערכת לא מצאה מסכים זמינים לתפקיד שלך.";
            StepOneTitle.Text = "בדוק משתמש";
            StepOneText.Text = "ודא שנכנסת עם עובד תקין.";
            StepTwoTitle.Text = "חזור להתחברות";
            StepTwoText.Text = "התנתק והתחבר עם משתמש אחר אם צריך.";

            SetSectionAvailability(ClientsButton, ClientsCardButton, ClientsStatCard, false);
            SetSectionAvailability(AnimalsButton, AnimalsCardButton, AnimalsStatCard, false);
            SetSectionAvailability(VisitsButton, VisitsCardButton, VisitsStatCard, false);
            SetSectionAvailability(MedicinesButton, MedicinesCardButton, MedicationsStatCard, false);
        }

        private void SetSectionAvailability(Control menuButton, Control actionCard, Control statCard, bool isAvailable)
        {
            menuButton.IsVisible = isAvailable;
            menuButton.IsEnabled = isAvailable;
            actionCard.IsVisible = isAvailable;
            actionCard.IsEnabled = isAvailable;
            statCard.IsVisible = isAvailable;
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