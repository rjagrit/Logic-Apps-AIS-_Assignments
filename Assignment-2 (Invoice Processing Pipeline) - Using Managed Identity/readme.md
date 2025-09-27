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
2. Create queue named `jagrit-queue-03`.

**Storage Account**
1. Create account.
2. Create container `invoices`.

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

**CLI Example:**
```bash
az role assignment create --assignee <LOGICAPP_OBJECT_ID> --role "Azure Service Bus Data Receiver" --scope "/subscriptions/<SUB>/resourceGroups/<RG>/providers/Microsoft.ServiceBus/namespaces/<NAMESPACE>"

az role assignment create --assignee <LOGICAPP_OBJECT_ID> --role "Storage Blob Data Contributor" --scope "/subscriptions/<SUB>/resourceGroups/<RG>/providers/Microsoft.Storage/storageAccounts/<STORAGEACCOUNT>/blobServices/default/containers/invoices"
```

---

## 4) Build Logic App Workflow

**Trigger**: Service Bus - When a message is received in a queue (peek-lock)
* Authentication: Managed Identity
* Queue: `jagrit-queue-03`
* Rename to `SB_Trigger`

**Action 1: Compose**
* Name: `DecodeCompose`
* Inputs: `base64ToString(triggerBody()?['ContentData'])`

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

**Action 3: Condition**
* Name: `Condition_ValidateAmount`
* Expression: `@lessOrEquals(body('Parse_JSON')?['Amount'], 0)`

**True Branch**
* Dead-letter message using MI
* Lock Token: `@triggerBody()?['LockToken']`
* Reason: `"Invalid amount"`

**False Branch**
* Create blob:
  - Blob name: `concat('invoice-', body('Parse_JSON')?['InvoiceId'], '.json')`
  - Content: `outputs('DecodeCompose')`
* Create summary blob:
  - Blob name: `concat('summary-', body('Parse_JSON')?['InvoiceId'], '.txt')`
  - Content:
```text
concat('Invoice Summary\n-----------------\nInvoice ID: ', body('Parse_JSON')?['InvoiceId'],
       '\nCustomer: ', body('Parse_JSON')?['CustomerName'],
       '\nAmount: ', string(body('Parse_JSON')?['Amount']),
       '\nDate: ', body('Parse_JSON')?['Date'])
```
* Complete Service Bus message using MI

---

## 5) Test the Workflow

**Sample invoice JSON**:
```json
{"InvoiceId":"INV001","CustomerName":"Ajay","Amount":1000,"Date":"2025-09-27"}
```

**Base64 for `ContentData`**:
```
eyJJbnZvaWNlSWQiOiJJTlYwMDEiLCJDdXN0b21lck5hbWUiOiJBamF5IiwiQW1vdW50IjoxMDAwLCJEYXRlIjoiMjAyNS0wOS0yNyJ9
```

**Full message**:
```json
{
  "ContentData": "eyJJbnZvaWNlSWQiOiJJTlYwMDEiLCJDdXN0b21lck5hbWUiOiJBamF5IiwiQW1vdW50IjoxMDAwLCJEYXRlIjoiMjAyNS0wOS0yNyJ9"
}
```

Send via Service Bus Explorer or any client.

---

## 6) Inspect Run History & Debug

* Check **Overview → Runs history**.
* Verify **Inputs/Outputs** of actions.
* Fix LockToken path or permissions if errors occur.

---

## Troubleshooting Checklist

* Ensure all connectors use Managed Identity.
* Verify RBAC roles and scopes.
* Update expression names if actions were renamed.
* Check LockToken path: `triggerBody()?['LockToken']` or `triggerOutputs()?['body']['LockToken']`.

---

## Optional Extras

* ARM/Bicep template for automated deployment.
* Visual flowchart for presentation slides.

