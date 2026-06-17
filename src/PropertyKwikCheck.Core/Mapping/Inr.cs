using System.Globalization;

namespace PropertyKwikCheck.Core.Mapping;

/// <summary>Indian digit grouping for rupee amounts (e.g. 480000 → "₹ 4,80,000").</summary>
public static class Inr
{
    private static readonly CultureInfo Hi = CultureInfo.GetCultureInfo("hi-IN");

    public static string Format(long amount) => "₹ " + amount.ToString("#,##0", Hi);

    public static string Format(long? amount) => amount is null ? "—" : Format(amount.Value);
}
