# Logic App Workflow - Insert Data if Not Exists

This Logic App workflow listens for an **HTTP POST request** containing XML data, extracts key fields, checks for existing records in a SQL table, and conditionally inserts a new record if it does not already exist.

---

## 📌 Workflow Steps

1. **Trigger**
   - `When an HTTP request is received`
   - Accepts incoming XML payload via POST request.

2. **Get Secret (Key Vault)**
   - Retrieves SQL connection string securely from **Azure Key Vault**.

3. **Extract Fields**
   - Extracts `ReferenceNumber` and `CustomerName` from the XML payload using XPath.

4. **Check Existing Record**
   - Queries the SQL table `[dbo].[IncomingData]` for the given `ReferenceNumber`.

5. **Condition**
   - If the record **does not exist** → Inserts a new row with `ReferenceNumber` and `CustomerName`.
   - If the record **already exists** → Skips insertion.

6. **Response**
   - Returns JSON response:
     - `201 Created` if new record inserted.
     - `200 OK` if record already exists.

---

## 📂 Connections Used

- **Key Vault Connection**
  - Managed Identity authentication.
  - Retrieves secret: `J-SqlConnectionString`.

- **SQL Connection**
  - Connects to Azure SQL database.
  - Table: `[dbo].[IncomingData]`.

---

## ⚡ Example Request

```xml
<Order>
  <Header>
    <Reference>12345</Reference>
    <Customer>John Doe</Customer>
  </Header>
</Order>
```

---

## ✅ Example Responses

### If Record Inserted
```json
{
  "status": "Record Inserted",
  "ReferenceNumber": "12345",
  "message": "Record did not exist and was successfully inserted."
}
```

### If Record Exists
```json
{
  "status": "Record Skipped",
  "ReferenceNumber": "12345",
  "message": "Record already exists, insertion skipped."
}
```

---

## 🛠 Deployment Notes

- Ensure **Key Vault secret** (`J-SqlConnectionString`) is configured.
- Grant **Managed Identity access** to Key Vault and SQL Database.
- Update SQL table `[dbo].[IncomingData]` schema as needed:
  ```sql
  CREATE TABLE [dbo].[IncomingData] (
      Id INT IDENTITY(1,1) PRIMARY KEY,
      ReferenceNumber NVARCHAR(100) NOT NULL,
      CustomerName NVARCHAR(200) NOT NULL
  );
  ```

---

## 📖 Summary

This Logic App ensures **idempotent inserts** by checking for existing records before inserting new data.  
It improves **data consistency**, prevents duplicates, and uses **Key Vault** for secure secrets management.
