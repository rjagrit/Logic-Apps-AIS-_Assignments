# Logic App: HTTP to SQL with Key Vault Secret

This Logic App demonstrates how to receive an HTTP POST request, extract data from XML, retrieve a secret from Azure Key Vault, and insert the data into an Azure SQL Database.
---

## Steps to Implement

### 1. **HTTP Trigger**
- **Trigger Name:** `When_an_HTTP_request_is_received`
- **Type:** HTTP Request
- **Method:** POST
- This trigger listens for incoming HTTP POST requests.
<img width="1380" height="654" alt="image" src="https://github.com/user-attachments/assets/9f39aaf6-6d26-4702-b9f3-f12f8984b876" />

### 2. **Get Secret from Key Vault**
- **Action Name:** `Get_secret`
- **Type:** ApiConnection
- **Connection:** Azure Key Vault (`keyvault-3`)
- **Path:** `/secrets/J-SqlConnectionString/value`
- This action retrieves the SQL connection string securely from Key Vault.
- **Runtime Configuration:** The inputs and outputs are marked as secure.
<img width="1358" height="656" alt="image" src="https://github.com/user-attachments/assets/e982abf6-4717-4364-96c5-285fc07a0d0a" />

### 3. **Extract Reference Number from XML**
- **Action Name:** `Extract_Reference_Number`
- **Type:** Compose
- **Input:** `@xpath(xml(triggerBody()),'/*/Header/Reference/text()')`
- This extracts the Reference number from the XML request body.
<img width="1304" height="665" alt="image" src="https://github.com/user-attachments/assets/4009dd32-f7cd-4324-9e47-3bb223c81c62" />

### 4. **Extract Customer Name from XML**
- **Action Name:** `Extract_Customer_Name`
- **Type:** Compose
- **Input:** `@xpath(xml(triggerBody()), '/*/Header/Customer/text()')`
- This extracts the Customer name from the XML request body.
<img width="1322" height="656" alt="image" src="https://github.com/user-attachments/assets/294ad516-595c-4234-b77f-b6caf8a37998" />

### 5. **Insert Data into SQL Table**
- **Action Name:** `Insert_row_(V2)`
- **Type:** ApiConnection
- **Connection:** Azure SQL (`sql`)
- **Method:** POST
- **Path:** `/v2/datasets/default,default/tables/[dbo].[IncomingData]/items`
- **Body:**
  ```json
  {
    "ReferenceNumber": "@{outputs('Extract_Reference_Number')}",
    "CustomerName": "@{outputs('Extract_Customer_Name')}"
  }
  ```
- This action inserts the extracted data into the SQL table `[dbo].[IncomingData]`.
<img width="1327" height="659" alt="image" src="https://github.com/user-attachments/assets/f16f3bdf-c0c5-476a-9bb0-97f2d78d2000" />

### 6. **Connections Parameter**
- The Logic App uses `$connections` parameter to manage connections to Key Vault and SQL.

---

## Summary
- **Trigger:** HTTP POST request
- **Retrieve Secret:** Azure Key Vault
- **Extract Data:** XML parsing using XPath
- **Insert into SQL:** Azure SQL Table `[dbo].[IncomingData]`
- **Secure:** Sensitive data is protected via `secureData` configuration

---

# The Output
## a) Passing the Data in Postman (use for testing API)
```json
<IncomingOrder>
    <Header>
        <Reference>ORD-2025-4567</Reference>
        <Customer>Global Logistics Co.</Customer>
    </Header>
</IncomingOrder>
  ```
<img width="1443" height="773" alt="image" src="https://github.com/user-attachments/assets/ae7b5f3b-d150-4722-b61c-70a53c72ea43" />

## b) Data inserted in the table
<img width="936" height="242" alt="image" src="https://github.com/user-attachments/assets/30607baa-f54c-4688-ae8a-943f1c29c63f" />

### The Workflow
<img width="1021" height="660" alt="image" src="https://github.com/user-attachments/assets/a161c431-890d-4aa5-92b9-bab02f996ef3" />


