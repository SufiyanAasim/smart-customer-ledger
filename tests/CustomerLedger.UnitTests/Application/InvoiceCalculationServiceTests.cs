using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Services;
using CustomerLedger.Domain.Entities;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class InvoiceCalculationServiceTests
{
    [Fact]
    public void CalculateLineTotal_ComputesGrossMinusDiscountPlusTax()
    {
        var item = new InvoiceItem { Quantity = 3, UnitPrice = 100m, DiscountAmount = 20m, TaxAmount = 10m };

        var lineTotal = InvoiceCalculationService.CalculateLineTotal(item);

        Assert.Equal(290m, lineTotal); // (3 * 100) - 20 + 10
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateLineTotal_RejectsNonPositiveQuantity(decimal quantity)
    {
        var item = new InvoiceItem { Quantity = quantity, UnitPrice = 100m };

        Assert.Throws<BusinessRuleException>(() => InvoiceCalculationService.CalculateLineTotal(item));
    }

    [Fact]
    public void CalculateLineTotal_RejectsNegativeUnitPrice()
    {
        var item = new InvoiceItem { Quantity = 1, UnitPrice = -5m };

        Assert.Throws<BusinessRuleException>(() => InvoiceCalculationService.CalculateLineTotal(item));
    }

    [Fact]
    public void CalculateLineTotal_RejectsDiscountLargerThanGrossPlusTax()
    {
        var item = new InvoiceItem { Quantity = 1, UnitPrice = 10m, DiscountAmount = 50m, TaxAmount = 0m };

        Assert.Throws<BusinessRuleException>(() => InvoiceCalculationService.CalculateLineTotal(item));
    }

    [Fact]
    public void RecalculateInvoiceTotals_SumsAllItemsAndAppliesOutstanding()
    {
        var invoice = new Invoice
        {
            PaidAmount = 50m,
            InvoiceItems = new List<InvoiceItem>
            {
                new() { Quantity = 2, UnitPrice = 100m, DiscountAmount = 0m, TaxAmount = 0m },
                new() { Quantity = 1, UnitPrice = 50m, DiscountAmount = 5m, TaxAmount = 5m }
            }
        };

        InvoiceCalculationService.RecalculateInvoiceTotals(invoice);

        Assert.Equal(250m, invoice.Subtotal);       // (2*100) + (1*50)
        Assert.Equal(5m, invoice.DiscountAmount);
        Assert.Equal(5m, invoice.TaxAmount);
        Assert.Equal(250m, invoice.TotalAmount);     // 250 - 5 + 5
        Assert.Equal(200m, invoice.OutstandingAmount); // 250 - 50 paid
        Assert.Equal(200m, invoice.InvoiceItems.First().LineTotal); // 2*100
    }

    [Fact]
    public void RecalculateInvoiceTotals_WithNoItems_ResultsInZeroTotal()
    {
        var invoice = new Invoice { InvoiceItems = new List<InvoiceItem>() };

        InvoiceCalculationService.RecalculateInvoiceTotals(invoice);

        Assert.Equal(0m, invoice.TotalAmount);
        Assert.Equal(0m, invoice.OutstandingAmount);
    }
}
