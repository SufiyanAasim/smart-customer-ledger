using System.Text;

namespace CustomerLedger.Application.Services;

/// <summary>
/// Shared CSV encoding/decoding used by every export and import path. Centralized so the
/// formula-injection defense (a field starting with =, +, -, or @ gets a leading apostrophe
/// neutralizing it in Excel/Sheets) and RFC 4180 quoting are applied exactly once, not
/// reimplemented per export screen.
/// </summary>
public static class CsvUtilities
{
    private static readonly char[] FormulaTriggerChars = { '=', '+', '-', '@' };

    public static string EscapeField(string? value)
    {
        value ??= string.Empty;

        if (value.Length > 0 && FormulaTriggerChars.Contains(value[0]))
        {
            value = "'" + value;
        }

        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuoting)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    public static string BuildRow(IEnumerable<string?> fields) =>
        string.Join(",", fields.Select(EscapeField));

    public static byte[] BuildCsvBytes(IEnumerable<string> lines)
    {
        var content = string.Join("\r\n", lines) + "\r\n";
        // UTF-8 BOM so Excel opens the file with correct encoding rather than guessing.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
    }

    /// <summary>
    /// Minimal RFC 4180 line splitter — handles quoted fields containing commas/newlines,
    /// which a naive `line.Split(',')` would break on. Used by CSV import.
    /// </summary>
    public static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    currentRow.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    currentRow.Add(field.ToString());
                    field.Clear();
                    if (currentRow.Count > 1 || !string.IsNullOrEmpty(currentRow[0]))
                    {
                        rows.Add(currentRow);
                    }
                    currentRow = new List<string>();
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (field.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(field.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }
}
