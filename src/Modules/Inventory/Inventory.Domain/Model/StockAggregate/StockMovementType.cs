// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

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