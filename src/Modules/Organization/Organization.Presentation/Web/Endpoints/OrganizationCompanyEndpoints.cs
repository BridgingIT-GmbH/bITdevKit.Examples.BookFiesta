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
        var group = app.MapGroup("api/companies")
            .WithTags("Organization");

        group.MapGet("/{id}", async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new CompanyFindOneQuery(id))).Result;

            return (result.Value == null) ? TypedResults.NotFound() : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetOrganizationCompany")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/tenants", async Task<Results<Ok<IEnumerable<TenantModel>>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new CompanyFindAllTenantsQuery(id))).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetOrganizationCompanyTenants")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, async Task<Results<Ok<IEnumerable<CompanyModel>>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId) =>
        {
            var result = (await mediator.Send(new CompanyFindAllQuery())).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetOrganizationCompanies")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, async Task<Results<Created<CompanyModel>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromBody] CompanyModel model) =>
        {
            var result = (await mediator.Send(new CompanyCreateCommand(model))).Result;

            return result.IsSuccess
                ? TypedResults.Created($"api/companies/{result.Value.Id}",
                                       mapper.Map<Company, CompanyModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "),
                                       statusCode: 400);
        }).WithName("CreateOrganizationCompany")
            .Produces<CompanyModel>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}