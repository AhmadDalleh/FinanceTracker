using FluentValidation;

namespace Application.Features.Accounts.Commands.ArchiveAccount;

public class ArchiveAccountCommandValidator : AbstractValidator<ArchiveAccountCommand>
{
    public ArchiveAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
