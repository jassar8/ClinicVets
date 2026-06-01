# Backend layer (ClinicVets desktop app)

This is a single-process Avalonia desktop application. There is no separate HTTP API server.

Backend responsibilities are implemented in:

- **`../Data/`** — SQLite persistence + Excel mirror (`ClinicVets.xlsx`, `ExcelExportService`)
- **`../Services/`** — Business rules (validation)

The **`../Models/`** folder holds entity types shared by the UI and data layer.

The **`../Frontend/`** folder contains views and UI helpers only.
