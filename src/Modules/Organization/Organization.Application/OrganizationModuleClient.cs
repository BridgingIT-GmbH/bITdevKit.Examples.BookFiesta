// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using EnsureThat;
using MediatR;

public class OrganizationModuleClient(
    IMediator mediator,
    IMapper mapper)
    : IOrganizationModuleClient
{
    public async Task<Result<TenantModel>> TenantFindOne(string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

        return result.For<Tenant, TenantModel>(mapper);
    }
}

// TODO: moved to bitdevbkit (common.mapping)
public static class ResultExtensions
{
    public static Result<TResult> For<TValue, TResult>(this Result<TValue> source, IMapper mapper)
        where TResult : class
    {
        EnsureArg.IsNotNull(mapper, nameof(mapper));

        if (source?.IsFailure == true)
        {
            return Result<TResult>.Failure()
                .WithMessages(source?.Messages)
                .WithErrors(source?.Errors);
        }

        return Result<TResult>
            .Success(source != null ? mapper.Map<TValue, TResult>(source.Value) : null)
            .WithMessages(source?.Messages)
            .WithErrors(source?.Errors);
    }
}