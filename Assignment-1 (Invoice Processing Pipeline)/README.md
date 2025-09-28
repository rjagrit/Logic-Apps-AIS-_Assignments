# 📌 Azure Logic App – Service Bus Invoice Processing

This Logic App listens to messages from a Service Bus queue, validates invoice data, and stores the results in Azure Blob Storage. Invalid messages are moved to the Dead Letter Queue (DLQ).

---
## Pre-requisites
1. **Service Bus Connection**
   - Requires a **Service Bus namespace**.
   - Use the **Primary Connection String** from the Service Bus (Shared access policies).
   - Needed for:
     - Trigger (*When a message is received in a queue*)
     - Dead-letter action
     - Complete message action

2. **Azure Storage Connection**
   - Requires a **Storage Account**.
   - Use the **Access Key** (from the Storage Account → Access Keys).
   - Needed for:
     - Create Blob actions

⚠️ Without these connections, the Logic App actions will not run.
## ⚡ Workflow Steps

### **Step 1: Trigger**

* **Action** → Service Bus → *When a message is received in a queue (peek-lock)*
* **Queue name** → `jagrit-queue-03`
<img width="1545" height="700" alt="Screenshot 2025-09-20 235805" src="https://github.com/user-attachments/assets/fac8608f-1599-43e1-b621-1877591182f0" />

---

### **Step 2: Decode the Message Content**

* **Action** → Compose  
* **Inputs**:

```text
base64ToString(triggerBody()?['ContentData'])
```
<img width="1542" height="705" alt="Screenshot 2025-09-20 235853" src="https://github.com/user-attachments/assets/47abe8c1-54a1-4e65-aee2-363ebc64d084" />

---

### **Step 3: Parse JSON**

* **Action** → Parse JSON  
* **Content** → Output of **Compose**  
* **Schema**:

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
<img width="1547" height="701" alt="Screenshot 2025-09-20 235945" src="https://github.com/user-attachments/assets/9aeb249f-6846-46ec-a039-bc30b52aff6b" />

---

### **Step 4: Condition – Validate Amount**

* **Action** → Condition  
* **Expression**:

```text
@lessOrEquals(body('Parse_JSON')?['Amount'], 0)
```
<img width="1548" height="710" alt="Screenshot 2025-09-21 000020" src="https://github.com/user-attachments/assets/99ac6bb3-1064-4ce7-815c-36c3cecec656" />

**👉 True branch (Amount ≤ 0):**  
* Action → Service Bus → *Dead-letter the message*  
  * Lock Token → `@triggerBody()?['LockToken']`  
  * Optional reason → `"Invalid amount"`
<img width="1544" height="699" alt="Screenshot 2025-09-21 000109" src="https://github.com/user-attachments/assets/929a1cca-007d-498c-890f-f47cfefacbef" />

**👉 False branch (Amount > 0):**  
→ Continue with normal processing
<img width="1544" height="704" alt="Screenshot 2025-09-21 000225" src="https://github.com/user-attachments/assets/116c8a8f-d445-4c13-b0fe-f15159d72d21" />

---

### **Step 5: Store Invoice as JSON file in Blob**

* **Action** → Azure Blob Storage → *Create Blob*  
* **Container name** → `invoices`  
* **Blob name**:

```text
concat('invoice-', body('Parse_JSON')?['InvoiceId'], '.json')
```
* **Blob content**:

```text
outputs('Compose')
```
<img width="1544" height="704" alt="Screenshot 2025-09-21 000225" src="https://github.com/user-attachments/assets/b49dc0c1-9ef8-4196-9703-faec87a11805" />

---

### **Step 6: Store Invoice Summary as TXT file in Blob**

* **Action** → Azure Blob Storage → *Create Blob*  
* **Container name** → `invoices`  
* **Blob name**:

```text
concat('summary-', body('Parse_JSON')?['InvoiceId'], '.txt')
```

* **Blob content**:

```text
Invoice Summary
-----------------
Invoice ID: @{body('Parse_JSON')?['InvoiceId']}
Customer: @{body('Parse_JSON')?['CustomerName']}
Amount: @{body('Parse_JSON')?['Amount']}
Date: @{body('Parse_JSON')?['Date']}
```
<img width="1543" height="701" alt="Screenshot 2025-09-21 000247" src="https://github.com/user-attachments/assets/1e275a38-a9e4-4115-8f88-b8956b9e5d89" />

---

### **Step 7: Complete the Service Bus Message**

* **Action** → Service Bus → *Complete the message*  
* **Lock Token** → `@triggerBody()?['LockToken']`
<img width="1541" height="702" alt="Screenshot 2025-09-21 000413" src="https://github.com/user-attachments/assets/49f1bcdc-6800-4c72-a9d2-66a7bf930c14" />

---

## ✅ Summary

- Messages from **Service Bus queue** are received in **peek-lock mode**.  
- The message content is **decoded from Base64 → JSON**.  
- If **Amount ≤ 0**, the message is **dead-lettered**.  
- If **Amount > 0**, the invoice is:  
  1. Stored as a **JSON file** in Blob Storage.  
  2. Stored as a **TXT summary** in Blob Storage.  
- Finally, the Service Bus message is **completed**.

---
## Output:
### Demo-1
```json
{
  "InvoiceId": "INV-1001",
  "CustomerName": "Jagrit Enterprises",
  "Amount": 1500,
  "Date": "2025-09-20"
}
```
-Send the message in the Service bus queues
<img width="1543" height="690" alt="Screenshot 2025-09-21 001446" src="https://github.com/user-attachments/assets/c421e711-c843-43a7-8f2a-05fa17a4022f" />

-Peek at the message. After that, the workflow will start
<img width="1555" height="654" alt="Screenshot 2025-09-21 001542" src="https://github.com/user-attachments/assets/60cf08b8-a08d-4e2a-b9e6-502c9f521b30" />

-Inside the Storage account Container, we found our files
JSON File
<img width="1102" height="579" alt="Screenshot 2025-09-21 001854" src="https://github.com/user-attachments/assets/85960182-649e-4121-b07a-bffbc7d8a515" />

Txt file
<img width="1503" height="628" alt="Screenshot 2025-09-21 002015" src="https://github.com/user-attachments/assets/93ee1838-3290-4bb4-befc-9dc5a2572a5d" />


### Demo-2
```json
{
    "InvoiceId": "INV-1002",
    "CustomerName": "Test Customer",
    "Amount": 0,
    "Date": "2025-09-20"
}
```
-Inside the DLQ
<img width="1360" height="687" alt="Screenshot 2025-09-21 003645" src="https://github.com/user-attachments/assets/790c968d-c3ae-4f48-8366-4b554a25efcf" />

## The Workflow diagram
<img width="1002" height="395" alt="Screenshot 2025-09-21 000811" src="https://github.com/user-attachments/assets/aed67035-7b3b-4a1c-9261-a5761267684c" />
<img width="1002" height="650" alt="Screenshot 2025-09-21 000840" src="https://github.com/user-attachments/assets/6a8731b5-bff9-430d-8319-f20e170108a4" />



