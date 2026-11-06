using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Entities;
using PaymentCore.Application.Abstractions;

namespace PaymentCore.Infrastructure.Persistence;
public class AppDbContext : DbContext, IAppDbContext
{
   public DbSet<User> Users {get; set;}
   public DbSet<Account> Accounts {get;set;}
   public DbSet<LedgerEntry> LedgerEntries{get;set;}
   public DbSet<Card> Cards{get;set;}
   public DbSet<DebitCard> DebitCards{get;set;}
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DebitCard>();
    }
}