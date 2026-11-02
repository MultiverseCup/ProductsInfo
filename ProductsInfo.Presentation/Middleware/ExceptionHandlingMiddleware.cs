using FluentValidation;

namespace ProductsInfo.Presentation.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ValidationException ex)
            {
                _logger.LogWarning(
                    "Ошибка валидации запроса: {Message}",
                    ex.Message
                );

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.ErrorMessage).ToArray()
                    );

                await context.Response.WriteAsJsonAsync(new
                {
                    Title = "Ошибка валидации",
                    Status = 400,
                    Errors = errors
                }, context.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Произошла непредвиденная ошибка."
                );

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new
                {
                    Title = "Внутренняя ошибка сервера",
                    Status = 500
                }, context.RequestAborted);
            }
        }
    }
}
