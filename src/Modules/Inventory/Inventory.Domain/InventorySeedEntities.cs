// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public static class InventorySeedEntities
{
    private static string GetSuffix(long ticks)
    {
        return ticks > 0 ? $"-{ticks}" : string.Empty;
    }

    private static string GetSku(long ticks, string sku)
    {
        return ticks > 0 ? new Random().NextInt64(10000000, 999999999999).ToString() : sku;
    }

#pragma warning disable SA1202
    public static (Stock[] Stocks, StockSnapshot[] StockSnapshots) Create(TenantId[] tenants, long ticks = 0)
#pragma warning restore SA1202
    {
        return (
            Stocks.Create(tenants, ticks),
            StockSnapshots.Create(tenants, Stocks.Create(tenants, ticks), ticks));
    }

    public static class Stocks
    {
        public static Stock[] Create(TenantId[] tenants, long ticks = 0)
        {
            var random = new Random(42); // Seed for reproducibility

            return
            [
                .. new[]
                {
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321125217"),
                        100,
                        20,
                        50,
                        Money.Create(30.00m),
                        StorageLocation.Create("A", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0134494166"),
                        75,
                        15,
                        40,
                        Money.Create(25.00m),
                        StorageLocation.Create("A", "1", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321127426"),
                        50,
                        10,
                        30,
                        Money.Create(35.00m),
                        StorageLocation.Create("A", "2", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321200686"),
                        120,
                        25,
                        60,
                        Money.Create(40.00m),
                        StorageLocation.Create("A", "2", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("1491950357"),
                        80,
                        20,
                        50,
                        Money.Create(28.00m),
                        StorageLocation.Create("B", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321834577"),
                        60,
                        15,
                        40,
                        Money.Create(32.00m),
                        StorageLocation.Create("B", "1", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321815736"),
                        90,
                        20,
                        50,
                        Money.Create(38.00m),
                        StorageLocation.Create("B", "2", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0201633610"),
                        70,
                        15,
                        40,
                        Money.Create(33.00m),
                        StorageLocation.Create("B", "2", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("1617294549"),
                        110,
                        25,
                        60,
                        Money.Create(29.00m),
                        StorageLocation.Create("C", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0201895513"),
                        40,
                        10,
                        30,
                        Money.Create(42.00m),
                        StorageLocation.Create("C", "1", "2"))
                }.Select(stock =>
                {
                    stock.Id = StockId.Create($"{GuidGenerator.Create($"Stock_{stock.Sku}{GetSuffix(ticks)}")}");

                    // Add some random stock movements and adjustments
                    for (var i = 0; i < 5; i++)
                    {
                        var quantity = random.Next(1, 21);
                        var type = random.Next(2) == 0 ? StockMovementType.Addition : StockMovementType.Removal;
                        stock.AddStock(quantity);
                        if (type == StockMovementType.Removal)
                        {
                            stock.RemoveStock(quantity);
                        }

                        var adjustment = random.Next(-10, 11); // Randomly adjust quantity
                        if (adjustment != 0)
                        {
                            stock.AdjustQuantity(adjustment, $"Random quantity adjustment {i + 1}");
                        }

                        if (random.Next(2) != 0) // Randomly adjust unit cost
                        {
                            continue;
                        }

                        // BUG: AdjustUnitCost causes InvalidException when inserted (seed)
                        // System.InvalidCastException
                        //     Unable to cast object of type 'BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain.StockAdjustmentId' to type 'BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain.StockId'.
                        // var costChange = (decimal)((random.NextDouble() * 10) - 5); // Random change between -5 and 5
                        // var newUnitCost = Money.Create(Math.Max(0.01m, stock.UnitCost.Amount + costChange));
                        // stock.AdjustUnitCost(newUnitCost, $"Random unitcost adjustment {i + 1}");
                        // stock.AdjustUnitCost(Money.Zero(), $"Random unitcost adjustment {i + 1}");
                    }

                    return stock;
                })
            ];
        }
    }

    public static class StockSnapshots
    {
        public static StockSnapshot[] Create(TenantId[] tenants, Stock[] stocks, long ticks = 0)
        {
            return
            [
                .. stocks.Select(stock =>
                {
                    var snapshot = StockSnapshot.Create(
                        tenants[0],
                        stock.Id,
                        stock.Sku,
                        stock.QuantityOnHand,
                        stock.QuantityReserved,
                        stock.UnitCost,
                        stock.Location,
                        DateTimeOffset.Parse("2024-01-01T00:00:00Z"));

                    snapshot.Id = StockSnapshotId.Create(
                        $"{GuidGenerator.Create($"StockSnapshot_{stock.Sku}_{snapshot.Timestamp.Ticks}{GetSuffix(ticks)}")}");

                    return snapshot;
                })
            ];
        }
    }
}