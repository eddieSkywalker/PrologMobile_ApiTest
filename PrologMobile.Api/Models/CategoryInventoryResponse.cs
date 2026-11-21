namespace PrologMobile.Api.Models;

/// <summary>Inventory figures and contributing records for one category.</summary>
public record CategoryInventoryResponse(
    string Category,
    decimal TotalInventoryValue,
    int AvailableRecordCount,
    InventoryItem MostRecentlyUpdated,
    IReadOnlyList<InventoryItem> Items);
