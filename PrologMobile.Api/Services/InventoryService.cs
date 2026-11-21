using PrologMobile.Api.Clients;
using PrologMobile.Api.Models;

namespace PrologMobile.Api.Services;

/// <summary>
/// Service for retrieving and aggregating inventory from the PrologMobile API.
/// </summary>
public class InventoryService
{
    private readonly IInventoryApiClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InventoryService> _logger;

    /// <summary>
    /// Creates a service that retrieves and aggregates inventory.
    /// </summary>
    public InventoryService(
        IInventoryApiClient client,
        IConfiguration configuration,
        ILogger<InventoryService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves every inventory page and aggregates the complete dataset.
    /// </summary>
    public async Task<List<CategoryInventoryResponse>> GetCategorizedInventoryAsync(CancellationToken cancellationToken = default)
    {
        // Read the page size and check that the API accepts it.
        var pageSize = _configuration.GetValue<int?>("InventoryPagination:PageSize")
            ?? throw new InvalidOperationException("InventoryPagination:PageSize must be configured.");

        // The first page tells us how many pages and records the API expects us to receive.
        var firstPage = await _client.GetInventoryAsync(1, pageSize, cancellationToken);
        ValidatePage(firstPage, 1, pageSize);

        // Check whether the reported page count agrees with the record count and page size.
        var expectedPages = firstPage.TotalRecords == 0
            ? 0
            : 1 + (firstPage.TotalRecords - 1) / pageSize;

        // Ensure that the reported total pages matches the expected number of pages based on the total records and page size.
        if (firstPage.TotalPages != expectedPages && !(firstPage.TotalRecords == 0 && firstPage.TotalPages == 1))
        {
            throw new InvalidDataException("Inventory API pagination metadata is inconsistent.");
        }

        // Keep all unique records so overlapping pages cannot count an item twice.
        var records = new List<InventoryItem>();
        var recordsById = new Dictionary<int, InventoryItem>();
        AddPage(firstPage, records, recordsById);

        // Retrieve the rest of the pages and add their records to the list.
        await GetRemainingPagesAsync(firstPage, pageSize, records, recordsById, cancellationToken);

        // Do not calculate results from a partial dataset.
        if (records.Count != firstPage.TotalRecords)
        {
            throw new InvalidDataException(
                $"Inventory API returned {records.Count} unique records; expected {firstPage.TotalRecords}.");
        }

        return InventorySummarizer.SummarizeByCategory(records);
    }

    /// <summary>
    /// Retrieves the pages after the first page and adds their records.
    /// </summary>
    private async Task GetRemainingPagesAsync(
        InventoryResponse firstPage,
        int pageSize,
        List<InventoryItem> records,
        Dictionary<int, InventoryItem> recordsById,
        CancellationToken cancellationToken)
    {
        // Request each page and check that its totals match the first page.
        for (var page = 2; page <= firstPage.TotalPages; page++)
        {
            var response = await _client.GetInventoryAsync(page, pageSize, cancellationToken);
            ValidatePage(response, page, pageSize);
            if (response.TotalPages != firstPage.TotalPages ||
                response.TotalRecords != firstPage.TotalRecords)
            {
                throw new InvalidDataException("Inventory API pagination metadata changed between pages.");
            }

            AddPage(response, records, recordsById);
        }
    }

    /// <summary>
    /// Checks that a page matches the request and contains pagination details.
    /// </summary>
    private static void ValidatePage(InventoryResponse response, int page, int pageSize)
    {
        // Make sure this is the page we asked for and that its basic information is present.
        if (response.Page != page || response.PageSize != pageSize ||
            response.TotalPages < 0 || response.TotalRecords < 0 || response.Records is null)
        {
            throw new InvalidDataException("Inventory API returned invalid pagination metadata or records.");
        }
    }

    /// <summary>
    /// Adds unique records and checks that repeated IDs have matching details.
    /// </summary>
    private void AddPage(
        InventoryResponse response,
        List<InventoryItem> records,
        Dictionary<int, InventoryItem> recordsById)
    {
        // Check each record before adding it to the full inventory list.
        foreach (var item in response.Records)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Category))
            {
                throw new InvalidDataException("Inventory API returned an invalid record.");
            }

            if (recordsById.TryGetValue(item.Id, out var existing))
            {
                // A repeated ID is safe to skip only when both copies describe the same item.
                if (existing.Name != item.Name || existing.Category != item.Category ||
                    existing.Price != item.Price || existing.Quantity != item.Quantity ||
                    existing.LastUpdated != item.LastUpdated)
                {
                    throw new InvalidDataException($"Inventory API returned conflicting details for record ID {item.Id}.");
                }

                _logger.LogWarning("Inventory API repeated record ID {RecordId} on page {Page}.", item.Id, response.Page);

                continue;
            }

            // Add the new record to the dictionary and the list of all records.
            recordsById.Add(item.Id, item);
            records.Add(item);
        }
    }
}
