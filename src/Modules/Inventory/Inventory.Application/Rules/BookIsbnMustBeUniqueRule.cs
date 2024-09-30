// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class SkuMustBeUniqueRule(IGenericRepository<Stock> repository, Stock stock) : DomainRuleBase
{
    public override string Message => "Stock Sku should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            StockSpecifications.ForSku(stock.Sku),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class StockRules
{
    public static IDomainRule SkuMustBeUnique(IGenericRepository<Stock> repository, Stock stock)
    {
        return new SkuMustBeUniqueRule(repository, stock);
    }
}