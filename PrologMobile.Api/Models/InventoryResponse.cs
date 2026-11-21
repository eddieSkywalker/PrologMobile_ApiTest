namespace PrologMobile.Api.Models;

public class InventoryResponse
{
    public required List<InventoryItem> Records { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required int TotalRecords { get; init; }
}
