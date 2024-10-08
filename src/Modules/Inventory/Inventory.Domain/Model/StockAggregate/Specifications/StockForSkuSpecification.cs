namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

using System.Linq.Expressions;
using BridgingIT.DevKit.Domain.Specifications;

public class StockForSkuSpecification(TenantId tenantId, ProductSku sku) : Specification<Stock>
{
    public override Expression<Func<Stock, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Sku.Value == sku.Value;
    }
}

public static class StockSpecifications
{
    public static Specification<Stock> ForSku(TenantId tenantId, ProductSku sku)
    {
        return new StockForSkuSpecification(tenantId, sku);
    }

    public static Specification<Stock> ForSku2(TenantId tenantId, ProductSku sku) // INFO: short version to define a specification
    {
        return new Specification<Stock>(e => e.TenantId == tenantId && e.Sku.Value == sku.Value);
    }
}