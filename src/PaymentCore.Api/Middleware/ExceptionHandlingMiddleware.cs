using Microsoft.EntityFrameworkCore;
using PaymentCore.Domain.Exceptions;
namespace PaymentCore.Api.Middleware;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;


    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(ArgumentException ex)
        {
            
            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(new
            {
               message = ex.Message 
            });
        }    
            catch(DomainException ex)
            {
                context.Response.StatusCode = 400;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch(DbUpdateConcurrencyException)
            {
                context.Response.StatusCode = 409;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Счёт был изменён другим запросом. Повторите операцию." 
                });
            }
           catch(Exception ex)
            {
                _logger.LogError(ex, "Необработанная ошибка HTTP-запроса.");

                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(new
                {
                   message = "Внутренняя ошибка сервера." 
                });
            }
    }
}