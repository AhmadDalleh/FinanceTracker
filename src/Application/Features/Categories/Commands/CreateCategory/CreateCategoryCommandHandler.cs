using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
