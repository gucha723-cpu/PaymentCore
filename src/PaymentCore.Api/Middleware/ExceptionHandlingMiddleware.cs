using PaymentCore.Domain.Exceptions;
namespace PaymentCore.Api.Middleware;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }catch(Exception ex)
        {
            if(ex is ArgumentException)
            {
            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(new
            {
               message = ex.Message 
            });
            }
            else if(ex is InsufficientFundsException)
            {
                context.Response.StatusCode = 400;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            else if(ex is DifferentCurrenciesException)
            {
                context.Response.StatusCode = 400;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            else
            {
                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Внутренняя ошибка сервера"
                });
            }
                
            }
    }
}