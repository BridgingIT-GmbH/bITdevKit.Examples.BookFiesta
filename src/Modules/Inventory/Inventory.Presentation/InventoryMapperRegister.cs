// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Mapster;

public class InventoryMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterStock(config);
    }

    private static void RegisterStock(TypeAdapterConfig config)
    {
        config.ForType<StockModel, Stock>()
            .IgnoreNullValues(true)
            .ConstructUsing(
                src => Stock.Create(
                    TenantId.Create(src.TenantId),
                    ProductSku.Create(src.Sku),
                    src.QuantityOnHand,
                    src.ReorderThreshold,
                    src.ReorderQuantity,
                    Money.Create(src.UnitCost),
                    src.StorageLocation))
            .AfterMapping(
                (src, dest) =>
                {
#pragma warning disable SA1501
                    if (dest.Id != null) { }
#pragma warning restore SA1501
                    else
                    {
                        if (src.QuantityReserved > 0)
                        {
                            dest.ReserveStock(src.QuantityReserved);
                        }
                    }
                });
    }
}