namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

using System;
using System.Collections.Generic;

[DebuggerDisplay("Id={Id}, Sku={Sku}, QuantityOnHand={QuantityOnHand}")]
[TypedEntityId<Guid>]
public class Stock : AuditableAggregateRoot<StockId>, IConcurrent
{
    private readonly List<StockMovement> movements = [];
    private readonly List<StockAdjustment> adjustments = [];

    private Stock() { } // Private constructor required by EF Core

    private Stock(
        TenantId tenantId,
        ProductSku sku,
        int quantityOnHand,
        int reorderThreshold,
        int reorderQuantity,
        Money unitCost,
        StorageLocation location)
    {
        this.TenantId = tenantId;
        this.Sku = sku;
        this.QuantityOnHand = quantityOnHand;
        this.QuantityReserved = 0;
        this.ReorderThreshold = reorderThreshold;
        this.ReorderQuantity = reorderQuantity;
        this.UnitCost = unitCost;
        this.Location = location;
        this.LastRestockedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; }

    public ProductSku Sku { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public int ReorderThreshold { get; private set; }

    public int ReorderQuantity { get; private set; }

    public Money UnitCost { get; private set; }

    public StorageLocation Location { get; private set; }

    public DateTimeOffset? LastRestockedAt { get; private set; }

    public IEnumerable<StockMovement> Movements => this.movements;

    public IEnumerable<StockAdjustment> Adjustments => this.adjustments;

    public Guid Version { get; set; }

    public static Stock Create(
        TenantId tenantId,
        ProductSku sku,
        int quantityOnHand,
        int reorderThreshold,
        int reorderQuantity,
        Money unitCost,
        StorageLocation storageLocation)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");
        _ = sku ?? throw new DomainRuleException("ProductSku cannot be empty.");
        _ = unitCost ?? throw new DomainRuleException("UnitCost cannot be empty.");
        _ = storageLocation ?? throw new DomainRuleException("StorageLocation cannot be empty.");

        var stock = new Stock(tenantId, sku, quantityOnHand, reorderThreshold, reorderQuantity, unitCost, storageLocation);

        stock.DomainEvents.Register(new StockCreatedDomainEvent(tenantId, stock));

        return stock;
    }

    public Stock AdjustQuantity(int quantityChange, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for stock adjustment cannot be empty.");
        }

        if (this.QuantityOnHand + quantityChange < 0)
        {
            throw new DomainRuleException("Stock adjustment would result in negative quantity.");
        }

        var oldQuantity = this.QuantityOnHand;
        this.QuantityOnHand += quantityChange;

        var adjustment = StockAdjustment.CreateQuantityAdjustment(this.Id, quantityChange, reason);
        this.adjustments.Add(adjustment);

        this.DomainEvents.Register(
            new StockQuantityAdjustedDomainEvent(this.TenantId, this, oldQuantity, this.QuantityOnHand, quantityChange, reason));

        return this;
    }

    public Stock AdjustUnitCost(Money newUnitCost, string reason)
    {
        if (newUnitCost == null || newUnitCost.Amount <= 0)
        {
            throw new DomainRuleException("New unit cost must be a positive value.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for unit cost adjustment cannot be empty.");
        }

        if (this.UnitCost == newUnitCost)
        {
            return this;
        }

        var oldUnitCost = this.UnitCost;
        this.UnitCost = newUnitCost;

        var adjustment = StockAdjustment.CreateUnitCostAdjustment(this.Id, oldUnitCost, newUnitCost, reason);
        this.adjustments.Add(adjustment);

        this.DomainEvents.Register(
            new StockUnitCostAdjustedDomainEvent(this.TenantId, this, oldUnitCost, newUnitCost, reason));

        return this;
    }

    public Stock AddStock(int quantity, string reason = null)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to add must be positive.");
        }

        this.QuantityOnHand += quantity;
        this.LastRestockedAt = DateTime.UtcNow;

        this.movements.Add(
            StockMovement.Create(this.Id, quantity, StockMovementType.Addition, reason ?? "Stock addition"));

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this.TenantId, this));

        return this;
    }

    public Stock RemoveStock(int quantity, string reason = null)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to remove must be positive.");
        }

        if (quantity > this.QuantityOnHand - this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough available stock to remove.");
        }

        this.QuantityOnHand -= quantity;

        this.movements.Add(
            StockMovement.Create(this.Id, -quantity, StockMovementType.Removal, reason ?? "Stock removal"));

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this.TenantId, this));

        return this;
    }

    public Stock ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to reserve must be positive.");
        }

        if (quantity > this.QuantityOnHand - this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough available stock to reserve.");
        }

        this.QuantityReserved += quantity;

        this.DomainEvents.Register(new StockReservedDomainEvent(this.TenantId, this, quantity));

        return this;
    }

    public Stock ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to release must be positive.");
        }

        if (quantity > this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough reserved stock to release.");
        }

        this.QuantityReserved -= quantity;

        this.DomainEvents.Register(new StockReservedReleasedDomainEvent(this.TenantId, this, quantity));

        return this;
    }

    public Stock UpdateReorderInfo(int threshold, int quantity)
    {
        if (threshold < 0)
        {
            throw new DomainRuleException("Reorder threshold must be non-negative.");
        }

        if (quantity <= 0)
        {
            throw new DomainRuleException("Reorder quantity must be positive.");
        }

        this.ReorderThreshold = threshold;
        this.ReorderQuantity = quantity;

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this.TenantId, this));

        return this;
    }

    public Stock MoveToLocation(StorageLocation newLocation)
    {
        _ = newLocation ?? throw new DomainRuleException("New location cannot be empty.");

        if (this.Location == newLocation)
        {
            return this;
        }

        this.Location = newLocation;

        this.DomainEvents.Register(new StockLocationChangedDomainEvent(this.TenantId, this, newLocation));

        return this;
    }
}