# Logic App Workflow - Stage Inspection Report via Stored Procedure

This Logic App workflow receives XML reports via HTTP, extracts a **ReportID**, and stages the full XML payload into SQL using a stored procedure.  
If the `ReportID` field is missing, the request is rejected.

---

## 📌 Workflow Overview

### 1. **Trigger**
- **When an HTTP request is received**
- Accepts incoming XML payload via `POST`.

### 2. **Get Secret (Key Vault)**
- Retrieves the SQL connection string from **Azure Key Vault** (`J-SqlConnectionString`).
- Uses **Managed Service Identity (MSI)** for secure authentication.

### 3. **Extract Report ID**
- Extracts `ReportID` from the XML payload header using XPath:
  ```xml
  /*/Header/ReportID/text()
  ```

### 4. **Get Raw XML Body**
- Captures the full raw XML body for database staging.

### 5. **Condition**
- If `ReportID` is present → Calls SQL stored procedure `[dbo].[usp_StageInspectionReport]` with parameters:
  - `RawXML` → Entire XML body
  - `ReportID` → Extracted Report ID
- If `ReportID` is missing → Returns HTTP `400` with rejection message.

### 6. **Response**
- Returns JSON response:
  - **202 Accepted** → If report successfully staged by stored procedure.
  - **400 Bad Request** → If `ReportID` is missing.

---

## 📂 Connections

- **Key Vault Connection**
  - Retrieves the SQL connection string securely.
  - Authentication via **Managed Identity**.

- **SQL Connection**
  - Executes stored procedure `[dbo].[usp_StageInspectionReport]` in Azure SQL Database.

---

## 📤 Example XML Request

```xml
<Report>
  <Header>
    <ReportID>78910</ReportID>
    <Inspector>Jane Smith</Inspector>
  </Header>
  <Body>
    <InspectionData>...</InspectionData>
  </Body>
</Report>
```

---

## ✅ Example Responses

### Report Accepted
```json
{
  "status": "Accepted",
  "message": "XML report staged successfully by Stored Procedure.",
  "report_id": "78910"
}
```

### Report Rejected
```json
{
  "status": "Rejected",
  "error": "Missing_Key_Field",
  "message": "The incoming XML is missing the required ReportID field in the header."
}
```

---

## 🛠 Deployment Notes

1. Ensure Key Vault contains secret **`J-SqlConnectionString`**.
2. Assign **Managed Identity** permissions to both **Key Vault** and **SQL Database**.
3. Create stored procedure in SQL:

```sql
CREATE PROCEDURE [dbo].[usp_StageInspectionReport]
    @RawXML NVARCHAR(MAX),
    @ReportID NVARCHAR(100)
AS
BEGIN
    INSERT INTO [dbo].[StagedReports] (ReportID, RawXML, CreatedOn)
    VALUES (@ReportID, @RawXML, GETUTCDATE());
END;
```
4. Prepare SQL staging table if not already present:

```sql
CREATE TABLE [dbo].[StagedReports] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportID NVARCHAR(100) NOT NULL,
    RawXML NVARCHAR(MAX) NOT NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

---

## 📖 Summary

This workflow ensures inspection reports are **safely staged** into SQL using a stored procedure, with validation on required fields.  
It provides **secure secrets management**, **robust error handling**, and **idempotent data ingestion** for inspection workflows.
