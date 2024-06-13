# API Documentation — Smart Customer Ledger

## Endpoint Specifications

### 1. Dashboard Summary API
- **Endpoint**: `/Home/Index`
- **Method**: `GET`
- **Authentication**: Required (`Authorize`)
- **Parameters**: `branchId` (optional int)
- **Response**: `DashboardSummary` model containing active customer count, billable invoices, total outstanding balance, and daily collection metrics.

### 2. Customer Risk Model API
- **Endpoint**: `/Analytics/Index`
- **Method**: `GET`
- **Authentication**: Required (`Administrator` or `BranchManager`)
- **Response**: List of `CustomerRiskScore` objects containing credit utilization, unpaid invoice ratio, and predicted default probability.
