using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Budgets;

public class BudgetDto : IMapFrom<Budget>
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateOnly Month { get; set; }
    public decimal BudgetedAmount { get; set; }
    public decimal ActualSpent { get; set; }
    public decimal PercentageUsed { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.BudgetedAmount, opt => opt.MapFrom(s => s.Amount))
            .ForMember(d => d.CategoryName, opt => opt.Ignore())
            .ForMember(d => d.ActualSpent, opt => opt.Ignore())
            .ForMember(d => d.PercentageUsed, opt => opt.Ignore());
    }
}
