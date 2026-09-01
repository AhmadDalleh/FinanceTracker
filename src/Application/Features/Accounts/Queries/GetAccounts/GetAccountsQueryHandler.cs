using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAccounts;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, List<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAccountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public Task<List<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Accounts
            .Where(a => a.UserId == _currentUserService.UserId);

        if (!request.IncludeArchived)
        {
            query = query.Where(a => !a.IsArchived);
        }

        return query
            .OrderBy(a => a.Name)
            .ProjectTo<AccountDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
