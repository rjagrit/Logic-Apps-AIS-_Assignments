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
<img width="1545" height="700" alt="image" src="https://github.com/user-attachments/assets/5d19d6a3-6b3e-4e7e-8767-249f8f679e1f" />

---

### **Step 2: Decode the Message Content**

* **Action** → Compose  
* **Inputs**:

```text
base64ToString(triggerBody()?['ContentData'])
```
<img width="1542" height="705" alt="image" src="https://github.com/user-attachments/assets/95850ed3-4386-4c13-9c2b-890b5d722db0" />

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
<img width="1547" height="701" alt="image" src="https://github.com/user-attachments/assets/4eeaffad-b088-4394-ad08-c871b81fc9dd" />

---

### **Step 4: Condition – Validate Amount**

* **Action** → Condition  
* **Expression**:

```text
@lessOrEquals(body('Parse_JSON')?['Amount'], 0)
```
<img width="1548" height="710" alt="image" src="https://github.com/user-attachments/assets/e036a6fb-5b1c-471e-8062-c13b6f20e18d" />

**👉 True branch (Amount ≤ 0):**  
* Action → Service Bus → *Dead-letter the message*  
  * Lock Token → `@triggerBody()?['LockToken']`  
  * Optional reason → `"Invalid amount"`
<img width="1544" height="699" alt="image" src="https://github.com/user-attachments/assets/570e51ce-2ba2-47d9-bfdd-ae7290d877fc" />

**👉 False branch (Amount > 0):**  
→ Continue with normal processing

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
<img width="1544" height="704" alt="image" src="https://github.com/user-attachments/assets/07f7b994-276f-4e2e-92fe-62bc69546b35" />

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
<img width="1543" height="701" alt="image" src="https://github.com/user-attachments/assets/69049ce8-eb4e-4011-aa88-6af4f0694c94" />

---

### **Step 7: Complete the Service Bus Message**

* **Action** → Service Bus → *Complete the message*  
* **Lock Token** → `@triggerBody()?['LockToken']`
<img width="1541" height="702" alt="image" src="https://github.com/user-attachments/assets/bd5af6ae-088b-4375-b5e3-947af8631251" />

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
<img width="1543" height="690" alt="image" src="https://github.com/user-attachments/assets/1d846c0c-0f45-449b-995f-f080036f3993" />

-Peek at the message. After that, the workflow will start
<img width="1555" height="654" alt="image" src="https://github.com/user-attachments/assets/ba6f02fb-ebce-4464-b957-d5d4f8d20915" />

-Inside the Storage account Container, we found our files
JSON File
<img width="1102" height="579" alt="image" src="https://github.com/user-attachments/assets/9da9facf-9e1e-46a5-ae3a-0721f7dd0b52" />

Txt file
<img width="1102" height="628" alt="image" src="https://github.com/user-attachments/assets/ddea2b26-e9ed-4abf-9269-ed7ceb1341e7" />

## The Workflow diagram
<img width="1002" height="395" alt="image" src="https://github.com/user-attachments/assets/e7f17e55-ce5b-4acf-916f-f84ddaca0d13" />
<img width="1009" height="650" alt="image" src="https://github.com/user-attachments/assets/0fef004d-bb6c-4eae-9727-74ecad4205ea" />



