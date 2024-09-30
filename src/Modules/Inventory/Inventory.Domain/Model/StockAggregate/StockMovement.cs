// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, StockId={StockId}, Quantity={Quantity}")]
[TypedEntityId<Guid>]
public class StockMovement : Entity<StockMovementId>
{
    private StockMovement() { } // Private constructor required by EF Core

    private StockMovement(StockId stockId, int quantity, StockMovementType type, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.Quantity = quantity;
        this.Type = type;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    public StockId StockId { get; private set; }

    public int Quantity { get; private set; }

    public StockMovementType Type { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public static StockMovement Create(StockId stockId, int quantity, StockMovementType type, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new DomainRuleException("StockId cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for movement cannot be empty.");
        }

        return new StockMovement(stockId, quantity, type, reason, timestamp ?? DateTimeOffset.UtcNow);
    }
}

[DebuggerDisplay("Id={Id}, Value={Value}")]
public class StockMovementType(int id, string value, string code)
    : Enumeration(id, value)
{
    public static StockMovementType Addition = new(0, nameof(Addition), "ADD");

    public static StockMovementType Removal = new(1, nameof(Removal), "REM");

    public string Code { get; } = code;

    public static IEnumerable<StockMovementType> GetAll()
    {
        return GetAll<StockMovementType>();
    }
}