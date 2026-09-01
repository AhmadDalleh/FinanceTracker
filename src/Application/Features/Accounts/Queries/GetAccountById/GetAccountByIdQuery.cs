using MediatR;

namespace Application.Features.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto>;
