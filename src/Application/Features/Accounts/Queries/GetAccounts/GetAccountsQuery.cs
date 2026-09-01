using MediatR;

namespace Application.Features.Accounts.Queries.GetAccounts;

public record GetAccountsQuery(bool IncludeArchived = false) : IRequest<List<AccountDto>>;
