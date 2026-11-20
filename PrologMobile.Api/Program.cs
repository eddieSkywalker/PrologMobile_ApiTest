using PrologMobile.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<IPrologMobileClient, PrologMobileClient>((services, client) =>
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

// Retrieve and log the initial inventory records at application startup.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var client = scope.ServiceProvider.GetRequiredService<IPrologMobileClient>();
        var inventory = await client.GetInventoryAsync();

        app.Logger.LogInformation("Received {Count} records.", inventory.Records.Count);
    }
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Inventory request failed at {Timestamp}.", DateTimeOffset.Now);
}

app.Run();
