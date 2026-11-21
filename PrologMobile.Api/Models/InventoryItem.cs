namespace PrologMobile.Api.Models;

public class InventoryItem
{
    /// <summary>
    /// Unique identifier for the record
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// Product name 
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Exactly one category per record
    /// </summary>
    public required string Category { get; init; }
    /// <summary>
    /// Unit price, USD
    /// </summary>
    public required decimal Price { get; init; }
    /// <summary>
    /// Units on hand; may be 0 
    /// </summary>
    public required int Quantity { get; init; }
    /// <summary>
    /// Last updated timestamp for the inventory item
    /// </summary>
    public required DateTimeOffset LastUpdated { get; init; }
}
