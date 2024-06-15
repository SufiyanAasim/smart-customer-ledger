# Architecture Overview — Smart Customer Ledger

## High Level System Architecture

Smart Customer Ledger is designed around a 4-tier clean architecture pattern ensuring separation of concerns, testability, and multi-branch data isolation.

```
+-------------------------------------------------------------+
|                      CustomerLedger.Web                     |
|         (ASP.NET Core MVC Controllers, Razor Views)         |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                  CustomerLedger.Application                 |
|       (Interfaces, DTOs, Calculation Logic, Services)       |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                 CustomerLedger.Infrastructure               |
|   (EF Core DbContext, Repositories, Health Monitors)        |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                     CustomerLedger.Domain                   |
|           (Domain Entities, Enums, Value Objects)           |
+-------------------------------------------------------------+
```

## Core Architectural Guarantees

1. **Multi-Branch Isolation**: Scope validation through `ICurrentUserContext`.
2. **ACID Transactions**: Row-level locking on invoice settlements.
3. **Machine Learning Risk Model**: From-scratch supervised gradient descent risk scoring.
