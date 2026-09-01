using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var ownedAccountIds = _context.Accounts
            .Where(a => a.UserId == _currentUserService.UserId)
            .Select(a => a.Id);

        var query = _context.Transactions.Where(t => ownedAccountIds.Contains(t.AccountId));

        if (request.AccountId is not null)
        {
            query = query.Where(t => t.AccountId == request.AccountId);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId);
        }

        if (request.FromDate is not null)
        {
            query = query.Where(t => t.Date >= request.FromDate);
        }

        if (request.ToDate is not null)
        {
            query = query.Where(t => t.Date <= request.ToDate);
        }

        if (request.MinAmount is not null)
        {
            query = query.Where(t => t.Amount >= request.MinAmount);
        }

        if (request.MaxAmount is not null)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount);
        }

        return PaginatedList<TransactionDto>.CreateAsync(
            query.OrderByDescending(t => t.Date).ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
