using System.Reflection;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    IQueryable<Account> IApplicationDbContext.Accounts => Accounts;
    IQueryable<Category> IApplicationDbContext.Categories => Categories;
    IQueryable<Transaction> IApplicationDbContext.Transactions => Transactions;
    IQueryable<Budget> IApplicationDbContext.Budgets => Budgets;
    IQueryable<User> IApplicationDbContext.Users => Users;
    IQueryable<PasswordResetToken> IApplicationDbContext.PasswordResetTokens => PasswordResetTokens;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
