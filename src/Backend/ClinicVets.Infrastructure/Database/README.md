# Database (desktop persistence)

There is no SQL Server in the v2 desktop build. Data is stored as JSON under:

`%LocalAppData%\ClinicVets\`

| File | Content |
|------|---------|
| `employees.json` | Staff accounts |
| `customer-directory.json` | Customers and animals |
| `medications.json` | Medicine inventory |
| `visits.json` | Visits and treatments |

Repositories that read/write these files live in `../Repositories/`.
