# תשובת מטלה — ביקורים, טיפולים ומלאי תרופות

**פרויקט:** ClinicVets — מערכת ניהול מרפאה וטרינרית  
**תחום:** רישום ביקורים וטיפולים, הוספת תרופה למלאי, מחיקת תרופה  
**קבצים רלוונטיים בפרויקט:**  
`VisitsView`, `MedicationsView`, `ValidationService`, `AppData`, `Visit`, `Medication`

---

## 1. CFG — גרף זרימת בקרה (Control Flow Graph)

נבחרו **שתי פונקציות אמיתיות** מהקוד. כל אחת כוללת תנאים (`if`) מרובים.

---

### 1.1 פונקציה ראשונה: `TryReadMedicationFields`

**מטרה:** לקרוא ולאמת שדות תרופה לפני הוספה או עדכון במלאי.  
**מיקום:** [`Source/Frontend/Views/MedicationsView.axaml.cs`](Source/Frontend/Views/MedicationsView.axaml.cs) — שורות 233–289  
**Class:** `MedicationsView`  
**Method:** `private bool TryReadMedicationFields(out Medication medication)`

#### קוד הפונקציה (מתוך הפרויקט — מקוצר)

```csharp
private bool TryReadMedicationFields(out Medication medication)
{
    medication = new Medication();
    string name = NameInput.Text?.Trim() ?? "";
    string stockText = StockInput.Text?.Trim() ?? "";
    string unitPriceText = UnitPriceInput.Text?.Trim() ?? "";
    DateTime expirationDate = ExpirationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

    if (!ValidationService.IsRequiredText(name))
    { UIHelper.ShowMessage(this, "שם תרופה הוא שדה חובה"); return false; }

    if (!int.TryParse(stockText, out int stockQuantity))
    { UIHelper.ShowMessage(this, "כמות מלאי חייבת להיות מספר שלם"); return false; }

    if (!ValidationService.IsValidStockQuantity(stockQuantity))
    { UIHelper.ShowMessage(this, "כמות מלאי לא יכולה להיות שלילית"); return false; }

    if (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out double unitPrice))
    { UIHelper.ShowMessage(this, "מחיר יחידה חייב להיות מספר"); return false; }

    if (!ValidationService.IsValidMoney(unitPrice))
    { UIHelper.ShowMessage(this, "מחיר יחידה לא יכול להיות שלילי"); return false; }

    if (!ValidationService.IsValidExpirationDate(expirationDate))
    { UIHelper.ShowMessage(this, "תאריך תפוגה לא יכול להיות בעבר"); return false; }

    medication = new Medication { Name = name, StockQuantity = stockQuantity, ... };
    return true;
}
```

#### הסבר CFG — `TryReadMedicationFields`

**צמתים (Nodes):**

| מזהה | תיאור |
|------|--------|
| M1 | התחלה — קריאת שדות מהטופס |
| M2 | האם שם לא ריק? |
| M3 | האם מלאי הוא int? |
| M4 | האם מלאי ≥ 0? |
| M5 | האם מחיר הוא double? |
| M6 | האם מחיר ≥ 0? |
| M7 | האם תאריך תפוגה ≥ היום? |
| M8 | בנה אובייקט `Medication`; החזר `true` |
| M9 | הודעת שגיאה; החזר `false` |
| M10 | סיום |

**קשתות (Edges):**

```
M1 → M2
M2 --[לא]--> M9 → M10
M2 --[כן]--> M3
M3 --[לא]--> M9
M3 --[כן]--> M4
M4 --[לא]--> M9
M4 --[כן]--> M5
M5 --[לא]--> M9
M5 --[כן]--> M6
M6 --[לא]--> M9
M6 --[כן]--> M7
M7 --[לא]--> M9
M7 --[כן]--> M8 → M10
```

---

### 1.2 פונקציה שנייה: `SaveVisit_Click`

**מטרה:** לשמור ביקור חדש עם חיה, תאריך, סיבה ולפחות שורת טיפול אחת.  
**מיקום:** [`Source/Frontend/Views/VisitsView.axaml.cs`](Source/Frontend/Views/VisitsView.axaml.cs) — שורות 476–553  
**Class:** `VisitsView`  
**Method:** `private void SaveVisit_Click(object? sender, RoutedEventArgs e)`

#### קוד הפונקציה (מתוך הפרויקט — לוגיקה מרכזית)

```csharp
private void SaveVisit_Click(object? sender, RoutedEventArgs e)
{
    if (selectedVisit != null) { UpdateSelectedVisit(); return; }

    string chipNumber = AnimalChipInput.Text?.Trim() ?? "";
    var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);
    if (animal == null)
    { UIHelper.ShowMessage(this, "יש לבחור חיה קיימת לפני שמירת ביקור"); return; }

    if (!TryGetVisitDate(out DateTime visitDate)) return;
    if (!ValidationService.IsValidVisitDate(visitDate)) { ... return; }
    if (IsVisitDateTimeInPast(visitDate)) { ... return; }
    if (!ValidationService.IsRequiredText(reason)) { ... return; }
    if (pendingTreatmentLines.Count == 0) { ... return; }
    if (!TryCalculateTotalCost(out double totalCost)) return;

    ApplyPendingTreatmentLinesStock(null);
    AppData.Visits.Add(newVisit);
    AppData.SaveMedicationsToDatabase();
    AppData.SaveVisitsToDatabase();
    UIHelper.ShowMessage(this, "הביקור נשמר בהצלחה");
}
```

#### הסבר CFG — `SaveVisit_Click`

**צמתים (Nodes):**

| מזהה | תיאור |
|------|--------|
| V1 | התחלה — לחיצה על "שמור ביקור" |
| V2 | האם `selectedVisit != null`? (מצב עדכון) |
| V3 | האם חיה קיימת לפי שבב? |
| V4 | האם תאריך ביקור תקין ועתידי? |
| V5 | האם סיבת ביקור מלאה? |
| V6 | האם יש לפחות טיפול אחד? |
| V7 | שמור ביקור + עדכן מלאי תרופות |
| V8 | הודעת שגיאה / יציאה |
| V9 | סיום |

**קשתות (Edges):**

```
V1 → V2
V2 --[כן / עדכון]--> UpdateSelectedVisit → V9
V2 --[לא / ביקור חדש]--> V3
V3 --[לא]--> V8 → V9
V3 --[כן]--> V4
V4 --[לא]--> V8
V4 --[כן]--> V5
V5 --[לא]--> V8
V5 --[כן]--> V6
V6 --[לא]--> V8
V6 --[כן]--> V7 → V9
```

---

## 2. שלוש User Stories (סיפורי משתמש)

### US-01 — רישום ביקור וטיפולים

**As a** וטרינר/ית,  
**I want** לרשום ביקור חדש לחיה עם סיבת הגעה, אבחנה ושורות טיפול (כולל תרופות),  
**So that** אוכל לתעד את הטיפול הרפואי ולעדכן את עלות הביקור.

**קריטריוני קבלה:**
- בחירת חיה לפי מספר שבב.
- תאריך ביקור עתידי (לא בעבר).
- לפחות שורת טיפול אחת לפני שמירה.
- עדכון מלאי תרופות אוטומטי (`ApplyPendingTreatmentLinesStock`).

---

### US-02 — הוספת תרופה למלאי

**As a** וטרינר/ית,  
**I want** להוסיף תרופה חדשה למלאי עם שם, כמות, מחיר ותאריך תפוגה,  
**So that** אוכל לרשום תרופות בביקורים ולנהל מלאי.

**קריטריוני קבלה:**
- ולידציה דרך `TryReadMedicationFields`.
- מניעת שם כפול.
- שמירה ל-`AppData.Medications`.

---

### US-03 — מחיקת תרופה מהמלאי

**As a** וטרינר/ית,  
**I want** למחוק תרופה שלא בשימוש,  
**So that** המלאי יישאר מעודכן וללא פריטים מיותרים.

**קריטריוני קבלה:**
- חובה לבחור תרופה לפני מחיקה.
- חסימה אם התרופה משויכת לביקור קיים.
- הודעת הצלחה לאחר מחיקה.

---

## 3. ארבעה מקרי בדיקה (Test Cases)

מקרי הבדיקה מגיעים מ-**US-01**, **US-02** ו-**US-03**.

---

### TC-01

| שדה | ערך |
|-----|-----|
| **Test Case ID** | TC-01 |
| **Test Case Name** | שמירת ביקור חדש — נתונים תקינים |
| **Type** | Functional — Positive |
| **Test Data** | שבב חיה קיים, תאריך עתידי, סיבה: "חיסון", שורת טיפול אחת לפחות |
| **Expected Result** | הודעה "הביקור נשמר בהצלחה"; הביקור מופיע ביומן; מלאי תרופות מתעדכן |

---

### TC-02

| שדה | ערך |
|-----|-----|
| **Test Case ID** | TC-02 |
| **Test Case Name** | שמירת ביקור — ללא שורות טיפול |
| **Type** | Validation — Negative |
| **Test Data** | חיה קיימת, סיבה מלאה, `pendingTreatmentLines.Count == 0` |
| **Expected Result** | הודעה: "יש להוסיף לפחות טיפול / קורס אחד לביקור"; לא נשמר |

---

### TC-03

| שדה | ערך |
|-----|-----|
| **Test Case ID** | TC-03 |
| **Test Case Name** | הוספת תרופה למלאי — נתונים תקינים |
| **Type** | Functional — Positive |
| **Test Data** | שם: `אנטיביוטיקה`, מלאי: `50`, מחיר: `25.5`, תפוגה: בעוד 6 חודשים |
| **Expected Result** | הודעה "התרופה נוספה בהצלחה"; התרופה ברשימה |

---

### TC-04

| שדה | ערך |
|-----|-----|
| **Test Case ID** | TC-04 |
| **Test Case Name** | מחיקת תרופה — תרופה משויכת לביקור |
| **Type** | Functional — Negative |
| **Test Data** | תרופה שמופיעה ב-`Visit.MedicationName` עם כמות > 0 |
| **Expected Result** | הודעה: "לא ניתן למחוק תרופה שמשויכת לביקור..."; התרופה נשארת |

---

## 4. בדיקות Functional ו-GUI

### 4.1 שתי בדיקות Functional

**FT-01 — הוספת תרופה ושמירה במסד**

| פריט | תיאור |
|------|--------|
| **מטרה** | לוודא שתרופה חדשה נשמרת |
| **Preconditions** | מחובר כ-vet; מסך תרופות פתוח |
| **Steps** | 1. לחץ "הוסף תרופה חדשה" 2. מלא שדות תקינים 3. לחץ "שמור תרופה" |
| **Expected** | הודעת הצלחה; התרופה ב-`AppData.Medications` |

**FT-02 — שמירת ביקור מפחית מלאי**

| פריט | תיאור |
|------|--------|
| **מטרה** | לוודא שמלאי יורד אחרי ביקור עם תרופה |
| **Preconditions** | תרופה עם מלאי 100; חיה קיימת |
| **Steps** | 1. צור ביקור 2. הוסף טיפול עם התרופה (כמות 5) 3. שמור |
| **Expected** | מלאי התרופה יורד ל-95 |

---

### 4.2 שתי בדיקות GUI

**GT-01 — פריסת מסך ביקורים**

| פריט | תיאור |
|------|--------|
| **מטרה** | לוודא שמסך הביקורים שלם |
| **Steps** | 1. התחבר כ-vet 2. פתח "ביקורים וטיפולים" |
| **Expected** | שדה שבב חיה, תאריך, סיבה, אזור טיפולים, יומן ביקורים, כפתור "שמור ביקור" |

**GT-02 — טופס הוספת תרופה**

| פריט | תיאור |
|------|--------|
| **מטרה** | לוודא טופס תרופות נפתח |
| **Steps** | 1. פתח מסך תרופות 2. לחץ "הוסף תרופה חדשה" |
| **Expected** | שדות שם, מלאי, מחיר, תאריך תפוגה, הערות; כפתורי שמור ומחק (מחק רק בעריכה) |

---

## 5. תרחיש בדיקה + 2 מקרי בדיקה + תסריטי בדיקה

### תרחיש (Scenario)

**שם התרחיש:** הוספת תרופה למלאי ורישום ביקור שמשתמש בה  
**User Story:** US-02 + US-01  
**תיאור:** מוסיפים תרופה חדשה, ואז יוצרים ביקור עם שורת טיפול שמורידה מהמלאי.

---

### תסריט בדיקה 1 — TC-S01

| שדה | תוכן |
|-----|------|
| **Test Case ID** | TC-S01 |
| **Test Objective** | לוודא הוספת תרופה למלאי |
| **Preconditions** | מחובר כ-vet; שם "ויטמין D" לא קיים במלאי |
| **Test Type** | Functional — End to End (חלק ראשון) |

**Steps:**

1. פתח מסך תרופות
2. לחץ "הוסף תרופה חדשה"
3. שם: `ויטמין D`
4. מלאי: `30`
5. מחיר: `45`
6. תאריך תפוגה: בעוד 12 חודשים
7. לחץ "שמור תרופה"

| **Expected Result** | הודעה "התרופה נוספה בהצלחה"; התרופה מופיעה ברשימה |
| **Actual Result** | הודעה "התרופה נוספה בהצלחה"; כרטיס "ויטמין D" מופיע עם מלאי 30 |
| **Pass/Fail** | **Pass** |

---

### תסריט בדיקה 2 — TC-S02

| שדה | תוכן |
|-----|------|
| **Test Case ID** | TC-S02 |
| **Test Objective** | לוודא ביקור עם התרופה מפחית מלאי |
| **Preconditions** | TC-S01 עבר; חיה עם שבב `3761234` קיימת; מלאי ויטמין D = 30 |
| **Test Type** | Functional — End to End (חלק שני) |

**Steps:**

1. פתח מסך ביקורים
2. הזן שבב: `3761234`
3. בחר תאריך עתידי
4. סיבה: "בדיקה שגרתית"
5. הוסף שורת טיפול: ויטמין D, כמות 2
6. לחץ "שמור ביקור"

| **Expected Result** | "הביקור נשמר בהצלחה"; מלאי ויטמין D = 28 |
| **Actual Result** | הודעת הצלחה; מלאי ויטמין D ירד ל-28; ביקור מופיע ביומן |
| **Pass/Fail** | **Pass** |

---

### תסריט בדיקה 3 — TC-S03 (מחיקה שלילית)

| שדה | תוכן |
|-----|------|
| **Test Case ID** | TC-S03 |
| **Test Objective** | לוודא שלא ניתן למחוק תרופה שבשימוש |
| **Preconditions** | TC-S02 עבר; ויטמין D משויך לביקור |
| **Test Type** | Functional — Negative |

**Steps:**

1. פתח מסך תרופות
2. בחר "ויטמין D"
3. לחץ "מחק תרופה"

| **Expected Result** | הודעה: לא ניתן למחוק תרופה שמשויכת לביקור; התרופה נשארת |
| **Actual Result** | הודעת חסימה מוצגת; ויטמין D עדיין ברשימה עם מלאי 28 |
| **Pass/Fail** | **Pass** |

---

## 6. Boundary Value Testing (בדיקות ערך גבול)

### טבלת גבולות — כמות מלאי (`IsValidStockQuantity`)

| מזהה | ערך | תוצאה צפויה |
|------|-----|-------------|
| B-ST1 | `-1` | **לא תקין** |
| B-ST2 | `0` | **תקין** (גבול תחתון) |
| B-ST3 | `100` | **תקין** |
| B-ST4 | `abc` | **לא תקין** (לא int) |

---

### טבלת גבולות — מחיר יחידה (`IsValidMoney`)

| מזהה | ערך | תוצאה צפויה |
|------|-----|-------------|
| B-PR1 | `-0.01` | **לא תקין** |
| B-PR2 | `0` | **תקין** |
| B-PR3 | `99.99` | **תקין** |
| B-PR4 | `free` | **לא תקין** |

---

### טבלת גבולות — תאריך תפוגה (`IsValidExpirationDate`)

| מזהה | ערך | תוצאה צפויה |
|------|-----|-------------|
| B-EX1 | אתמול | **לא תקין** |
| B-EX2 | היום | **תקין** (גבול) |
| B-EX3 | בעוד שנה | **תקין** |

---

### טבלת גבולות — תאריך ביקור (`IsValidVisitDate` + לא בעבר)

| מזהה | ערך | תוצאה צפויה |
|------|-----|-------------|
| B-V1 | `1999-01-01` | **לא תקין** (לפני 2000) |
| B-V2 | `2000-01-01` | **תקין** (פורמט) |
| B-V3 | אתמול | **לא תקין** (בעבר — `IsVisitDateTimeInPast`) |
| B-V4 | מחר | **תקין** |

---

### טבלת גבולות — סיבת ביקור (`IsRequiredText`)

| מזהה | ערך | תוצאה צפויה |
|------|-----|-------------|
| B-R1 | `` (ריק) | **לא תקין** |
| B-R2 | `חיסון` | **תקין** |
| B-R3 | `   ` (רווחים) | **לא תקין** |

---

## 7. Decision Table ו-Decision Tree — החלטת מחיקת תרופה

### תנאים (Conditions)

| # | תנאי | משמעות |
|---|------|--------|
| C1 | תרופה נבחרה | `FindSelectedOrTypedMedication()` לא null |
| C2 | לא בשימוש בביקור | אין `Visit` עם `MedicationName` ו-`MedicationQuantity > 0` |

### פעולות (Actions)

| # | פעולה |
|---|--------|
| A1 | **מחק תרופה** — הסרה מ-`AppData.Medications` + שמירה |
| A2 | **הצג שגיאה — לא נבחרה** | "בחר תרופה מהרשימה לפני מחיקה" |
| A3 | **חסום מחיקה** | "לא ניתן למחוק תרופה שמשויכת לביקור..." |

*מימוש: `MedicationsView.DeleteMedication_Click` — שורות 175–205*

---

### Decision Table — טבלת החלטות

| כלל | C1 נבחרה | C2 לא בשימוש | פעולה |
|-----|----------|--------------|--------|
| R1 | לא | — | A2 |
| R2 | כן | לא | A3 |
| R3 | כן | כן | A1 |

---

### Decision Tree — עץ החלטות (מחיקת תרופה)

```
           [לחיצה על "מחק תרופה"]
                    |
         [האם תרופה נבחרה?]
            /              \
          לא                כן
           |                 |
      [A2 שגיאה]    [משויכת לביקור?]
                      /           \
                    כן             לא
                     |              |
                [A3 חסום]      [A1 מחק]
```

---

### Decision Table — שמירת ביקור (2 תנאים נוספים)

| כלל | חיה קיימת | יש טיפול | פעולה |
|-----|-----------|----------|--------|
| R1 | לא | — | שגיאה: "יש לבחור חיה קיימת..." |
| R2 | כן | לא | שגיאה: "יש להוסיף לפחות טיפול..." |
| R3 | כן | כן | שמור ביקור + עדכן מלאי |

---

## נספח — מיפוי לפרויקט

| נושא | קובץ |
|------|------|
| שמירת ביקור | `Source/Frontend/Views/VisitsView.axaml.cs` — `SaveVisit_Click` |
| קריאת שדות ביקור | `TryReadVisitFields` |
| הוספת תרופה | `MedicationsView.AddMedication_Click` |
| מחיקת תרופה | `MedicationsView.DeleteMedication_Click` |
| ולידציה | `Source/Services/ValidationService.cs` |
| מלאי בביקור | `ApplyPendingTreatmentLinesStock`, `RestoreMedicationStock` |

---

*מסמך זה מתאר ביקורים/טיפולים ומלאי תרופות בלבד. לא בוצעו שינויים בקוד.*
