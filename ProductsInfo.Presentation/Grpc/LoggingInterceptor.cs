using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ProductsInfo.Presentation.Grpc
{
    public sealed class LoggingInterceptor(
    ILogger<LoggingInterceptor> logger
) : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            logger.LogInformation(
                "gRPC Request. Method: {Method}, Request: {@Request}",
                context.Method,
                request
            );

            try
            {
                var response = await continuation(request, context);

                logger.LogInformation(
                    "gRPC Response. Method: {Method}, Response: {@Response}",
                    context.Method,
                    response
                );

                return response;
            }
            catch (ValidationException ex)
            {
                var errors = string.Join(
                    "; ",
                    ex.Errors.Select(e =>
                        $"{e.PropertyName}: {e.ErrorMessage}")
                );

                logger.LogWarning(
                    "gRPC Validation Error: {Errors}",
                    errors
                );

                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        errors
                    )
                );
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "gRPC Internal Error. Method: {Method}",
                    context.Method
                );

                throw new RpcException(
                    new Status(
                        StatusCode.Internal,
                        "Внутренняя ошибка сервера."
                    )
                );
            }
        }
    }
}
