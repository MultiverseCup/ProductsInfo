using System.Text.Json.Serialization;
using FluentValidation;

using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using ProductsInfo.Application.Services;
using ProductsInfo.Application.Validators;

using ProductsInfo.Infrastructure.Repositories;

using ProductsInfo.Presentation.Grpc;
using ProductsInfo.Presentation.Middleware;
using ProductsInfo.Presentation.Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddSwaggerGen();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
});

builder.Services.AddScoped<LoggingInterceptor>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<
    IValidator<CreateProductCommand>,
    CreateProductCommandValidator>();

builder.Services.AddScoped<
    IValidator<ProductFilter>,
    ProductFilterValidator>();

builder.Services.AddSingleton<
    IProductRepository,
    InMemoryProductRepository>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGrpcService<ProductsGrpcService>();

app.Run();