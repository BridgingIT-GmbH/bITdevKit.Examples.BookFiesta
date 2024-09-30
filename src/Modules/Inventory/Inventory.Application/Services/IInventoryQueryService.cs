// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public interface IInventoryQueryService
{
    /// <summary>
    ///     Retrieves the top stocks based on total movement quantity within a specified time period.
    /// </summary>
    /// <param name="start">The start of the time period.</param>
    /// <param name="end">The end of the time period.</param>
    /// <param name="limit">The maximum number of stocks to retrieve (default is 5).</param>
    Task<Result<IEnumerable<Stock>>> StockFindTopAsync(DateTimeOffset start, DateTimeOffset end, int limit = 5);
}