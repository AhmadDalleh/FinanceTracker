using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Categories;

public class CategoryDto : IMapFrom<Category>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Category, CategoryDto>();
    }
}
