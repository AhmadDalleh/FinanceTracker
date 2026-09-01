using Application.Common.Interfaces;
using Domain.Entities;
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

    public async Task AddAsync(Category category, CancellationToken cancellationToken) =>
        await _context.Categories.AddAsync(category, cancellationToken);
}
