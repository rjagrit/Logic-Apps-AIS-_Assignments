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
<img width="1545" height="700" alt="Screenshot 2025-09-20 235805" src="https://github.com/user-attachments/assets/03a5fe11-45fe-4036-ba2c-36aa2deeead9" />

---

### **Step 2: Decode the Message Content**

* **Action** → Compose  
* **Inputs**:

```text
base64ToString(triggerBody()?['ContentData'])
```
<img width="1542" height="705" alt="Screenshot 2025-09-20 235853" src="https://github.com/user-attachments/assets/2bc42d88-5dac-4a02-abf6-a470d18758e3" />

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
<img width="1547" height="701" alt="Screenshot 2025-09-20 235945" src="https://github.com/user-attachments/assets/b8488e04-ecb0-4a4d-aa6d-16c8bbb28d45" />

---

### **Step 4: Condition – Validate Amount**

* **Action** → Condition  
* **Expression**:

```text
@lessOrEquals(body('Parse_JSON')?['Amount'], 0)
```
<img width="1548" height="710" alt="Screenshot 2025-09-21 000020" src="https://github.com/user-attachments/assets/8acee728-69f4-4371-a119-b365c8eca5b5" />

**👉 True branch (Amount ≤ 0):**  
* Action → Service Bus → *Dead-letter the message*  
  * Lock Token → `@triggerBody()?['LockToken']`  
  * Optional reason → `"Invalid amount"`
<img width="1544" height="699" alt="Screenshot 2025-09-21 000109" src="https://github.com/user-attachments/assets/b276c121-8e4e-4ab9-ab41-6691a5298d7d" />

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
<img width="1544" height="704" alt="Screenshot 2025-09-21 000225" src="https://github.com/user-attachments/assets/d5bb308c-a023-4564-a6b3-3dbb83619ae4" />

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
<img width="1543" height="701" alt="Screenshot 2025-09-21 000247" src="https://github.com/user-attachments/assets/3c33fe6e-e030-4c0d-97ea-d28effc6e976" />

---

### **Step 7: Complete the Service Bus Message**

* **Action** → Service Bus → *Complete the message*  
* **Lock Token** → `@triggerBody()?['LockToken']`
<img width="1541" height="702" alt="Screenshot 2025-09-21 000413" src="https://github.com/user-attachments/assets/15bf6109-4985-4fe5-9e45-ec147259bab1" />

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
<img width="1543" height="690" alt="Screenshot 2025-09-21 001446" src="https://github.com/user-attachments/assets/0d99552a-e2a5-48e9-bce3-89eb4501ab8a" />

-Peek at the message. After that, the workflow will start
<img width="1555" height="654" alt="Screenshot 2025-09-21 001542" src="https://github.com/user-attachments/assets/a457601e-7b4b-4783-8095-12e1f2a64732" />

-Inside the Storage account Container, we found our files
JSON File
<img width="1588" height="748" alt="image" src="https://github.com/user-attachments/assets/64065fe4-4e7d-4c2e-bd11-6c79ba730b09" />

Txt file
<img width="1578" height="737" alt="image" src="https://github.com/user-attachments/assets/216fb2f1-fe3b-4449-8a08-f1efbeefde25" />

### Demo-2
```json
{
    "InvoiceId": "INV-112",
    "CustomerName": "Jagga Enterprises",
    "Amount": 0,
    "Date": "2025-09-20"
}
```
It will go to the Dead Letter Queue (DLQ)
<img width="1622" height="792" alt="image" src="https://github.com/user-attachments/assets/1119b4d9-5fda-47d5-b186-4553a4a93b53" />

### The Workflow
<img width="779" height="455" alt="image" src="https://github.com/user-attachments/assets/a48ebdd8-d108-466b-bfb4-e633bcde609c" />
<img width="779" height="559" alt="image" src="https://github.com/user-attachments/assets/40d4360f-fefb-4fd0-a602-906ec2ca2566" />


