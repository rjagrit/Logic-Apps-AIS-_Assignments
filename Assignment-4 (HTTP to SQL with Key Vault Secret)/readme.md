# README.md

# Logic App: HTTP to SQL with Key Vault Secret

This Logic App demonstrates how to receive an HTTP POST request, extract data from XML, retrieve a secret from Azure Key Vault, and insert the data into an Azure SQL Database.

---

## Steps to Implement

### 1. **HTTP Trigger**
- **Trigger Name:** `When_an_HTTP_request_is_received`
- **Type:** HTTP Request
- **Method:** POST
- This trigger listens for incoming HTTP POST requests.

### 2. **Get Secret from Key Vault**
- **Action Name:** `Get_secret`
- **Type:** ApiConnection
- **Connection:** Azure Key Vault (`keyvault-3`)
- **Path:** `/secrets/J-SqlConnectionString/value`
- This action retrieves the SQL connection string securely from Key Vault.
- **Runtime Configuration:** The inputs and outputs are marked as secure.

### 3. **Extract Reference Number from XML**
- **Action Name:** `Extract_Reference_Number`
- **Type:** Compose
- **Input:** `@xpath(xml(triggerBody()),'/*/Header/Reference/text()')`
- This extracts the Reference number from the XML request body.

### 4. **Extract Customer Name from XML**
- **Action Name:** `Extract_Customer_Name`
- **Type:** Compose
- **Input:** `@xpath(xml(triggerBody()), '/*/Header/Customer/text()')`
- This extracts the Customer name from the XML request body.

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

**Author:** Jagrit Rattan  
**Date:** 2025-09-30

