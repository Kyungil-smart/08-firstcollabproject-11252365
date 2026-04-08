using System;

public static class NumberTextFormatter
{
    private const double Thousand = 1_000d;
    private const double Million = 1_000_000d;
    private const double Billion = 1_000_000_000d;

    public static string FormatCurrency(double value)
    {
        return $"${FormatCompactNumber(value)}";
    }

    public static string FormatNumber(double value)
    {
        return FormatCompactNumber(value);
    }

    private static string FormatCompactNumber(double value)
    {
        double absValue = Math.Abs(value);

        return absValue switch
        {
            >= Billion => $"{value / Billion:0.#}B",
            >= Million => $"{value / Million:0.#}M",
            >= Thousand => $"{value / Thousand:0.#}K",
            _ => value.ToString("N0")
        };
    }
}
