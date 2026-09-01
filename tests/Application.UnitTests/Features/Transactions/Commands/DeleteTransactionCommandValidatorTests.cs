using Application.Features.Transactions.Commands.DeleteTransaction;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class DeleteTransactionCommandValidatorTests
{
    private readonly DeleteTransactionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_HasNoErrors()
    {
        var result = _validator.Validate(new DeleteTransactionCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteTransactionCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteTransactionCommand.Id));
    }
}
