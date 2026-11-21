using System.Text.Json;
using PrologMobile.Api.Clients;
using PrologMobile.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddHttpClient<IInventoryApiClient, InventoryApiClient>((services, client) =>
{
    // Read the API address and key from the application's configuration.
    var configuration = services.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiCredentials:BaseUrl"]
        ?? throw new InvalidOperationException("ApiCredentials:BaseUrl is missing or null.");
    var apiKey = configuration["ApiCredentials:ApiKey"]
        ?? throw new InvalidOperationException("ApiCredentials:ApiKey is missing or null.");
    
    // Use this address for requests and send the key in the X-API-Key header.
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Make the call to the API and retrieve the records and log the aggregated inventory.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<InventoryService>();
        var inventory = await service.GetCategorizedInventoryAsync();

        // Log the aggregated inventory.
        app.Logger.LogInformation("Aggregated inventory:{NewLine}{Inventory}",
            Environment.NewLine,
            JsonSerializer.Serialize(inventory, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            }));
    }
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Failed to retrieve and summarize all inventory records at {Timestamp}.", DateTimeOffset.Now);
}

app.Run();
