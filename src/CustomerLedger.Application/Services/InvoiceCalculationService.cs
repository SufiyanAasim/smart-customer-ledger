using CustomerLedger.Application.Exceptions;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Services;

/// <summary>
/// Centralizes invoice/line-item arithmetic so it is never duplicated between controllers,
/// services, and (later) triggers. LineTotal = (Quantity * UnitPrice) - Discount + Tax;
/// invoice Subtotal/TotalAmount/OutstandingAmount are derived the same way everywhere.
/// </summary>
public static class InvoiceCalculationService
{
    public static decimal CalculateLineTotal(InvoiceItem item)
    {
        if (item.Quantity <= 0)
        {
            throw new BusinessRuleException("Invoice item quantity must be greater than zero.");
        }

        if (item.UnitPrice < 0)
        {
            throw new BusinessRuleException("Invoice item unit price cannot be negative.");
        }

        var gross = item.Quantity * item.UnitPrice;
        var lineTotal = gross - item.DiscountAmount + item.TaxAmount;

        if (lineTotal < 0)
        {
            throw new BusinessRuleException("Discounts cannot create a negative line total.");
        }

        return lineTotal;
    }

    public static void RecalculateInvoiceTotals(Invoice invoice)
    {
        foreach (var item in invoice.InvoiceItems)
        {
            item.LineTotal = CalculateLineTotal(item);
        }

        invoice.Subtotal = invoice.InvoiceItems.Sum(i => i.Quantity * i.UnitPrice);
        invoice.DiscountAmount = invoice.InvoiceItems.Sum(i => i.DiscountAmount);
        invoice.TaxAmount = invoice.InvoiceItems.Sum(i => i.TaxAmount);
        invoice.TotalAmount = invoice.Subtotal - invoice.DiscountAmount + invoice.TaxAmount;
        invoice.OutstandingAmount = invoice.TotalAmount - invoice.PaidAmount;
    }
}
