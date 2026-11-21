using PrologMobile.Api.Models;

namespace PrologMobile.Api.Services;

public static class InventorySummarizer
{
    /// <summary>
    /// Groups available records by category and calculates each category's figures.
    /// </summary>
    public static List<CategoryInventoryResponse> SummarizeByCategory(IEnumerable<InventoryItem> records)
    {
        // Remove zero-stock records before calculating anything.
        var availableRecords = records.Where(item => item.Quantity != 0);
        var categories = availableRecords
            .GroupBy(item => item.Category, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToArray();
        var results = new List<CategoryInventoryResponse>();

        // Process each category to calculate its inventory figures.
        foreach (var category in categories)
        {
            var items = category.OrderBy(item => item.Id).ToArray();

            // If two items have the same timestamp, choose the one with the lower ID.
            var mostRecentlyUpdated = items
                .OrderByDescending(item => item.LastUpdated)
                .ThenBy(item => item.Id)
                .First();
            var totalValue = items.Sum(item => item.Price * item.Quantity);

            results.Add(new CategoryInventoryResponse(category.Key, totalValue, items.Length, mostRecentlyUpdated, items));
        }

        return results;
    }
}
