using Application.Common.Exceptions;
using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAccountById;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAccountByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == request.Id && a.UserId == _currentUserService.UserId)
            .ProjectTo<AccountDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return account ?? throw new NotFoundException(nameof(Account), request.Id);
    }
}
