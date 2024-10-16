// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Application.Messaging;

public class StockChangedMessage : MessageBase
{
    public string TenantId { get; set; }

    public string StockId { get; set; }

    public string Sku { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public decimal UnitCost { get; set; }
}