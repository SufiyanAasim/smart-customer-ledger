# System Architecture

```mermaid
graph TB
    Browser[Browser]

    subgraph Web["CustomerLedger.Web"]
        Controllers[Controllers]
        Views[Razor Views]
        Program[Program.cs composition root]
    end

    subgraph Application["CustomerLedger.Application"]
        Interfaces[Service Interfaces]
        DTOs[DTOs / Results]
        CalcServices["Pure calculation services\n(InvoiceCalculationService, CsvUtilities)"]
    end

    subgraph Infrastructure["CustomerLedger.Infrastructure"]
        Services["Service Implementations\n(BranchService, CustomerService, InvoiceService,\nPaymentService, ReconciliationService, ...)"]
        DbContext[ApplicationDbContext]
        Identity[ASP.NET Core Identity store]
        Backup["MySqlBackupService / MySqlRestoreService\n(shells out to mysqldump / mysql)"]
    end

    subgraph Domain["CustomerLedger.Domain"]
        Entities[Entities / Enums / Constants]
    end

    MySQL[(MySQL 8.0)]
    MysqldumpProc[["mysqldump / mysql\n(OS process)"]]

    Browser --> Controllers
    Controllers --> Views
    Controllers --> Interfaces
    Interfaces -.implemented by.-> Services
    Services --> DbContext
    Services --> Entities
    DbContext --> MySQL
    Identity --> DbContext
    Backup --> MysqldumpProc
    MysqldumpProc --> MySQL
    CalcServices --> Entities
    Services --> CalcServices
```

**Dependency direction**: Domain has no dependency on anything else. Application depends
only on Domain. Infrastructure depends on Application and Domain (it implements
Application's interfaces). Web depends on all three but contains no business logic itself —
controllers call an injected `I...Service` and translate the result/exception into an
HTTP response.

**Why this shape**: it lets `CustomerLedger.UnitTests` exercise `InvoiceCalculationService`
and `CsvUtilities` with zero infrastructure, and lets `CustomerLedger.DatabaseTests`/
`IntegrationTests` exercise the real service implementations against a real MySQL database
without ever touching ASP.NET Core's HTTP pipeline (except for the one
`WebApplicationSmokeTests` class, which deliberately does boot the full pipeline).
