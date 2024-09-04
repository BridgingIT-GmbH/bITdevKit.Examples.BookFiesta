namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class OrganizationCompanyEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/organization/companies")
            .WithTags("Organization");

        group.MapGet("/{id}", GetCompany).WithName("GetOrganizationCompany")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/tenants", GetCompanyTenants).WithName("GetOrganizationCompanyTenants")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetCompanies).WithName("GetOrganizationCompanies")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, CreateCompany).WithName("CreateOrganizationCompany")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> GetCompany(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyFindOneQuery(id))).Result;

        return result.Value == null ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, NotFound, ProblemHttpResult>> GetCompanyTenants(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyFindAllTenantsQuery(id))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CompanyModel>>, ProblemHttpResult>> GetCompanies(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new CompanyFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Company>, IEnumerable<CompanyModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<CompanyModel>, ProblemHttpResult>> CreateCompany(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/companies/{result.Value.Id}",
                                   mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "),
                                   statusCode: 400);
    }
}