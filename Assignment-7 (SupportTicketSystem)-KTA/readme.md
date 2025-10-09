# 🧩 Azure Logic App: Create Ticket Workflow

This Logic App automates ticket creation and storage using Azure services. It integrates **HTTP trigger**, **Azure Function**, **SQL Database**, and **Blob Storage** to manage support tickets end-to-end.

---

## 🚀 Workflow Overview

1. **Trigger:** When an HTTP POST request is received with ticket data (`name`, `email`, `issue`).  
2. **Action 1:** Sends this data to an **Azure Function** that generates a ticket ID and status.  
3. **Action 2:** Parses the JSON response from the function.  
4. **Action 3:** Inserts the parsed data into a **SQL Database** table `[dbo].[Tickets]`.  
5. **Action 4:** Creates a **.txt file in Azure Blob Storage** with the ticket details.

---

## 🧱 Prerequisites

Before deploying, make sure you have:

- An **Azure Subscription**
- A **Resource Group** (e.g., `khoj-rg-dev-training`)
- A **SQL Database** with a table named `[dbo].[Tickets]` having columns:
  ```sql
  CREATE TABLE [dbo].[Tickets] (
      TicketId NVARCHAR(50),
      Name NVARCHAR(100),
      Email NVARCHAR(100),
      Issue NVARCHAR(255),
      Status NVARCHAR(50),
      CreatedAt DATETIME
  );
  ```
- An **Azure Function App** with an HTTP endpoint `/api/CreateTicket`
- A **Blob Storage Account** with a container (e.g., `jagrit-cont-03`)

---

## ⚙️ Step-by-Step Setup

### Step 1️⃣ – Create Logic App
1. Go to **Azure Portal → Create Resource → Logic App (Consumption)**.
2. Provide:
   - Name: `CreateTicketWorkflow`
   - Region: `East US`
   - Resource Group: `khoj-rg-dev-training`
3. Click **Create**.

---

### Step 2️⃣ – Add Trigger: HTTP Request
1. In the Logic App Designer, choose **When an HTTP request is received**.
2. Set **Method** to `POST`.
3. Add JSON Schema:
   ```json
   {
     "type": "object",
     "properties": {
       "name": { "type": "string" },
       "email": { "type": "string" },
       "issue": { "type": "string" }
     }
   }
   ```

---

### Step 3️⃣ – Add HTTP Action (Call Azure Function)
1. Add new action → **HTTP**.
2. Configure:
   - Method: `POST`
   - URI: `https://<your-function-url>/api/CreateTicket?code=<FUNCTION_KEY>`  
     ⚠️ **Important:** Replace `<FUNCTION_KEY>` with your actual Azure Function key during deployment. Do **not** commit the real key to GitHub or any public repository.
   - Body:
     ```json
     {
       "name": "@{triggerBody()?['name']}",
       "email": "@{triggerBody()?['email']}",
       "issue": "@{triggerBody()?['issue']}"
     }
     ```

---

### Step 4️⃣ – Parse JSON Response
1. Add new action → **Data Operations → Parse JSON**.
2. Content: `@body('HTTP')`
3. Schema:
   ```json
   {
     "type": "object",
     "properties": {
       "TicketId": { "type": "string" },
       "Name": { "type": "string" },
       "Email": { "type": "string" },
       "Issue": { "type": "string" },
       "Status": { "type": "string" },
       "CreatedAt": { "type": "string" }
     }
   }
   ```

---

### Step 5️⃣ – Insert Row into SQL Database
1. Add new action → **SQL Server → Insert row (V2)**.
2. Configure your SQL connection.
3. Table name: `[dbo].[Tickets]`
4. Map columns:
   - TicketId → `@body('Parse_JSON')?['TicketId']`
   - Name → `@body('Parse_JSON')?['Name']`
   - Email → `@body('Parse_JSON')?['Email']`
   - Issue → `@body('Parse_JSON')?['Issue']`
   - Status → `@body('Parse_JSON')?['Status']`
   - CreatedAt → `@body('Parse_JSON')?['CreatedAt']`

---

### Step 6️⃣ – Create Ticket File in Blob Storage
1. Add new action → **Azure Blob Storage → Create blob (V2)**.
2. Configure your Blob connection (use **Managed Identity** if available).
3. Set:
   - Folder path: `/jagrit-cont-03`
   - File name:
     ```
     tickets/@{formatDateTime(utcNow(),'yyyy-MM-dd')}/@{body('Parse_JSON')?['TicketId']}.txt
     ```
   - File content:
     ```
     Ticket ID: @{body('Parse_JSON')?['TicketId']}
     Name      : @{body('Parse_JSON')?['Name']}
     Email     : @{body('Parse_JSON')?['Email']}
     Issue     : @{body('Parse_JSON')?['Issue']}
     Status    : @{body('Parse_JSON')?['Status']}
     RaisedAt  : @{body('Parse_JSON')?['CreatedAt']}
     ```

---

### Step 7️⃣ – Test the Logic App
1. Click **Save** and copy the HTTP POST URL from the trigger.
2. Use **Postman** or **cURL** to send a test request:
   ```bash
   curl -X POST <HTTP_TRIGGER_URL> \\
   -H "Content-Type: application/json" \\
   -d '{"name": "John Doe", "email": "john.doe@company.com", "issue": "Login not working"}'
   ```
3. Verify:
   - Azure Function executes successfully.
   - SQL Database contains a new record.
   - Blob Storage contains a text file with ticket details.

---

## 🧩 Connections Used

| Connector | Connection Name | Type |
|------------|----------------|------|
| Azure SQL Database | `sql` | Standard Connection |
| Azure Blob Storage | `azureblob-4` | Managed Service Identity |

---

## 📁 JSON Definition
You can directly import the provided Logic App JSON file in Azure Portal → **Logic App Designer → Code View** and paste the JSON.

**⚠️ Security Note:** Before uploading, ensure all secrets (e.g., Function Keys, Connection Strings) are replaced with placeholders like `<FUNCTION_KEY>` or stored in **Azure Key Vault** / **GitHub Secrets**.

---

## ✅ Result
After deployment, the Logic App will:
- Accept ticket requests via HTTP
- Automatically generate a ticket via Function App
- Store the details in SQL Database
- Create a ticket file in Blob Storage for record keeping.

---

## 🧠 Author
**Jagrit**  
System Analyst Trainee @ Khoj Information Technology Pvt. Ltd.

