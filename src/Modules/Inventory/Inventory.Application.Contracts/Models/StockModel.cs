﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Sku { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public int ReorderThreshold { get; set; }

    public int ReorderQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public string StorageLocation { get; set; }

    public DateTimeOffset? LastRestockedAt { get; set; }

    public StockMovementModel[] Movements { get; set; }

    public StockAdjustmentModel[] Adjustments { get; set; }
}

public class StockAdjustmentModel
{
    public string Id { get; set; }

    public int? QuantityChange { get; private set; }

    public decimal OldUnitCost { get; private set; }

    public decimal NewUnitCost { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }
}

public class StockMovementModel
{
    public int Quantity { get; private set; }

    public int Type { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }
}