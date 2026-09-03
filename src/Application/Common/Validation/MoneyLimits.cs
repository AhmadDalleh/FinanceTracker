namespace Application.Common.Validation;

/// <summary>
/// Keeps monetary amounts safely inside the database's decimal(18,2) columns and rejects
/// values before they ever reach EF Core, so an out-of-range or over-precise amount is a
/// clear validation error instead of an unhandled numeric-overflow exception.
/// </summary>
public static class MoneyLimits
{
    public const decimal MaxAmount = 999_999_999_999.99m;

    public static bool HasAtMostTwoDecimalPlaces(decimal value) =>
        value * 100m == decimal.Truncate(value * 100m);
}
