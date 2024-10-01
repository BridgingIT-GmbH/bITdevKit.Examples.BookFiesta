// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using MediatR;

public class OrganizationModuleClient(IMediator mediator, IMapper mapper)
    : IOrganizationModuleClient
{
    public async Task<Result<TenantModel>> TenantFindOne(string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

        return result.For<Tenant, TenantModel>(mapper);
    }
}