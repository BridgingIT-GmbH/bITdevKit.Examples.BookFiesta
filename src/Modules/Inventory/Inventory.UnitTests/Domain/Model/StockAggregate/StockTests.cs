// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Inventory.UnitTests.Domain;

using System;
using Bogus;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Shouldly;
using Xunit;

public class StockTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidInputs_ReturnsStockInstance()
    {
        // Arrange
        var tenantId = TenantId.Create();
        var sku = ProductSku.Create(new Random().NextInt64(10000000, 999999999999).ToString());
        var quantityOnHand = this.faker.Random.Int(1, 100);
        var reorderThreshold = this.faker.Random.Int(1, 50);
        var reorderQuantity = this.faker.Random.Int(10, 100);
        var unitCost = Money.Create(this.faker.Random.Decimal(0.01m, 1000.00m));
        var storageLocation = this.CreateLocation();

        // Act
        var sut = Stock.Create(tenantId, sku, quantityOnHand, reorderThreshold, reorderQuantity, unitCost, storageLocation);

        // Assert
        sut.ShouldNotBeNull();
        sut.TenantId.ShouldBe(tenantId);
        sut.Sku.ShouldBe(sku);
        sut.QuantityOnHand.ShouldBe(quantityOnHand);
        sut.ReorderThreshold.ShouldBe(reorderThreshold);
        sut.ReorderQuantity.ShouldBe(reorderQuantity);
        sut.UnitCost.ShouldBe(unitCost);
        sut.Location.ShouldBe(storageLocation);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockCreatedDomainEvent);
    }

    [Fact]
    public void Create_NullTenantId_ThrowsDomainRuleException()
    {
        // Arrange
        var sku = ProductSku.Create(new Random().NextInt64(10000000, 999999999999).ToString());
        var quantityOnHand = this.faker.Random.Int(1, 100);
        var reorderThreshold = this.faker.Random.Int(1, 50);
        var reorderQuantity = this.faker.Random.Int(10, 100);
        var unitCost = Money.Create(this.faker.Random.Decimal(0.01m, 1000.00m));
        var storageLocation = this.CreateLocation();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
                Stock.Create(null, sku, quantityOnHand, reorderThreshold, reorderQuantity, unitCost, storageLocation))
            .Message.ShouldBe("TenantId cannot be empty.");
    }

    [Fact]
    public void AdjustQuantity_ValidInput_AdjustsQuantityAndAddsAdjustment()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityChange = this.faker.Random.Int(-10, 10);
        var reason = this.faker.Lorem.Sentence();
        var oldQuantity = sut.QuantityOnHand;

        // Act
        sut.AdjustQuantity(quantityChange, reason);

        // Assert
        sut.QuantityOnHand.ShouldBe(oldQuantity + quantityChange);
        sut.Adjustments.ShouldContain(a => a.QuantityChange == quantityChange && a.Reason == reason);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockQuantityAdjustedDomainEvent);
    }

    [Fact]
    public void AdjustQuantity_NegativeResultingQuantity_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityChange = -sut.QuantityOnHand - 1;
        var reason = this.faker.Lorem.Sentence();

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.AdjustQuantity(quantityChange, reason))
            .Message.ShouldBe("Stock adjustment would result in negative quantity.");
    }

    [Fact]
    public void AdjustUnitCost_ValidInput_AdjustsUnitCostAndAddsAdjustment()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var newUnitCost = Money.Create(this.faker.Random.Decimal(0.01m, 1000.00m));
        var reason = this.faker.Lorem.Sentence();
        var oldUnitCost = sut.UnitCost;

        // Act
        sut.AdjustUnitCost(newUnitCost, reason);

        // Assert
        sut.UnitCost.ShouldBe(newUnitCost);
        sut.Adjustments.ShouldContain(a => a.OldUnitCost == oldUnitCost && a.NewUnitCost == newUnitCost && a.Reason == reason);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockUnitCostAdjustedDomainEvent);
    }

    [Fact]
    public void AdjustUnitCost_NegativeUnitCost_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var newUnitCost = Money.Create(-1);
        var reason = this.faker.Lorem.Sentence();

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.AdjustUnitCost(newUnitCost, reason))
            .Message.ShouldBe("New unit cost must be a positive value.");
    }

    [Fact]
    public void AddStock_ValidQuantity_IncreasesQuantityOnHand()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToAdd = this.faker.Random.Int(1, 100);
        var oldQuantity = sut.QuantityOnHand;

        // Act
        sut.AddStock(quantityToAdd);

        // Assert
        sut.QuantityOnHand.ShouldBe(oldQuantity + quantityToAdd);
        sut.Movements.ShouldContain(m => m.Quantity == quantityToAdd && m.Type == StockMovementType.Addition);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockUpdatedDomainEvent);
    }

    [Fact]
    public void AddStock_NegativeQuantity_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToAdd = -1;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.AddStock(quantityToAdd))
            .Message.ShouldBe("Quantity to add must be positive.");
    }

    [Fact]
    public void RemoveStock_ValidQuantity_DecreasesQuantityOnHand()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToRemove = this.faker.Random.Int(1, sut.QuantityOnHand - sut.QuantityReserved);
        var oldQuantity = sut.QuantityOnHand;

        // Act
        sut.RemoveStock(quantityToRemove);

        // Assert
        sut.QuantityOnHand.ShouldBe(oldQuantity - quantityToRemove);
        sut.Movements.ShouldContain(m => m.Quantity == -quantityToRemove && m.Type == StockMovementType.Removal);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockUpdatedDomainEvent);
    }

    [Fact]
    public void RemoveStock_QuantityExceedsAvailable_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToRemove = sut.QuantityOnHand - sut.QuantityReserved + 1;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.RemoveStock(quantityToRemove))
            .Message.ShouldBe("Not enough available stock to remove.");
    }

    [Fact]
    public void ReserveStock_ValidQuantity_IncreasesQuantityReserved()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToReserve = this.faker.Random.Int(1, sut.QuantityOnHand - sut.QuantityReserved);
        var oldReserved = sut.QuantityReserved;

        // Act
        sut.ReserveStock(quantityToReserve);

        // Assert
        sut.QuantityReserved.ShouldBe(oldReserved + quantityToReserve);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockReservedDomainEvent);
    }

    [Fact]
    public void ReserveStock_QuantityExceedsAvailable_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToReserve = sut.QuantityOnHand - sut.QuantityReserved + 1;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.ReserveStock(quantityToReserve))
            .Message.ShouldBe("Not enough available stock to reserve.");
    }

    [Fact]
    public void ReleaseReservedStock_ValidQuantity_DecreasesQuantityReserved()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToReserve = this.faker.Random.Int(1, sut.QuantityOnHand);
        sut.ReserveStock(quantityToReserve);
        var quantityToRelease = this.faker.Random.Int(1, quantityToReserve);
        var oldReserved = sut.QuantityReserved;

        // Act
        sut.ReleaseReservedStock(quantityToRelease);

        // Assert
        sut.QuantityReserved.ShouldBe(oldReserved - quantityToRelease);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockReservedReleasedDomainEvent);
    }

    [Fact]
    public void ReleaseReservedStock_QuantityExceedsReserved_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var quantityToReserve = this.faker.Random.Int(1, sut.QuantityOnHand);
        sut.ReserveStock(quantityToReserve);
        var quantityToRelease = quantityToReserve + 1;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.ReleaseReservedStock(quantityToRelease))
            .Message.ShouldBe("Not enough reserved stock to release.");
    }

    [Fact]
    public void UpdateReorderInfo_ValidInput_UpdatesReorderInfo()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var newThreshold = this.faker.Random.Int(1, 50);
        var newQuantity = this.faker.Random.Int(10, 100);

        // Act
        sut.UpdateReorderInfo(newThreshold, newQuantity);

        // Assert
        sut.ReorderThreshold.ShouldBe(newThreshold);
        sut.ReorderQuantity.ShouldBe(newQuantity);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockUpdatedDomainEvent);
    }

    [Fact]
    public void UpdateReorderInfo_NegativeThreshold_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var newThreshold = -1;
        var newQuantity = this.faker.Random.Int(10, 100);

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.UpdateReorderInfo(newThreshold, newQuantity))
            .Message.ShouldBe("Reorder threshold must be non-negative.");
    }

    [Fact]
    public void MoveToLocation_ValidLocation_UpdatesStorageLocation()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var newLocation = this.CreateLocation();

        // Act
        sut.MoveToLocation(newLocation);

        // Assert
        sut.Location.ShouldBe(newLocation);
        sut.DomainEvents.GetAll().ShouldContain(e => e is StockLocationChangedDomainEvent);
    }

    [Fact]
    public void MoveToLocation_NullLocation_ThrowsDomainRuleException()
    {
        // Arrange
        var sut = this.CreateValidStock();

        // Act & Assert
        Should.Throw<ArgumentException>(() => sut.MoveToLocation(null))
            .Message.ShouldBe("New location cannot be empty.");
    }

    [Fact]
    public void Stock_MultipleOperations_AccumulatesChangesCorrectly()
    {
        // Arrange
        var sut = this.CreateValidStock();
        var initialQuantity = sut.QuantityOnHand;
        var initialMovements = sut.Movements.Count();

        // Act
        sut.AddStock(20); // movement
        sut.RemoveStock(5); // movement
        sut.ReserveStock(10);
        sut.ReleaseReservedStock(5);

        // Assert
        sut.QuantityOnHand.ShouldBe(initialQuantity + 15);
        sut.QuantityReserved.ShouldBe(5);
        sut.Movements.Count().ShouldBe(initialMovements + 2); // + 2 new
        sut.DomainEvents.GetAll().Count().ShouldBeGreaterThan(3); // At least one event for each operation
    }

    [Fact]
    public void Stock_EdgeCaseOperations_HandledCorrectly()
    {
        // Arrange
        var sut = this.CreateValidStock();
        sut.RemoveStock(sut.QuantityOnHand - sut.QuantityReserved); // Remove all available stock

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.RemoveStock(1));
        Should.Throw<DomainRuleException>(() => sut.ReserveStock(1));

        sut.AddStock(1);
        sut.ReserveStock(1);
        Should.Throw<DomainRuleException>(() => sut.RemoveStock(1));

        sut.ReleaseReservedStock(1);
        sut.RemoveStock(1);

        sut.QuantityOnHand.ShouldBe(0);
        sut.QuantityReserved.ShouldBe(0);
    }

    private Stock CreateValidStock()
    {
        var tenantId = TenantId.Create();
        var sku = ProductSku.Create(new Random().NextInt64(10000000, 999999999999).ToString());
        var quantityOnHand = this.faker.Random.Int(50, 100);
        var reorderThreshold = this.faker.Random.Int(1, 20);
        var reorderQuantity = this.faker.Random.Int(21, 50);
        var unitCost = Money.Create(this.faker.Random.Decimal(0.01m, 1000.00m));
        var storageLocation = this.CreateLocation();
        var stock = Stock.Create(tenantId, sku, quantityOnHand, reorderThreshold, reorderQuantity, unitCost, storageLocation);

        stock.Id = StockId.Create($"{GuidGenerator.Create($"Stock_{sku}")}");

        var random = new Random(42); // Use a seed for reproducibility
        for (var i = 0; i < 5; i++)
        {
            var quantity = random.Next(1, 21);
            var type = random.Next(2) == 0 ? StockMovementType.Addition : StockMovementType.Removal;
            stock.AddStock(quantity);
            if (type == StockMovementType.Removal && stock.QuantityOnHand >= quantity)
            {
                stock.RemoveStock(quantity);
            }

            var adjustment = random.Next(-10, 11);
            if (adjustment != 0 && stock.QuantityOnHand + adjustment >= 0)
            {
                stock.AdjustQuantity(adjustment, $"Random quantity adjustment {i + 1}");
            }

            if (random.Next(2) == 0)
            {
                var costChange = (decimal)((random.NextDouble() * 10) - 5);
                var newUnitCost = Money.Create(Math.Max(0.01m, stock.UnitCost.Amount + costChange));
                stock.AdjustUnitCost(newUnitCost, $"Random unit cost adjustment {i + 1}");
            }
        }

        return stock;
    }

    private StorageLocation CreateLocation() => StorageLocation.Create(
        this.faker.Random.String2(1),
        this.faker.Random.Number(1, 9).ToString(),
        this.faker.Random.Number(1, 9).ToString());
}