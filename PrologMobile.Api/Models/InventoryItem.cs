namespace PrologMobile.Api.Models;

public class InventoryItem
{
    /// <summary>
    /// Unique identifier for the record
    /// </summary>
    public int Id { get; init; }
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
    public decimal Price { get; init; }
    /// <summary>
    /// Units on hand; may be 0 
    /// </summary>
    public int Quantity { get; init; }
    /// <summary>
    /// Last updated timestamp for the inventory item
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; }
}
