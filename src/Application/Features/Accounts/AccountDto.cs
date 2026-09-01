using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Accounts;

public class AccountDto : IMapFrom<Account>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsArchived { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Account, AccountDto>()
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Balance.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Balance.Currency));
    }
}
