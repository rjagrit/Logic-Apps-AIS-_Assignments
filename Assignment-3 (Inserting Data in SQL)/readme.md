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
<img width="1367" height="663" alt="image" src="https://github.com/user-attachments/assets/df82b285-c2db-4e3a-9a9b-ba521e6bfa4f" />

#### **Parse JSON**

-   Action: Parse JSON
-   Content: `@triggerBody()`
-   Schema: (same as above)
<img width="1367" height="656" alt="image" src="https://github.com/user-attachments/assets/6ba23121-251d-4d4f-abb2-41862aed1f89" />

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
<img width="1340" height="654" alt="image" src="https://github.com/user-attachments/assets/6add2232-a45b-4722-bc76-858f91ec0e96" />
<img width="1314" height="659" alt="image" src="https://github.com/user-attachments/assets/a28da02c-a75f-4414-9c59-b6b6b0f38279" />

#### **Get Rows**

-   Action: **SQL → Get Rows (V2)**
-   Table: `dbo.Employees`
-   Filter query:

        FirstName eq '@{body('Parse_JSON')?['FirstName']}'
<img width="1315" height="658" alt="image" src="https://github.com/user-attachments/assets/bb9161c6-4592-401f-a052-2576ffe041da" />


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
<img width="1305" height="657" alt="image" src="https://github.com/user-attachments/assets/a3a36626-5fc0-4ee6-a233-5a8082336d1e" />

------------------------------------------------------------------------

### 4. **Test with Postman**

## Test Data-1
Send a request:

``` http
POST https://<your-logic-app-endpoint>
Content-Type: application/json

{
  "FirstName": "Rahul",
  "LastName": "Sharma",
  "Department": "IT",
  "Salary": 60000,
  "JoiningDate": "2025-09-27"
}
```

**Response:**

``` json
{
  "status": "Success",
  "dbFirstName": "Rahul",
  "dbLastName": "Sharma"
}
```
## Test Data-2
Send a request:

``` http
POST https://<your-logic-app-endpoint>
Content-Type: application/json

{
  "FirstName": "Priya",
  "LastName": "Verma",
  "Department": "HR",
  "Salary": 45500,
  "JoiningDate": "2024-05-15"
}
```

**Response:**

``` json
{
  "status": "Success",
  "dbFirstName": "Priya",
  "dbLastName": "Verma"
}
```
<img width="1919" height="789" alt="image" src="https://github.com/user-attachments/assets/20bbc278-ff6e-4925-ace5-b687977725e9" />

------------------------------------------------------------------------

## ✅ Workflow Summary

-   HTTP POST request → Parse JSON → Insert Row into SQL → Get Row →
    Return Response

## Workflow Diagram
<img width="1342" height="655" alt="image" src="https://github.com/user-attachments/assets/e6468945-14e6-40b6-8a4d-1683535e35ee" />

