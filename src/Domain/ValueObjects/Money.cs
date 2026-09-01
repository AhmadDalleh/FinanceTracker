namespace Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
}
