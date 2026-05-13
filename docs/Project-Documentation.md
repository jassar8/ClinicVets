# ClinicVets
ClinicVets
Veterinary Clinic Management System


Project Charter


Team Members:
Jassar Abo Mamar
Rayan
Fares


Start Date: 21/04/2026
End Date: 31/05/2026
 
1. General Project Information
1.1 Project Name
ClinicVets – Veterinary Clinic Management System
1.2 Project Members
Jassar Abo Mamar
Mayar
Fares
1.3 Stakeholders
Veterinarians – responsible for diagnosing animals and managing treatments.
Clinic Secretaries – support daily operations and system usage.
Pet Owners – whose data and animal records are stored.
Clinic Management – requires accurate and organized data.
Development Team – responsible for building and testing the system.
2. Product Vision
For veterinary clinics that require an efficient and reliable system, the ClinicVets system is a desktop-based application developed in C# with a graphical user interface.

Unlike manual systems or spreadsheets, it provides a structured platform for managing visits, treatments, cost calculation, and vaccine tracking.

The system improves workflow efficiency, reduces human errors, and ensures data validation and consistency.
3. Personas
Veterinarian:
Dr. Amir Levi, age 41, works in a busy clinic environment. He needs a fast and reliable system to manage visits, record diagnosis, and prescribe treatments efficiently.

Clinic Secretary:
Maya Cohen, age 29, supports clinic operations and requires a simple and clear interface with minimal errors.

Clinic Manager:
Rina David, age 50, oversees operations and needs accurate and organized data for decision-making.

System Developer:
Responsible for building and testing the system, ensuring correctness, validation, and testability.
4. Problem Definition & Objectives
Veterinary clinics often rely on manual or disconnected systems, leading to inefficiency, data inconsistency, and errors.

The objective is to build a reliable C# GUI-based system that manages visits, treatments, cost calculation, and vaccine alerts, while supporting testing requirements.
5. Scope (Team 3)
In Scope:
- Opening visits
- Recording diagnosis
- Managing treatments
- Calculating visit cost
- Displaying vaccine alerts

Out of Scope:
- Login system
- Customer management
- Animal registration
- Web or mobile applications
6. Features
- Visit Management
- Treatment Recording
- Cost Calculation
- Vaccine Alert System
7. User Stories
1. As a veterinarian, I want to open and save a visit so that I can document medical records.
2. As a veterinarian, I want to add diagnosis and medicines so that I can record treatments correctly.
3. As a veterinarian, I want the system to calculate cost and show vaccine alerts so that I can ensure proper care and billing.
8. Roadmap
1. Requirements Analysis
2. Testing Design
3. GUI Design
4. Implementation
5. Testing and Submission
9. Development Environment
C#, Windows Forms or WPF, Excel or Database
10. Success Metrics
- System runs without errors
- Accurate calculations
- Completed testing tasks
- No critical bugs
11. Risks
- Logic errors
- GUI usability issues
- Data storage problems
- Missing validation rules

## Architecture

All business logic runs in **C#** (no Python, Node.js, or web-server backend). The WinForms desktop host calls **Application** services, which use **Infrastructure** repositories for local JSON storage.

## Repository layout (for review and demo)

| Area | Path | Purpose |
|------|------|--------|
| Domain (models) | `src/Backend/ClinicVets.Core/Models/` | `Employee` and future domain types |
| Application (services) | `src/Backend/ClinicVets.Application/Services/`, `Interfaces/`, `Validation/` | Business logic, contracts, and input rules |
| Infrastructure (data) | `src/Backend/ClinicVets.Infrastructure/Data/` | Local JSON persistence (`JsonFileEmployeeRepository`) |
| Desktop (UI) | `src/Frontend/Forms/`, `src/Frontend/UI/`, `src/Frontend/UserControls/` | WinForms screens, theme, and shared controls |
| Tests | `Tests/ClinicVets.Tests/` (`Functional/`, `Integration/`, `GUI/`) | Automated tests (xUnit) |
| Branding source | `assets/branding/` | Logo PNG; multi-size `.ico` is built to `assets/app/ClinicVets.ico` |
| Persistence overview | `database/README.md` | Where runtime JSON files live |
| Published app | `PublishedApp/` | Release output after `publish-win-x64.ps1` |
| Quick launch copy | `RunApp/` | After each **build** or **publish** of the desktop project, outputs are mirrored here (`RunApp/ClinicVets.exe`). |

## Running As Standalone Windows EXE (Demo)

The course demo runs as a **WinForms desktop app** (no browser, no localhost web server).

1. Open PowerShell in the project root.
2. Run:
   `.\publish-win-x64.ps1`
3. After publish completes, double-click either:
   - `.\RunApp\ClinicVets.exe` (recommended — always at repo root after build/publish), or
   - `.\PublishedApp\ClinicVets.exe`

Keep the entire `RunApp` or `PublishedApp` folder together when copying to another PC (self-contained publish includes the runtime).

Employee data is saved locally under:
`%LocalAppData%\ClinicVets\employees.json`
