// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Common;

/// <summary>
///     Specifies the public API for this module that will be exposed to other modules
/// </summary>
public interface IInventoryModuleClient
{
    /// <summary>
    ///     Retrieves the details of a stock based on the ID.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="id">The unique identifier of the tenant stock.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains a <see cref="Result{T}" /> with the
    ///     stock details when successful; otherwise, an error result.
    /// </returns>
    // INFO incase the Organization module is a seperate webservice use refit -> [Get("api/organization/tenants/{id}")]
    public Task<Result<StockModel>> StockFindOne(string tenantId, string id);
}