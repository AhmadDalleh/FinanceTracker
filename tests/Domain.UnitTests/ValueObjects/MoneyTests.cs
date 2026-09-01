using Domain.ValueObjects;
using Xunit;

namespace Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmountAndGivenCurrency()
    {
        var money = Money.Zero("USD");

        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Equals_WhenAmountAndCurrencyMatch_ReturnsTrue()
    {
        var a = new Money(10.50m, "USD");
        var b = new Money(10.50m, "USD");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_WhenCurrencyDiffers_ReturnsFalse()
    {
        var a = new Money(10.50m, "USD");
        var b = new Money(10.50m, "EUR");

        Assert.NotEqual(a, b);
    }
}
