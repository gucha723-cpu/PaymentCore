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

app.UseHttpsRedirection();

app.Run();
public record CreateUserRequest(string FirstName, string LastName, string Email);