# Test Plan

## Actually executed, as of v3.0.0 — Snapshot

```
CustomerLedger.UnitTests:        20 passed, 0 failed, 0 skipped
CustomerLedger.DatabaseTests:     0 passed, 0 failed, 5 skipped  (MySQL unreachable in this sandbox)
CustomerLedger.IntegrationTests:  0 passed, 0 failed, 23 skipped (MySQL unreachable in this sandbox)
```

Run with:

```bash
dotnet test
```

To execute the 28 currently-skipped tests for real:

```bash
export CUSTOMERLEDGER_TEST_CONNECTION="Server=localhost;Port=3306;Database=customerledger_test;Uid=root;Pwd=<password>;"
dotnet test
```

## Test inventory

### CustomerLedger.UnitTests

| Class | Cases |
|---|---|
| `InvoiceCalculationServiceTests` | Line-total math, quantity/price/discount rejection, invoice-total aggregation, zero-item invoice |
| `CurrentUserContextTests` | Administrator can access any branch; Branch Manager/Staff limited to own branch; no branch claim → no access |
| `CsvUtilitiesTests` | Formula-injection neutralization (`=`,`+`,`-`,`@`), comma/quote escaping, round-trip CSV parsing with embedded commas/newlines |

### CustomerLedger.DatabaseTests

| Class | Cases |
|---|---|
| `ReferentialIntegrityTests` | Customer with a non-existent BranchId rejected by FK; Branch cannot be deleted while referenced |
| `UniqueConstraintTests` | Duplicate BranchCode rejected; second CustomerAccount for the same customer rejected |
| `IndexUsageTests` | Invoice list query's `EXPLAIN` shows the branch/status/date index as a candidate |

### CustomerLedger.IntegrationTests

| Class | Cases |
|---|---|
| `CustomerServiceTests` | Registration creates a linked account; duplicate CustomerCode rejected; cross-branch create/read rejected |
| `InvoiceServiceTests` | Draft totals calculated correctly; activation requires ≥1 item; invoice rejected for an inactive customer |
| `PaymentServiceTests` | Full/partial payment updates status correctly; zero/negative/overpayment rejected; payment against a cancelled invoice rejected; cross-branch payment rejected |
| `PaymentReversalTests` | Reversal restores invoice/account balances; double reversal rejected; reversal without a reason rejected |
| `InstallmentScheduleServiceTests` | Paying a schedule row marks it Paid; plan auto-completes once every row is settled |
| `ReconciliationServiceTests` | Drifted totals corrected and reported; already-correct account reports no mismatch |
| `ConcurrentPaymentTests` | Two independent connections racing to overpay one invoice — exactly one succeeds |
| `BackupServiceTests` | Missing `mysqldump` binary recorded as Failed, never Completed |
| `WebApplicationSmokeTests` | Anonymous request redirects to Login; Login page renders |

## Out of scope for this test plan

UI rendering/visual regression testing, load/performance testing beyond `EXPLAIN`-based
index verification, and penetration testing (see
[Security-Test-Cases.md](Security-Test-Cases.md) for the security-specific subset that
*is* covered).
