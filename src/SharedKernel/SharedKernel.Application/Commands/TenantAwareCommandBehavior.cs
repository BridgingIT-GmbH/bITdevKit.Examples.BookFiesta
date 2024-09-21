// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

using Common;
using DevKit.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Organization.Application;

/// <summary>
/// A behavior for handling tenant-aware commands in the application. This class is responsible for
/// checking if a request implements the ITenantAware interface and ensuring the tenant exists and is active
/// before allowing the request to proceed.
/// </summary>
/// <typeparam name="TRequest">The type of request implementing IRequest.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public class TenantAwareCommandBehavior<TRequest, TResponse>(
    ILoggerFactory loggerFactory,
    IOrganizationModuleClient organizationModuleClient)
    : CommandBehaviorBase<TRequest, TResponse>(loggerFactory)
    where TRequest : class, IRequest<TResponse>
{
    protected override bool CanProcess(TRequest request)
    {
        return request is ITenantAware;
    }

    protected override async Task<TResponse> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITenantAware instance)
        {
            return await next()
                .AnyContext();
        }

        if (!(await organizationModuleClient.TenantFindOne(instance.TenantId)).Value?.IsActive == false)
        {
            throw new Exception("Tenant does not exists or inactive");
        }

        return await next()
            .AnyContext();
    }
}