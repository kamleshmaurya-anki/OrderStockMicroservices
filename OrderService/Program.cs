using Microsoft.EntityFrameworkCore;
using OrderService.Clients;
using OrderService.Data;
using OrderService.Middleware;
using OrderService.Repositories;
using OrderService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (Serilog writing to file + console) ----
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ---- Services ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderManagementService>();

// Typed HttpClient for calling Product Service. This is the ONLY channel
// Order Service has to product data - it does not reference product_db.
var productServiceBaseUrl = builder.Configuration["ProductService:BaseUrl"]
    ?? throw new InvalidOperationException("ProductService:BaseUrl is not configured.");
var productServiceTimeoutSeconds = builder.Configuration.GetValue("ProductService:TimeoutSeconds", 10);

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(productServiceBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(productServiceTimeoutSeconds);
});

var app = builder.Build();

// ---- Middleware pipeline ----
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
