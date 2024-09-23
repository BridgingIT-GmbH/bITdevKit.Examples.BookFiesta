// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using Common;

/// <summary>
///     Specifies the public API for this module that will be exposed to other modules
/// </summary>
public interface IOrganizationModuleClient
{
    /// <summary>
    ///     Retrieves the details of a tenant based on the tenant ID.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains a <see cref="Result{T}" /> with the
    ///     tenant details when successful; otherwise, an error result.
    /// </returns>
    // INFO incase the Organization module is a seperate webservice use refit -> [Get("api/organization/tenants/{id}")]
    public Task<Result<TenantModel>> TenantFindOne(string id);
}