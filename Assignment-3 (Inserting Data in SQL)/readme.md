# Logic App: Insert Employee Record into SQL Database

This Logic App receives employee data via an **HTTP POST request**,
inserts it into an **Azure SQL Database**, retrieves the record, and
returns a success response.

------------------------------------------------------------------------

## 🚀 Steps to Reproduce

### 1. **Prepare SQL Table**

Run this in your Azure SQL Database (via SSMS):

``` sql
CREATE TABLE dbo.Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Department NVARCHAR(100),
    Salary INT,
    JoiningDate DATE
);
```

------------------------------------------------------------------------

### 2. **Create Logic App**

1.  In **Azure Portal** → Create a **Logic App (Consumption)**.
2.  Name it, choose resource group, and deploy.

------------------------------------------------------------------------

### 3. **Define Workflow**

#### **Trigger**

-   Add **When an HTTP request is received** trigger.
-   Method: `POST`
-   Request body schema:

``` json
{
  "type": "object",
  "properties": {
    "FirstName": { "type": "string" },
    "LastName": { "type": "string" },
    "Department": { "type": "string" },
    "Salary": { "type": "integer" },
    "JoiningDate": { "type": "string" }
  }
}
```

#### **Parse JSON**

-   Action: Parse JSON
-   Content: `@triggerBody()`
-   Schema: (same as above)

#### **Insert Row**

-   Action: **SQL → Insert Row (V2)**
-   Connect to Azure SQL Database.
-   Table: `dbo.Employees`
-   Map fields:
    -   FirstName → `FirstName`
    -   LastName → `LastName`
    -   Department → `Department`
    -   Salary → `Salary`
    -   JoiningDate → `JoiningDate`

#### **Get Rows**

-   Action: **SQL → Get Rows (V2)**

-   Table: `dbo.Employees`

-   Filter query:

        FirstName eq '@{body('Parse_JSON')?['FirstName']}'

#### **Response**

-   Action: **Response**
-   Status Code: `200`
-   Body:

``` json
{
  "status": "Success",
  "dbFirstName": "@{first(body('Get_rows_(V2)')?['value'])?['FirstName']}",
  "dbLastName": "@{first(body('Get_rows_(V2)')?['value'])?['LastName']}"
}
```

------------------------------------------------------------------------

### 4. **Test with Postman**

Send a request:

``` http
POST https://<your-logic-app-endpoint>
Content-Type: application/json

{
  "FirstName": "Jagrit",
  "LastName": "Rattan",
  "Department": "IT",
  "Salary": 80000,
  "JoiningDate": "2025-09-28"
}
```

**Response:**

``` json
{
  "status": "Success",
  "dbFirstName": "Jagrit",
  "dbLastName": "Rattan"
}
```

------------------------------------------------------------------------

## ✅ Workflow Summary

-   HTTP POST request → Parse JSON → Insert Row into SQL → Get Row →
    Return Response
