# Logic App Workflow - Stage Inspection Report via Stored Procedure

This Logic App workflow receives XML reports via HTTP, extracts a **ReportID**, and stages the full XML payload into SQL using a stored procedure.  
If the `ReportID` field is missing, the request is rejected.

---

## 📌 Workflow Overview

### 1. **Trigger**
- **When an HTTP request is received**
- Accepts incoming XML payload via `POST`.
<img width="1328" height="659" alt="image" src="https://github.com/user-attachments/assets/756d89e3-04d2-4472-b024-95b36071611a" />

### 2. **Get Secret (Key Vault)**
- Retrieves the SQL connection string from **Azure Key Vault** (`J-SqlConnectionString`).
- Uses **Managed Service Identity (MSI)** for secure authentication.
<img width="1355" height="657" alt="image" src="https://github.com/user-attachments/assets/89af6b84-9721-4161-bfc0-e62dd29fd7d3" />

### 3. **Extract Report ID**
- Extracts `ReportID` from the XML payload header using XPath:
  ```xml
  /*/Header/ReportID/text()
  ```
<img width="1322" height="659" alt="image" src="https://github.com/user-attachments/assets/8ee1e5ca-3c8c-41bf-bb28-419dee1abdb0" />

### 4. **Get Raw XML Body**
- Captures the full raw XML body for database staging.
<img width="1330" height="595" alt="image" src="https://github.com/user-attachments/assets/af32152f-450a-4522-9c7d-22156089144e" />

### 5. **Condition**
<img width="1333" height="652" alt="image" src="https://github.com/user-attachments/assets/7eed9a5b-b930-495e-bb26-0e59f8bec40c" />

- If `ReportID` is present → Calls SQL stored procedure `[dbo].[usp_StageInspectionReport]` with parameters:
  - `RawXML` → Entire XML body
  - `ReportID` → Extracted Report ID
<img width="1349" height="655" alt="image" src="https://github.com/user-attachments/assets/e184b550-9d3b-4b32-b0b7-3ba657eeb5f9" />
<img width="1327" height="169" alt="image" src="https://github.com/user-attachments/assets/bfedb81a-a9fb-4965-adaa-5b218a0aa970" />

- If `ReportID` is missing → Returns HTTP `400` with rejection message.

### 6. **Response**
- Returns JSON response:
  - **202 Accepted** → If report successfully staged by stored procedure.
<img width="1477" height="572" alt="image" src="https://github.com/user-attachments/assets/822c6d61-f0c9-48c1-8bd6-08d935681365" />

  - **400 Bad Request** → If `ReportID` is missing.
<img width="1180" height="640" alt="image" src="https://github.com/user-attachments/assets/70737b33-ae9a-4d5f-bdd7-f21166339cd6" />

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
<InspectionReport>
    <Header>
        <ReportID>VID-2025-A99</ReportID>
        <InspectorName>Sarah Connor</InspectorName>
    </Header>
    <Details>
        <BrakePadThickness>8mm</BrakePadThickness>
    </Details>
</InspectionReport>
```
<img width="1449" height="729" alt="image" src="https://github.com/user-attachments/assets/dd0b3a44-38cd-4932-a4f4-321b0ed91930" />

```xml
<InspectionReport>
    <Header>
        <ReportID>VID-2025-A100</ReportID>
        <InspectorName>James Watson</InspectorName>
    </Header>
    <Details>
        <BrakePadThickness>10mm</BrakePadThickness>
    </Details>
</InspectionReport>
```
<img width="1446" height="724" alt="image" src="https://github.com/user-attachments/assets/dda7f29a-03af-42b7-bb0f-21c9be778b81" />

## Inside the Database
<img width="1171" height="286" alt="image" src="https://github.com/user-attachments/assets/29fc321c-f34d-4b38-b7f2-08b4c1b11079" />

## Workflow Diagram
<img width="1046" height="653" alt="image" src="https://github.com/user-attachments/assets/1fed9147-18af-4e63-ba38-5bf82ded04d4" />
<img width="1043" height="581" alt="image" src="https://github.com/user-attachments/assets/179d8008-36ce-4e1b-8dca-4bff94032a33" />


---
