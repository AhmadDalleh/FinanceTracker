using FluentValidation;

namespace Application.Features.Transactions.Commands.DeleteTransaction;

public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
