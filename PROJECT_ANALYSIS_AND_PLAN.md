# ClinicVets — Project Analysis and Plan (read-only phase)

**Date:** 2026-05-14  
**Current working branch (workspace):** `fix` (with uncommitted local edits from SDK/TFM troubleshooting)  
**Phase:** Analysis only — **no merges, deletions, or code changes** were made to produce this document (except this file).

> **Note on HW/PDF:** No homework or project PDF was found inside the repository (`*.pdf` search returned zero files). Requirements below are inferred from: feature areas repeated in your instructions, `TEMP_TEST_CHECKLIST.md` on branch `v4`, commit messages, and typical veterinary-clinic desktop project expectations. **When you have the PDF, map each rubric item to section 4 and adjust priorities.**

---

## 1. Branch inventory

### 1.1 Local branches (17)

| Branch | Tip commit (short) | One-line role |
|--------|-------------------|---------------|
| `main` | `627f7e0` | GitHub `main`: root Avalonia app, `net10.0`, flat `Views/` |
| `fix` | `627f7e0` | Same as `main` at creation; intended fix workspace |
| `v1login` | `e8dfb06` | Early WinForms + layered backend; login-focused era |
| `v2` | `3f341ae` | Mature **WinForms** UI + **backend** + **68 unit tests** |
| `v3` | `65fada4` | Dual stack: WinForms + Avalonia; **includes updateapp merge** |
| `v3-before-updateapp` | `b202ee1` | Same as v3 **minus** commit `65fada4` (pre–friend merge) |
| `v4` | `58c6554` | Consolidation: `run testapp/` official app, RunApp sync, tests |
| `v10` | `02bd127` | v4 lineage + folder cleanup (`Assets/Styles`, `Database/`) |
| `updateapp` | `dfcb8e4` | Friend/reference Avalonia at repo root (flat layout) |
| `new-front-end` | (older) | WPF experiment (historical) |
| `admin` | — | Admin-related work |
| `feature/employee-authentication` | `5f9cd2b` | RBAC + default admin (backend-oriented) |
| `CLIN-10-Employee-Login` | `c186eb9` | Desktop login UX iteration |
| `CLIN-11-Employee-Registration` | `40ea289` | Registration + repo hygiene |
| `CLIN-12-Customer-Registration` | — | Customer registration feature branch |

### 1.2 Remote-only branches (not checked out locally)

`origin/CLIN-13-Customer-Search`, `origin/CLIN-14-View-Customer-Animals`, `origin/CLIN-18-Link-Animal-to-Owner`, `origin/CLIN-19-Manage-Animal-Records`, `origin/page-log-in`, plus mirrors of branches above.

### 1.3 Branches analyzed in depth

**Primary (9):** `main`, `fix`, `v1login`, `v2`, `v3`, `v3-before-updateapp`, `v4`, `v10`, `updateapp`  

**Secondary (sampled, 6):** `CLIN-10-Employee-Login`, `CLIN-11-Employee-Registration`, `CLIN-12-Customer-Registration`, `feature/employee-authentication`, `new-front-end`, `admin`

**Total branches considered:** **23** (17 local + 6 remote-only / secondary)

---

## 2. Technology map per branch

| Branch | UI stack | App location | Backend (`src/Backend`) | Official EXE name | TFM (Avalonia) |
|--------|----------|--------------|-------------------------|-------------------|----------------|
| `main` / `fix` | Avalonia @ root | `ClinicVetsAvalonia.csproj`, `Views/*.axaml` | **No** | `ClinicVetsAvalonia` (default assembly name) | **net10.0** |
| `updateapp` | Avalonia @ root | Same pattern as main | **No** | Default | net9/10 (root csproj) |
| `v3`, `v3-before-updateapp` | Avalonia + WinForms | Root Avalonia + `src/Frontend` | **Yes** | Two apps (`ClinicVets` / WinForms) | Avalonia in tree |
| `v4`, `v10` | Avalonia + WinForms | **`run testapp/`** + WinForms | **Yes** | **`ClinicVets.exe`** (Avalonia) | **net9.0-windows** |
| `v2`, `v1login` | WinForms only | `src/Frontend` | **Yes** | `ClinicVets` (WinForms) | N/A |
| `v2` | — | — | Application services, repositories, **68 tests** | — | — |

---

## 3. Per-branch analysis

### 3.1 `main` and `fix`

| Area | Assessment |
|------|------------|
| **Frontend** | Single Avalonia app at repo root; 11 `.axaml` screens; Hebrew RTL on login; turquoise/white styling in XAML. |
| **Backend** | **Monolithic** — `Data/AppData.cs` (~570 lines) holds schema + CRUD + in-memory lists. No `Core`/`Application`/`Infrastructure` projects. |
| **Navigation** | `MainWindow.axaml.cs` → login → main menu → feature views (same pattern as early Avalonia). |
| **Database** | SQLite under `%AppData%\ClinicVetsAvalonia\clinic.db` (note: different folder name than v4’s `ClinicVets`). |
| **Auth / register / reset** | Implemented in `Views/*` + `ValidationService` + `PasswordResetService`. |
| **Medicine** | `MedicationsView` with add/update/delete/search patterns in code-behind. |
| **EXE / build** | `Run-Windows.bat` → `dotnet run`; **no** `run testapp/`, **no** RunApp sync script in tree; targets **net10.0** → fails on machines with only **.NET 9 SDK**. |
| **Tests** | `Tests/ClinicVetsAvalonia.Tests/` (xUnit) on main — validation + integration style tests. |
| **Stability** | Builds only with .NET 10 SDK; workspace on `fix` may pick up **untracked** folders from other branches (`run testapp/`, `src/`) and break build unless excluded. |
| **Duplicates / confusion** | `fix` ≡ `main` at `627f7e0`; unrelated history was force-pushed to `origin/main` earlier. |

**Strengths:** Simple tree; one entry point; matches friend layout for comparison.  
**Weaknesses:** net10.0 mismatch; flat `Views/`; no RunApp publish pipeline; DB path naming inconsistency; no layered backend for teacher “architecture” story.

---

### 3.2 `updateapp` (reference only — do not merge)

| Area | Assessment |
|------|------------|
| **Role** | Friend’s Avalonia snapshot; **reference for visuals only**. |
| **Frontend** | Root-level `Views/`; global styles in `App.axaml` (buttons, TextBox, ComboBox, DatePicker). |
| **Backend** | Same monolithic `Data/AppData` style as main. |
| **Diff vs v3-before-updateapp** | Small XAML tweaks (~9 files, ~40 lines) + `App.axaml` style block + publish script — **not** a separate architecture. |
| **EXE** | `Publish-Avalonia-WinX64.ps1` at root; no `run testapp` convention. |

**What to learn:** Color palette (`#0797C9`, `#B7D6E3`, `#FBFEFF`), corner radii, padding, RTL layout ideas.  
**What NEVER to merge:** Entire branch, `App.axaml` as sole style source, duplicate root layout, friend commit history, any assumption this is “your” final app.

---

### 3.3 `v3-before-updateapp` (your Avalonia before friend merge commit)

| Area | Assessment |
|------|------------|
| **vs `v3`** | Parent of `65fada4`; **only difference** from `v3` is that merge commit (small XAML + App.axaml + publish script changes). |
| **Frontend** | Avalonia at **repo root** (not yet in `run testapp/`). |
| **Backend** | Full WinForms stack + backend projects still present. |
| **Stability** | Known good baseline for **your** UI before labeled “updateapp merge”. |

**Strengths:** Best historical point for “my version before friend merge.”  
**Weaknesses:** Still dual UI stacks; less organization than v4/v10.

---

### 3.4 `v3`

| Area | Assessment |
|------|------------|
| **Contains** | Everything in `v3-before-updateapp` **plus** commit `65fada4` (“Merge updateapp frontend and logic into v3”). |
| **Risk** | Mixes friend styling/structure into your line; hard to tell what is yours vs reference without diff review. |

**Recommendation:** Treat as **contaminated baseline**; prefer `v3-before-updateapp` or `v10` for “your” UI logic.

---

### 3.5 `v2`

| Area | Assessment |
|------|------------|
| **Frontend** | **WinForms only** — mature forms, responsive layout commits, dashboard, login/register. **No Avalonia.** |
| **Backend** | **Best structured:** `ClinicVets.Core`, `Application`, `Infrastructure`; DI-style services. |
| **Tests** | **`Tests/ClinicVets.Tests`** — 68 passing tests (auth, validation, roles, customers, employees). |
| **EXE** | `publish-win-x64.ps1`, `RunApp/README.md`; WinForms `ClinicVets.exe` naming era. |
| **Medicine / visits / animals** | WinForms feature forms (not Avalonia views). |

**Strengths:** Cleanest **backend architecture** and **automated test** coverage.  
**Weaknesses:** Not the Avalonia teacher demo if they expect the XAML app; different UX stack.

---

### 3.6 `v1login` / `v1login`

| Area | Assessment |
|------|------------|
| **Role** | Early project phase; WinForms + backend; login scaffolding. |
| **vs v2** | Less UI polish; fewer features; predecessor to v2. |

**Strengths:** Simple, fewer moving parts.  
**Weaknesses:** Superseded by v2 for WinForms quality.

---

### 3.7 `v4`

| Area | Assessment |
|------|------------|
| **Frontend** | Avalonia moved to **`run testapp/`** with organized `Views/Auth`, `Dashboard`, `Medicine`, etc. |
| **Styles** | `Styles/` or `AppUi/Styles/` → theme brushes + `AppTheme.axaml`. |
| **Data** | `Repositories/AppData.cs` + `Database/DbPaths.cs` + `ClinicSchema.cs`; `%AppData%\ClinicVets\`. |
| **EXE** | **`ClinicVets.exe`**; `Publish-Avalonia-WinX64.ps1`; **`docs/Scripts/Sync-RunApp.ps1`** → `RunApp/`. |
| **Tests** | `Tests/ClinicVets.Avalonia.Unit` (42 tests) + backend `ClinicVets.Tests` (68). |
| **WinForms** | Still in solution as **`ClinicVetsWinForms.exe`** (renamed to avoid clash). |
| **Docs** | `TEMP_TEST_CHECKLIST.md`, `RunApp-README`, solution layout. |

**What was “broken” / confusing in v4 (process & ops, not necessarily code):**

- Perception that EXE was “friend version” when wrong folder (`RunApp\win-x64\`) or old build was launched.
- **Dual desktop apps** in one solution (WinForms + Avalonia).
- Multiple publish scripts (`publish-win-x64.ps1`, `Publish-V4-WinX64.ps1`, root launchers removed then re-added on other branches).
- History still contains updateapp merge **ancestor** on v3 line.
- **14 manual UI tests** still required per checklist.

**Strengths:** Best **operational EXE pipeline** and **Avalonia project organization** in repo history.  
**Weaknesses:** Complexity for reviewers; branch noise; v4 commit messages say “stable” but UX still mixed lineages.

---

### 3.8 `v10`

| Area | Assessment |
|------|------------|
| **vs v4** | Small diff: `Styles/` → `Assets/Styles/`, version **10.0.0**, window title, README; **explicit policy: no updateapp merge in v10 commit**. |
| **Frontend foundation** | Same screens as v4 with cleaner asset layout. |
| **EXE** | Same publish + RunApp sync as v4. |

**Strengths:** Best **named “final” branch** for your story; organized folders; tests inherited from v4.  
**Weaknesses:** Still inherits v3/updateapp **ancestry** in git history (not a clean rewrite).

---

### 3.9 Feature branches (`CLIN-*`, `feature/employee-authentication`)

| Branch | Focus |
|--------|--------|
| `CLIN-10-Employee-Login` | Maximized window, responsive login layouts |
| `CLIN-11-Employee-Registration` | Registration flow, gitignore hygiene |
| `CLIN-12-Customer-Registration` | Customer registration |
| `feature/employee-authentication` | RBAC, default administrator (backend) |

**Use:** Cherry-pick **ideas** or **tests** into final branch — not wholesale merges without review.

---

## 4. Comparison to expected homework / project requirements

*(Inferred — verify against PDF.)*

| Requirement area | Expected (typical) | Best current match | Gap |
|------------------|-------------------|-------------------|-----|
| Employee login | Valid/invalid credentials | v4/v10 Avalonia + v2 service tests | main uses same UI pattern; DB path differs |
| Employee registration | Validation, duplicates | v4/v10 `RegisterEmployeeView` + `ValidationService` | WinForms v2 has separate registration service tests |
| Password reset | Email/code flow | v4/v10 `ForgotPasswordService` (demo mode without Gmail env) | Needs manual UI verification |
| Role permissions | Secretary vs Vet menus | v4/v10 `MainMenuView` + unit tests | v2 has richer RBAC in backend tests |
| Clients / animals / visits | CRUD + navigation | v4/v10 views under `Views/` | main has flat `Views/` same features |
| Medicine inventory | CRUD + search + filters | v4/v10 `MedicationsView` + `MedicationSearchFilter` | — |
| Hebrew RTL | FlowDirection, readable UI | All Avalonia branches | Manual visual QA open |
| SQLite persistence | Local DB | v4/v10 `Repositories/AppData` | main uses `Data/AppData` + `ClinicVetsAvalonia` folder |
| Professional structure | Layers, folders, tests | **v2 backend** + **v10/v4 Avalonia layout** | main/fix monolithic |
| EXE deliverable | Runnable Windows app | **v4/v10** publish + RunApp | main needs net10 SDK or TFM fix |
| Automated tests | Unit/integration | **v2 (68)** + **v4 (42 Avalonia)** | main has smaller Avalonia test project |
| No copied friend code | Original submission | **v3-before-updateapp**, **v10 policy** | v3, ancestry of v4/v10 |

---

## 5. Knowledge map (what is best where)

| Question | Answer |
|----------|--------|
| **Best in v1 / v1login** | Early WinForms login path; minimal scope; good for understanding evolution only. |
| **Best in v2** | **Backend architecture**, **domain services**, **68 automated tests**, WinForms UX polish. |
| **Best in v3 (your line)** | Avalonia feature-complete at root; dual solution — **undermined by updateapp merge commit**. |
| **Best in v3-before-updateapp** | **Your Avalonia UI before friend merge** — best “pure you” Avalonia snapshot on old layout. |
| **Broken / risky in v4** | Operational confusion (EXE paths), dual apps, historical friend merge in ancestry, manual QA debt — **not necessarily broken code** if built from `run testapp` only. |
| **Learned from updateapp** | Global control styles, turquoise theme, rounded fields — **reimplement in your `Assets/Styles`**, don’t copy repo. |
| **NEVER merge** | **`updateapp` branch wholesale**; friend `App.axaml` as startup; duplicate root apps; overwriting `run testapp` with root flat layout. |
| **Best EXE setup** | **`v4` / `v10`**: `run testapp/Publish-Avalonia-WinX64.ps1`, `AssemblyName=ClinicVets`, `Sync-RunApp.ps1`, strip `RunApp\win-x64`. |
| **Cleanest Avalonia logic** | **`v4` / `v10`** (`Repositories`, `Database`, `ValidationService`, navigation in `MainWindow`). |
| **Best frontend foundation** | **`v10`** (folder layout) or **`v4`** (same features, slightly less polished paths). |

---

## 6. Detected problems (cross-branch / workspace)

### 6.1 Duplicated and conflicting trees

| Issue | Where |
|-------|--------|
| **Two Avalonia layouts** | Root (`main`/`fix`/`updateapp`) vs `run testapp/` (`v4`/`v10`) |
| **Two desktop stacks** | Avalonia + WinForms in `v3`/`v4`/`v10` |
| **Untracked folders on disk** | `run testapp/`, `src/`, `RunApp/`, `Tests/` when on `main`/`fix` — cause CS0579 duplicate attributes if SDK globs them |
| **Multiple DB paths** | `ClinicVetsAvalonia` (main) vs `ClinicVets` (v4/v10) |
| **Multiple test projects** | `ClinicVets.Tests`, `ClinicVets.Avalonia.Unit`, `ClinicVetsAvalonia.Tests` |

### 6.2 EXE / publish confusion

| Artifact | Branch context |
|----------|----------------|
| `RunApp\ClinicVets.exe` | Synced from v4/v10 build/publish |
| `run testapp\Publish\ClinicVets.exe` | Self-contained publish output |
| `RunApp\win-x64\ClinicVets.exe` | **Stale duplicate** (fixed in v4 sync script, may exist on old builds) |
| `bin\...\ClinicVetsAvalonia.exe` | main/fix assembly name |
| WinForms `ClinicVetsWinForms.exe` | v4 solution |

### 6.3 Startup confusion

- **`Run-Windows.bat`** (main/fix) vs **`run testapp/Run.bat`** (v4/v10)
- VS Code `.vscode/launch.json` on v4 pointed at `run testapp`; on main at root `bin/`
- No single “official” branch documented on `main`

### 6.4 Namespace / structure inconsistencies

| Pattern | Branches |
|---------|----------|
| `ClinicVetsAvalonia.Data` | main, updateapp |
| `ClinicVetsAvalonia.Repositories` + `.Database` | v4, v10 |
| `ClinicVets.Application.Services` | v2 backend |
| Flat `Views/` vs `Views/Auth/`, etc. | main vs v4/v10 |

---

## 7. Recommended final strategy (plan only — not implemented)

### 7.1 Recommended base branch

**`v10`** (or equivalently **`v4` @ `58c6554`** if you want the pre-v10 commit message only).

**Reason:** Only line with **`run testapp/`** as official app, **`ClinicVets.exe`**, RunApp sync, Avalonia unit tests, organized views, and explicit separation from friend branch policy.

**Do not use `main`/`fix` as final base without:**

- Retargeting TFM to `net9.0-windows` (or installing .NET 10 SDK)
- Migrating `run testapp` structure **or** abandoning root monolith
- Resolving untracked-folder build pollution

### 7.2 What to take from each branch

| Source | Take | Avoid |
|--------|------|--------|
| **v10 / v4** | Entire `run testapp` Avalonia app, publish, RunApp sync, tests | WinForms unless teacher requires |
| **v3-before-updateapp** | Screen flow reference if diff shows regressions after merge | The merge commit itself |
| **v2** | Backend projects, service patterns, `ClinicVets.Tests` | WinForms UI as default EXE |
| **v1login** | Historical reference only | — |
| **updateapp** | Style tokens (colors, radii) → rewrite in `Assets/Styles` | Files, App startup, logic |
| **main** | `Tests/ClinicVetsAvalonia.Tests` cases if not in v4 | net10.0, flat layout, `Data/` monolith |
| **CLIN-*** | Specific UX fixes (maximize window, etc.) | Blind merge |

### 7.3 Final branch structure (proposal)

```
main          ← protected; receives squash from release/v10 after QA
release/v10   ← teacher-facing stable (from v10)
fix           ← short-lived patches (rebased on release/v10)
archive/v2-winforms
archive/updateapp-reference  (read-only tag, no merge)
```

### 7.4 Final merge strategy (when implementation phase starts)

1. Branch **`release/v10`** from **`v10`**.
2. **Do not merge `updateapp`.** Optionally `git diff updateapp -- Views/*.axaml` for visual ideas only.
3. Port any missing **v2 backend tests** or services only if teacher requires layered architecture in submission PDF.
4. Cherry-pick CLIN UX commits if they fix real bugs (one commit at a time).
5. Align **`main`** to `release/v10` after full QA (or keep `main` as GitHub default with fast-forward).

### 7.5 Final frontend strategy

- **Single app root:** `run testapp/`
- **Folders:** `Views/{Auth,Dashboard,Medicine,Employees,Clients,Animals,Visits,Shared}`, `Assets/Styles`, `Database`, `Repositories`, `Services`, `Models`, `ViewModels`
- **Theme:** Centralize in `Assets/Styles` (v10 model); borrow **values** from updateapp reference only
- **RTL:** Keep `FlowDirection="RightToLeft"` on all user-facing views; manual screenshot pass

### 7.6 Final logic strategy

- Keep **v4/v10** `AppData` + SQLite schema for Avalonia demo (fastest path to working EXE).
- **Optional phase 2:** Wire Avalonia to v2 `Application` services behind interfaces (teacher architecture bonus).
- One **DB path**: `%AppData%\ClinicVets\` everywhere; migration note for old `ClinicVetsAvalonia` DB.

### 7.7 Final EXE strategy

| Step | Action |
|------|--------|
| Build | `dotnet build run testapp/ClinicVetsAvalonia.csproj -c Release` |
| Publish | `run testapp/Publish-Avalonia-WinX64.ps1` |
| Distribute | Zip entire `run testapp/Publish/` folder |
| Daily dev | `run testapp/Run.bat` or `RunApp/ClinicVets.exe` after sync |
| Rename | Always **`ClinicVets.exe`**; WinForms stays **`ClinicVetsWinForms.exe`** |

### 7.8 Final cleanup plan (implementation phase)

1. `git clean` / remove untracked duplicate trees on dev machines (careful — backup first).
2. Delete or gitignore: stale `RunApp/win-x64`, duplicate root Avalonia when on v10 line.
3. One solution entry: `ClinicVets.sln` with startup project **ClinicVetsAvalonia** in `run testapp`.
4. Remove obsolete launchers at repo root or point them to `run testapp`.
5. Consolidate test projects or document which are “teacher tests” vs internal.
6. Add `README.md` at root: official run/publish paths.

### 7.9 Final testing strategy

| Layer | Source |
|-------|--------|
| Unit (validation, filters, roles) | `ClinicVets.Avalonia.Unit` (v4/v10) |
| Backend services | `ClinicVets.Tests` (v2/v4 solution) |
| Manual Hebrew UI | Checklist from `TEMP_TEST_CHECKLIST.md` (v4) |
| EXE smoke | Launch `Publish/ClinicVets.exe`; verify title/version |
| Pre-submit | Full `dotnet test ClinicVets.sln -c Release` |

---

## 8. Biggest bugs and risks (summary)

1. **Wrong SDK / TFM** on `main`/`fix` (`net10.0` vs installed SDK 9).
2. **Wrong EXE or folder** launched (`RunApp\win-x64`, old build, WinForms exe).
3. **Untracked cross-branch folders** causing duplicate compile errors on `main`/`fix`.
4. **Friend merge commit** on `v3` blurring ownership of UI changes.
5. **Two database folder names** (`ClinicVets` vs `ClinicVetsAvalonia`).
6. **Dual UI stacks** confusing reviewers and developers.
7. **Incomplete manual RTL/visual QA** (automated tests don’t cover UI layout).

---

## 9. Implementation phase gate

**Do not start merges or file moves until:**

- [ ] PDF requirements mapped to section 4
- [ ] You confirm final deliverable is **Avalonia** (not WinForms)
- [ ] You choose **`v10`** vs **`v3-before-updateapp`** as UI baseline
- [ ] Backup/tag current `fix` and `main`

---

## 10. Quick reference — how to run (by branch line)

| Branch line | Command |
|-------------|---------|
| **main / fix** | `Run-Windows.bat` or `dotnet run --project ClinicVetsAvalonia.csproj` (needs .NET 10 SDK **or** TFM fix) |
| **v4 / v10** | `run testapp/Run.bat` or `dotnet run --project "run testapp/ClinicVetsAvalonia.csproj"` |
| **Published EXE** | `run testapp/Publish/ClinicVets.exe` (v4/v10) |

---

*End of analysis document. No repository code was modified to create this plan.*
