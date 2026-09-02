using Application.Common.Interfaces;

namespace Application.Common.Extensions;

public static class ApplicationDbContextExtensions
{
    /// <summary>
    /// Ids of the accounts owned by the given user. Transactions have no UserId of
    /// their own - they belong to an Account, which belongs to a user - so this is
    /// the scoping subquery every transaction-related query needs.
    /// </summary>
    public static IQueryable<Guid> OwnedAccountIds(this IApplicationDbContext context, string? userId) =>
        context.Accounts.Where(a => a.UserId == userId).Select(a => a.Id);
}
