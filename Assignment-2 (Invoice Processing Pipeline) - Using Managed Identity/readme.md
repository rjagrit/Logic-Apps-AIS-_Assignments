# Logic App with Managed Identity

This guide helps you recreate a Logic App with Managed Identity in Azure, including triggers, expressions, and a test sample.

---

## Prerequisites

* Azure subscription with appropriate permissions.
* Resource Group.
* Service Bus namespace with a queue (e.g., `jagrit-queue-03`).
* Storage account with container `invoices`.
* Logic App (Standard recommended).

---

## 1) Create Resources

**Service Bus**
1. Create namespace.
<img width="1907" height="609" alt="Screenshot 2025-09-28 110210" src="https://github.com/user-attachments/assets/46520666-5455-4621-b3da-fbbaffda4cca" />

2. Create queue named `jagrit-queue-03`.
<img width="1907" height="609" alt="Screenshot 2025-09-28 110210" src="https://github.com/user-attachments/assets/064d8482-a35f-471c-8085-7c4ae5626597" />

**Storage Account**
1. Create account.
<img width="1919" height="571" alt="Screenshot 2025-09-28 110320" src="https://github.com/user-attachments/assets/5dc9dba7-fbd2-4b1b-8e10-82c15e868941" />

2. Create container `invoices`.
<img width="1918" height="334" alt="Screenshot 2025-09-28 110345" src="https://github.com/user-attachments/assets/66982fb6-4ec1-408f-98c7-ff7f2648cc4c" />

---

## 2) Create Logic App & Enable Managed Identity

1. Create **Logic App (Standard)**.
2. Go to **Identity** → **System assigned** → **On** → Save.
3. Copy the Object ID for RBAC.

---

## 3) Assign RBAC Roles

**A. Service Bus**
* Role: **Azure Service Bus Data Receiver**
* Assign to Logic App MI at namespace or queue scope.

**B. Blob Storage**
* Role: **Storage Blob Data Contributor**
* Assign at container `invoices`.
---

## 4) Build Logic App Workflow

**Trigger**: Service Bus - When a message is received in a queue (peek-lock)
* Authentication: Managed Identity
* Queue: `jagrit-queue-03`
* Rename to `SB_Trigger`
<img width="1487" height="650" alt="Screenshot 2025-09-28 110538" src="https://github.com/user-attachments/assets/67ea3912-91a5-497c-bb90-63920998c36f" />
<img width="1488" height="656" alt="Screenshot 2025-09-28 110554" src="https://github.com/user-attachments/assets/05e303fc-9013-445c-98a3-26a2ca036383" />

**Action 1: Compose**
* Name: `DecodeCompose`
* Inputs: `base64ToString(triggerBody()?['ContentData'])`
<img width="1310" height="648" alt="Screenshot 2025-09-28 110741" src="https://github.com/user-attachments/assets/65541f9d-2047-4579-9e2c-176612be4548" />

**Action 2: Parse JSON**
* Name: `Parse_JSON`
* Content: `outputs('DecodeCompose')`
* Schema:
```json
{
  "type": "object",
  "properties": {
    "InvoiceId": { "type": "string" },
    "CustomerName": { "type": "string" },
    "Amount": { "type": "number" },
    "Date": { "type": "string" }
  }
}
```
<img width="1340" height="654" alt="Screenshot 2025-09-28 110720" src="https://github.com/user-attachments/assets/f8c2b6fb-5b7b-40ae-885d-586256142ee2" />
<img width="1310" height="648" alt="Screenshot 2025-09-28 110741" src="https://github.com/user-attachments/assets/5388dfdf-1b67-4a4f-b6d7-ca8f20480dfe" />


**Action 3: Condition**
* Name: `Condition_ValidateAmount`
* Expression: `@lessOrEquals(body('Parse_JSON')?['Amount'], 0)`
<img width="1311" height="497" alt="Screenshot 2025-09-28 110757" src="https://github.com/user-attachments/assets/1b08c956-18db-40ef-b0ab-4ea676afa861" />

**True Branch**
* Dead-letter message using MI
* Lock Token: `@triggerBody()?['LockToken']`
* Reason: `"Invalid amount"`
<img width="1315" height="652" alt="Screenshot 2025-09-28 110816" src="https://github.com/user-attachments/assets/59cbfa99-82ca-4226-a884-5295adf626e5" />

**False Branch**
* Create blob:
  - Blob name: `concat('invoice-', body('Parse_JSON')?['InvoiceId'], '.json')`
  - Content: `outputs('DecodeCompose')`
<img width="1389" height="654" alt="Screenshot 2025-09-28 112253" src="https://github.com/user-attachments/assets/550b4582-83f2-4507-a380-bc6fb3e37df3" />
<img width="1296" height="396" alt="Screenshot 2025-09-28 112321" src="https://github.com/user-attachments/assets/bd010c3e-577d-4c63-ba01-305bd1ff0bf5" />

* Create summary blob:
  - Blob name: `concat('summary-', body('Parse_JSON')?['InvoiceId'], '.txt')`
  - Content:
```text
concat('Invoice Summary\n-----------------\nInvoice ID: ', body('Parse_JSON')?['InvoiceId'],
       '\nCustomer: ', body('Parse_JSON')?['CustomerName'],
       '\nAmount: ', string(body('Parse_JSON')?['Amount']),
       '\nDate: ', body('Parse_JSON')?['Date'])
```
<img width="1382" height="626" alt="Screenshot 2025-09-28 112340" src="https://github.com/user-attachments/assets/6beca32c-e7bb-4937-b525-75f20ffcdf6d" />
<img width="1296" height="391" alt="Screenshot 2025-09-28 112353" src="https://github.com/user-attachments/assets/7168795f-bcd8-4896-a894-1a6ae1024368" />

* Complete Service Bus message using MI
<img width="1381" height="660" alt="Screenshot 2025-09-28 112405" src="https://github.com/user-attachments/assets/ba1f9627-e803-433d-85be-9d963c89df9b" />

---

## 5) Test the Workflow
## Output:
### Demo-1
```json
{
  "InvoiceId": "INV-111",
  "CustomerName": "Jai Enterprises",
  "Amount": 1500,
  "Date": "2025-09-20"
}
```
-Inside the Storage account Container, we found our files
JSON File
<img width="1588" height="748" alt="Screenshot 2025-09-28 112615" src="https://github.com/user-attachments/assets/f00b568d-e613-48f6-a0c1-a9cf35654391" />

Txt file
<img width="1578" height="737" alt="Screenshot 2025-09-28 112649" src="https://github.com/user-attachments/assets/526946ba-dd97-4460-ac1a-c97175a90cfc" />


### Demo-2
```json
{
    "InvoiceId": "INV-112",
    "CustomerName": "Test Customer",
    "Amount": 0,
    "Date": "2025-09-20"
}
```
-Inside the DLQ
<img width="1916" height="773" alt="Screenshot 2025-09-28 112758" src="https://github.com/user-attachments/assets/a6b03ab4-8e1a-460a-a463-69d8baedfe4a" />

## The Workflow diagram
<img width="779" height="455" alt="Screenshot 2025-09-28 112903" src="https://github.com/user-attachments/assets/01043f7d-9dfe-4f31-b930-d8b161379dd3" />
<img width="781" height="559" alt="Screenshot 2025-09-28 112957" src="https://github.com/user-attachments/assets/5fefbb2f-485d-439c-9f52-035335787d26" />

