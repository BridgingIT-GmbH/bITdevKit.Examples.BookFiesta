// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Organization.Application;

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