// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, StockId={StockId}, QuantityChange={QuantityChange}, OldUnitCost={OldUnitCost}, NewUnitCost={NewUnitCost}")]
[TypedEntityId<Guid>]
public class StockAdjustment : Entity<StockAdjustmentId>
{
    private StockAdjustment() { } // Private constructor required by EF Core

    private StockAdjustment(StockId stockId, int? quantityChange, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.QuantityChange = quantityChange;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    private StockAdjustment(StockId stockId, Money oldUnitCost, Money newUnitCost, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.OldUnitCost = oldUnitCost;
        this.NewUnitCost = newUnitCost;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    public StockId StockId { get; private set; }

    public int? QuantityChange { get; private set; }

    public Money OldUnitCost { get; private set; }

    public Money NewUnitCost { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public static StockAdjustment CreateQuantityAdjustment(StockId stockId, int quantityChange, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for adjustment cannot be empty.");
        }

        return new StockAdjustment(stockId, quantityChange, reason, timestamp ?? DateTimeOffset.UtcNow);
    }

    public static StockAdjustment CreateUnitCostAdjustment(StockId stockId, Money oldUnitCost, Money newUnitCost, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        _ = oldUnitCost ?? throw new ArgumentException("Old unit cost cannot be empty.");
        _ = newUnitCost ?? throw new ArgumentException("New unit cost cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for adjustment cannot be empty.");
        }

        return new StockAdjustment(stockId, oldUnitCost, newUnitCost, reason, timestamp ?? DateTimeOffset.UtcNow);
    }
}