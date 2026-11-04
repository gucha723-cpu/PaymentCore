using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Entities;

namespace PaymentCore.Application.Abstractions;
public interface IAppDbContext
{
    public DbSet<User> Users {get; set;}
   public DbSet<Account> Accounts {get;set;}
   public DbSet<LedgerEntry> LedgerEntries{get;set;}

   public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}