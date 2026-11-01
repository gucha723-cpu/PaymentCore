using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Entities;
using PaymentCore.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapPost("/users", async (CreateUserRequest request, AppDbContext db) =>
{
    User user = new User(request.FirstName, request.LastName, request.Email);
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { user.Id });
});

app.MapPost("/accounts",async(CreateAccountRequest request, AppDbContext db) =>
{
   Account account = new Account(request.UserId, request.Currency);
   db.Accounts.Add(account);
   await db.SaveChangesAsync();
   return Results.Ok(new {account.Id});
});

app.MapPost("/accounts/{id}/deposit", async(Guid id, DepositRequest request, AppDbContext db) =>
{
   var account = await db.Accounts.FindAsync(id);
   if (account == null)
    {
        return Results.NotFound();
    } 
    account.Deposit(request.Amount);
    db.LedgerEntries.Add(new LedgerEntry(account.Id, request.Amount, "Пополнение"));
    await db.SaveChangesAsync();
    return Results.Ok(new{account.Balance});
});

app.UseHttpsRedirection();

app.Run();
public record CreateUserRequest(string FirstName, string LastName, string Email);
public record CreateAccountRequest(Guid UserId, string Currency);
public record DepositRequest(decimal Amount);