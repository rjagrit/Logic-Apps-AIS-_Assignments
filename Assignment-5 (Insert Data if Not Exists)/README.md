# Logic App Workflow - Insert Data if Not Exists

This Logic App workflow listens for an **HTTP POST request** containing XML data, extracts key fields, checks for existing records in a SQL table, and conditionally inserts a new record if it does not already exist.

---

## 📌 Workflow Steps

1. **Trigger**
   - `When an HTTP request is received`
   - Accepts incoming XML payload via POST request.
<img width="1380" height="654" alt="Screenshot 2025-09-30 112930" src="https://github.com/user-attachments/assets/3f1048d1-bab2-46db-8eb7-e41fab0f6509" />

2. **Get Secret (Key Vault)**
   - Retrieves SQL connection string securely from **Azure Key Vault**.
<img width="1358" height="656" alt="Screenshot 2025-09-30 112947" src="https://github.com/user-attachments/assets/d68e723b-a059-4be8-9205-85f0feb91d66" />

3. **Extract Fields**
   - Extracts `ReferenceNumber` and `CustomerName` from the XML payload using XPath.
<img width="1304" height="665" alt="Screenshot 2025-09-30 113005" src="https://github.com/user-attachments/assets/e53d3744-7943-4130-9186-7e9ad209d79e" />
<img width="1322" height="656" alt="Screenshot 2025-09-30 113020" src="https://github.com/user-attachments/assets/65cbdde6-ee96-4b1f-932f-f960d475ca2e" />

4. **Check Existing Record**
   - Queries the SQL table `[dbo].[IncomingData]` for the given `ReferenceNumber`.
<img width="1345" height="646" alt="image" src="https://github.com/user-attachments/assets/c9201df7-1901-4114-b82e-42261c0824f0" />

5. **Condition**
   <img width="1278" height="650" alt="image" src="https://github.com/user-attachments/assets/1f974122-b656-4c1f-a7cd-996ec4c90cc0" />

    - If the record **does not exist** → Inserts a new row with `ReferenceNumber` and `CustomerName`.
   <img width="1347" height="655" alt="image" src="https://github.com/user-attachments/assets/e8f31fca-f547-46ea-9cf4-4a0f316abdde" />

   - If the record **already exists** → Skips insertion.
   
7. **Response**
   - Returns JSON response:
     - `201 Created` if a new record is inserted.
     <img width="1345" height="652" alt="image" src="https://github.com/user-attachments/assets/874b7e8b-2f65-482a-9fb2-25f219d45a2c" />

     - `200 OK` if record already exists.
     <img width="1282" height="657" alt="image" src="https://github.com/user-attachments/assets/6b60b370-6b22-4f15-8ead-2be07cd31282" />

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
<img width="1446" height="724" alt="image" src="https://github.com/user-attachments/assets/78a0e27a-4beb-4131-954f-f52532c48265" />

### If Record Exists
```json
{
  "status": "Record Skipped",
  "ReferenceNumber": "12345",
  "message": "Record already exists, insertion skipped."
}
```
<img width="1443" height="724" alt="image" src="https://github.com/user-attachments/assets/78de6dc3-a141-466e-a9bd-cdfc203ed895" />

---

## The Workflow
<img width="1078" height="654" alt="image" src="https://github.com/user-attachments/assets/27173dc7-8b80-4645-a6d2-f41b9004f15c" />
<img width="1086" height="658" alt="image" src="https://github.com/user-attachments/assets/f847bdd8-6ac0-43a7-ba11-f1cc9d438e73" />

## 📖 Summary

This Logic App ensures **idempotent inserts** by checking for existing records before inserting new data.  
It improves **data consistency**, prevents duplicates, and uses **Key Vault** for secure secrets management.
