using CustomerLedger.Application.Services;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class CsvUtilitiesTests
{
    [Theory]
    [InlineData("=SUM(A1:A2)", "'=SUM(A1:A2)")]
    [InlineData("+1234", "'+1234")]
    [InlineData("-1234", "'-1234")]
    [InlineData("@mention", "'@mention")]
    public void EscapeField_NeutralizesFormulaInjectionCharacters(string input, string expected)
    {
        Assert.Equal(expected, CsvUtilities.EscapeField(input));
    }

    [Fact]
    public void EscapeField_QuotesValuesContainingCommas()
    {
        var result = CsvUtilities.EscapeField("Smith, John");
        Assert.Equal("\"Smith, John\"", result);
    }

    [Fact]
    public void EscapeField_DoublesEmbeddedQuotes()
    {
        var result = CsvUtilities.EscapeField("He said \"hello\"");
        Assert.Equal("\"He said \"\"hello\"\"\"", result);
    }

    [Fact]
    public void EscapeField_PlainValue_IsUnchanged()
    {
        Assert.Equal("Karachi", CsvUtilities.EscapeField("Karachi"));
    }

    [Fact]
    public void BuildRow_JoinsEscapedFieldsWithCommas()
    {
        var row = CsvUtilities.BuildRow(new[] { "CUST-001", "Ali, Khan", "Karachi" });
        Assert.Equal("CUST-001,\"Ali, Khan\",Karachi", row);
    }

    [Fact]
    public void ParseCsv_HandlesQuotedFieldsWithEmbeddedCommasAndNewlines()
    {
        var csv = "Code,Name,Notes\nA1,\"Doe, Jane\",\"Line1\nLine2\"\nA2,Simple,NoNotes\n";

        var rows = CsvUtilities.ParseCsv(csv);

        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { "Code", "Name", "Notes" }, rows[0]);
        Assert.Equal("Doe, Jane", rows[1][1]);
        Assert.Equal("Line1\nLine2", rows[1][2]);
        Assert.Equal("A2", rows[2][0]);
    }

    [Fact]
    public void ParseCsv_EmptyContent_ReturnsNoRows()
    {
        var rows = CsvUtilities.ParseCsv(string.Empty);
        Assert.Empty(rows);
    }
}
