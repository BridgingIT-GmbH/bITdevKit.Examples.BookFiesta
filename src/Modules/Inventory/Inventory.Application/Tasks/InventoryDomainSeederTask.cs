// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class InventoryDomainSeederTask(
    IGenericRepository<Stock> stockRepository,
    IGenericRepository<StockSnapshot> stockSnapshotRepository)
        : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var stocks = await this.SeedStocks(stockRepository, tenantIds);
        var stockSnapshots = await this.SeedStocksSnapshots(stockSnapshotRepository, stocks, tenantIds);
    }

    private async Task<Stock[]> SeedStocks(IGenericRepository<Stock> repository, TenantId[] tenantIds)
    {
        var entities = InventorySeedEntities.Stocks.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(InventoryDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<StockSnapshot[]> SeedStocksSnapshots(IGenericRepository<StockSnapshot> repository, Stock[] stocks, TenantId[] tenantIds)
    {
        var entities = InventorySeedEntities.StockSnapshots.Create(tenantIds, stocks);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(InventoryDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }
}