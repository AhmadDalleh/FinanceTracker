using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ApplicationDbContext _context;

    public BudgetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<bool> ExistsForCategoryAndMonthAsync(Guid categoryId, DateOnly month, string userId, CancellationToken cancellationToken) =>
        _context.Budgets.AnyAsync(b => b.CategoryId == categoryId && b.Month == month && b.UserId == userId, cancellationToken);

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken) =>
        await _context.Budgets.AddAsync(budget, cancellationToken);

    public void Update(Budget budget) =>
        _context.Budgets.Update(budget);

    public void Remove(Budget budget) =>
        _context.Budgets.Remove(budget);
}
