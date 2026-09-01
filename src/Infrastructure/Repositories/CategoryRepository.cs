using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(Guid id, string userId, CancellationToken cancellationToken) =>
        _context.Categories.AnyAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
}
