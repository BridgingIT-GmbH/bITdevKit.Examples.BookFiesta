namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

using System.Linq.Expressions;
using BridgingIT.DevKit.Domain.Specifications;

public class StockForSkuSpecification(ProductSku sku) : Specification<Stock>
{
    public override Expression<Func<Stock, bool>> ToExpression()
    {
        return e => e.Sku.Value == sku.Value;
    }
}

public static class StockSpecifications
{
    public static Specification<Stock> ForSku(ProductSku sku)
    {
        return new StockForSkuSpecification(sku);
    }

    public static Specification<Stock> ForSku2(ProductSku sku) // INFO: short version to define a specification
    {
        return new Specification<Stock>(e => e.Sku.Value == sku.Value);
    }
}