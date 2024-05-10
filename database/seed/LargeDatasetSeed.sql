-- =====================================================================
-- CustomerLedger — LargeDatasetSeed.sql
-- Generates a bounded volume of synthetic customers + invoices in a
-- dedicated branch, for EXPLAIN/index-usage demonstrations that need
-- more than a handful of rows to show a meaningful difference between
-- an index scan and a full table scan.
--
-- Row count is explicit and reported at the end — nothing here is
-- silently capped or truncated without saying so. Default: 2,000
-- customers, each with exactly one invoice (2,000 invoices). Adjust
-- p_row_count below to generate more/fewer.
-- =====================================================================

USE customerledger;

INSERT INTO Branches (BranchCode, Name, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
SELECT 'PERF-TEST', 'Performance Test Branch', '000-0000000', 'Synthetic Data', 'Karachi', 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM Branches WHERE BranchCode = 'PERF-TEST');

SET @perf_branch_id = (SELECT BranchId FROM Branches WHERE BranchCode = 'PERF-TEST');
SET @seed_user_id = (SELECT Id FROM AspNetUsers ORDER BY CreatedAtUtc LIMIT 1);

DROP PROCEDURE IF EXISTS sp_generate_large_dataset;

DELIMITER $$
CREATE PROCEDURE sp_generate_large_dataset(IN p_row_count INT)
BEGIN
    DECLARE i INT DEFAULT 1;
    DECLARE v_customer_id INT;
    DECLARE v_code VARCHAR(30);

    WHILE i <= p_row_count DO
        SET v_code = CONCAT('PERF-CUST-', LPAD(i, 6, '0'));

        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerCode = v_code) THEN
            INSERT INTO Customers (BranchId, CustomerCode, FullName, PhoneNumber, Address, City, RegistrationDate, Status, IsDeleted, CreatedAtUtc)
            VALUES (@perf_branch_id, v_code, CONCAT('Synthetic Customer ', i), CONCAT('0300', LPAD(i, 7, '0')), 'Synthetic Address', 'Karachi', UTC_TIMESTAMP(6), 'Active', 0, UTC_TIMESTAMP(6));

            SET v_customer_id = LAST_INSERT_ID();

            INSERT INTO CustomerAccounts (CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, CreatedAtUtc, ConcurrencyVersion)
            VALUES (v_customer_id, 0, 0, 0, 0, 'Active', UTC_TIMESTAMP(6), 0);

            IF @seed_user_id IS NOT NULL THEN
                INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion)
                VALUES (
                    v_customer_id, @perf_branch_id, CONCAT('PERF-INV-', LPAD(i, 6, '0')),
                    DATE_SUB(UTC_TIMESTAMP(6), INTERVAL (i % 90) DAY), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL (30 - (i % 90)) DAY),
                    1000 * (1 + (i % 20)), 0, 0, 1000 * (1 + (i % 20)), 0, 1000 * (1 + (i % 20)),
                    'Unpaid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
                );
            END IF;
        END IF;

        SET i = i + 1;
    END WHILE;
END$$
DELIMITER ;

-- Adjust the row count here — 2,000 keeps this script's runtime reasonable
-- for a classroom/grading demonstration while still being large enough
-- for EXPLAIN to show a clear index-vs-scan difference.
CALL sp_generate_large_dataset(2000);

DROP PROCEDURE IF EXISTS sp_generate_large_dataset;

-- Report exactly what was generated — no silent truncation.
SELECT
    (SELECT COUNT(*) FROM Customers WHERE CustomerCode LIKE 'PERF-CUST-%') AS synthetic_customers_generated,
    (SELECT COUNT(*) FROM Invoices WHERE InvoiceNumber LIKE 'PERF-INV-%') AS synthetic_invoices_generated;
