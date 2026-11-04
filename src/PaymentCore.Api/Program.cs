using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Entities;
using PaymentCore.Infrastructure.Persistence;
using PaymentCore.Application.Abstractions;
using PaymentCore.Application.Users;
using PaymentCore.Application.Accounts;
using PaymentCore.Application.Transfers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();


