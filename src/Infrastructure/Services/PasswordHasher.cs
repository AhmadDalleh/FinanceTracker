using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _identityHasher = new();

    public string Hash(string password) => _identityHasher.HashPassword(default!, password);

    public bool Verify(string hash, string password) =>
        _identityHasher.VerifyHashedPassword(default!, hash, password) != PasswordVerificationResult.Failed;
}
