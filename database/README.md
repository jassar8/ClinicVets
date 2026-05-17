# Database (persistence)

ClinicVets uses **file-based JSON storage** for the desktop demo (no SQL server in this repository).

Employee and customer records are written under the current user profile, for example:

`%LocalAppData%\ClinicVets\employees.json`  
`%LocalAppData%\ClinicVets\customer-directory.json`  
`%LocalAppData%\ClinicVets\medications.json`

Implementation lives in `src/Backend/ClinicVets.Infrastructure/Data/`. This folder holds documentation only; runtime data files are created at first run.
