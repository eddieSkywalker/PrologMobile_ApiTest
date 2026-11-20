using System.Text.Json;
using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

/// <summary>
/// Client for interacting with the Prolog Mobile API.
/// </summary>
public class PrologMobileClient : IPrologMobileClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PrologMobileClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrologMobileClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the Prolog Mobile API.</param>
    /// <param name="configuration">The application configuration containing pagination defaults.</param>
    /// <param name="logger">The logger used to report API failures.</param>
    public PrologMobileClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PrologMobileClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of inventory items from the Prolog Mobile API.
    /// </summary>
    /// <param name="page">The page number to retrieve, or null to use the configured default.</param>
    /// <param name="pageSize">The number of items per page, or null to use the configured default.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    /// <returns>The inventory response.</returns>
    public async Task<InventoryResponse> GetInventoryAsync(
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var requestedPage = page ?? _configuration.GetValue<int?>("InventoryPagination:Page")
            ?? throw new InvalidOperationException("InventoryPagination:Page must be configured.");
        var requestedPageSize = pageSize ?? _configuration.GetValue<int?>("InventoryPagination:PageSize")
            ?? throw new InvalidOperationException("InventoryPagination:PageSize must be configured.");

        try
        {
            // Make a GET request to the inventory endpoint with the requested page and page size.
            using var response = await _httpClient.GetAsync(
                $"/api/records?page={requestedPage}&pageSize={requestedPageSize}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var inventory = await response.Content.ReadFromJsonAsync<InventoryResponse>(cancellationToken);

            return inventory ?? throw new JsonException("The inventory API returned a null response.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Inventory request for page {Page} failed.", requestedPage);
            throw;
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Could not read the inventory response for page {Page}.", requestedPage);
            throw;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Inventory response for page {Page} contained invalid or null JSON.", requestedPage);
            throw;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "Inventory request for page {Page} timed out.", requestedPage);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while retrieving inventory page {Page}.", requestedPage);
            throw;
        }
    }
}