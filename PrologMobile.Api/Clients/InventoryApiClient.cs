using System.Net;
using System.Text.Json;
using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

/// <summary>
/// Client for interacting with the Prolog Mobile API.
/// </summary>
public class InventoryApiClient : IInventoryApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions PrettyJsonOptions = new() { WriteIndented = true };
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the Prolog Mobile API.</param>
    /// <param name="logger">The logger used to report API failures.</param>
    public InventoryApiClient(
        HttpClient httpClient,
        ILogger<InventoryApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of inventory items from the Prolog Mobile API.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    /// <returns>The inventory response.</returns>
    public async Task<InventoryResponse> GetInventoryAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Make a GET request to the inventory endpoint with the requested page and page size.
            using var response = await SendRequestAsync(page, pageSize, cancellationToken);

            response.EnsureSuccessStatusCode();

            // Format the response for the log, then convert the JSON into an inventory object.
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            
            using var document = JsonDocument.Parse(payload);
            var formattedPayload = JsonSerializer.Serialize(document.RootElement, PrettyJsonOptions);
            _logger.LogInformation("Inventory API response for page {Page}:{NewLine}{Payload}",
                page, Environment.NewLine, formattedPayload);

            var inventory = document.RootElement.Deserialize<InventoryResponse>(JsonOptions);

            return inventory ?? throw new JsonException("The inventory API returned a null response.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Inventory request for page {Page} failed.", page);
            throw;
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Could not read the inventory response for page {Page}.", page);
            throw;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Inventory response for page {Page} contained invalid or null JSON.", page);
            throw;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "Inventory request for page {Page} timed out.", page);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while retrieving inventory page {Page}.", page);
            throw;
        }
    }

    /// <summary>
    /// Requests one page and retries a limited number of times when the API returns 429.
    /// </summary>
    private async Task<HttpResponseMessage> SendRequestAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 4;

        for (var retries = 0; ; retries++)
        {
            var response = await _httpClient.GetAsync(
                $"/api/records?page={page}&pageSize={pageSize}",
                cancellationToken);

            // Return the response if it is not rate-limited or we have used all retries.
            if (response.StatusCode != HttpStatusCode.TooManyRequests || retries == maxRetries)
            {
                return response;
            }

            // Use the server's wait time if provided; otherwise wait 1, 2, 3, then 4 seconds.
            var delay = TimeSpan.FromSeconds(retries + 1);
            var retryAfter = response.Headers.RetryAfter;
            if (retryAfter?.Delta is TimeSpan waitTime)
            {
                delay = waitTime;
            }
            else if (retryAfter?.Date is DateTimeOffset retryTime)
            {
                delay = retryTime - DateTimeOffset.UtcNow;
            }

            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            // Release the response before waiting, then try the same page again.
            response.Dispose();
            _logger.LogWarning(
                "Inventory API rate limit reached for page {Page}. Waiting {Seconds} seconds before retry {Retry} of {MaxRetries}.",
                page, delay.TotalSeconds, retries + 1, maxRetries);

            await Task.Delay(delay, cancellationToken);
        }
    }
}