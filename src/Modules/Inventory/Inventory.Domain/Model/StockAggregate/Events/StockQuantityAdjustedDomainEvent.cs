namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockQuantityAdjustedDomainEvent(TenantId tenantId, Stock stock, int oldQuantity, int newQuantity, int quantityChange, string reason)
    : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int OldQuantity { get; } = oldQuantity;
    public int NewQuantity { get; } = newQuantity;
    public int QuantityChange { get; } = quantityChange;
    public string Reason { get; } = reason;
}