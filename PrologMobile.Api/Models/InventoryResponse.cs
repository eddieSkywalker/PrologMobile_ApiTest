namespace PrologMobile.Api.Models;

public class InventoryResponse
{
    public required List<InventoryItem> Records { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalRecords { get; init; }
}
