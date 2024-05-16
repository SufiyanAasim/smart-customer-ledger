# Events (Scheduled Mechanisms)

## What actually ships in this project

CustomerLedger uses an **ASP.NET Core `BackgroundService`**
(`OverdueInstallmentBackgroundService`, in `CustomerLedger.Infrastructure/Services/`), not
the MySQL Event Scheduler, to transition `InstallmentSchedules` rows from `Pending` to
`Overdue` once their due date has passed. It runs once at application startup and then on
a fixed one-hour interval for the lifetime of the process, calling
`IInstallmentScheduleService.MarkOverdueInstallmentsAsync`.

This project does **not** ship `database/events/CreateEvents.sql` or `DropEvents.sql` —
those files only apply if the MySQL Event Scheduler approach were chosen instead (see
below), and creating them without an actual event would misrepresent what exists, per this
project's "do not claim a feature that does not exist" documentation rule.

## Why an application-level scheduler instead of the MySQL Event Scheduler

Both are valid per the project specification ("use one of: computed view logic, MySQL Event
Scheduler, ASP.NET Core background service, explicit scheduled command"). The application
approach was chosen because:

- It keeps the "what makes an installment overdue" logic in the same C# codebase as the
  rest of the domain logic (`InstallmentStatus` enum, `InstallmentScheduleService`), rather
  than splitting business logic between SQL and C#.
- It is testable with the same xUnit tooling as everything else — no separate MySQL Event
  Scheduler test harness is needed.
- The MySQL Event Scheduler (`event_scheduler` global variable) is **off by default** on
  many managed MySQL hosts and requires a server-level `SET GLOBAL event_scheduler = ON`
  that an application deployment cannot always guarantee — a background service ships with
  the app itself and has no such external dependency.

## What the MySQL Event Scheduler equivalent would look like

Documented here for completeness/viva preparation, not implemented:

```sql
SET GLOBAL event_scheduler = ON;

CREATE EVENT IF NOT EXISTS ev_mark_overdue_installments
ON SCHEDULE EVERY 1 HOUR
DO
  UPDATE InstallmentSchedules
  SET Status = 'Overdue', UpdatedAtUtc = UTC_TIMESTAMP(6)
  WHERE Status = 'Pending' AND DueDate < UTC_TIMESTAMP();
```

If a future release moved this logic into the database, `database/events/CreateEvents.sql`
would contain exactly the statement above, and `OverdueInstallmentBackgroundService` would
be removed to avoid two competing sources of the same transition.
