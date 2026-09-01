using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Transactions;

public class TransactionDto : IMapFrom<Transaction>
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly Date { get; set; }
    public string? Note { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Transaction, TransactionDto>();
    }
}
