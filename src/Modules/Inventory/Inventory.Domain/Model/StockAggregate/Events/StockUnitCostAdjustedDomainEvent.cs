namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockUnitCostAdjustedDomainEvent(Stock stock, Money oldUnitCost, Money newUnitCost, string reason)
    : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public Money OldUnitCost { get; } = oldUnitCost;
    public Money NewUnitCost { get; } = newUnitCost;
    public string Reason { get; } = reason;
}