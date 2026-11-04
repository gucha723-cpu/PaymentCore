using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Entities;
using PaymentCore.Application.Abstractions;

namespace PaymentCore.Infrastructure.Persistence;
public class AppDbContext : DbContext, IAppDbContext
{
   public DbSet<User> Users {get; set;}
   public DbSet<Account> Accounts {get;set;}
   public DbSet<LedgerEntry> LedgerEntries{get;set;}

   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
}