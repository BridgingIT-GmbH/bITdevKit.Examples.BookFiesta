namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

using Application;
using Common;
using DevKit.Presentation.Web;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
///     Specifies the external API for this module that will be exposed for the outside boundary
/// </summary>
public class OrganizationCompanyEndpoint : EndpointsBase
{
    /// <summary>
    ///     Maps the endpoints for the Organization Company to the specified route builder.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder instance used to define routes.</param>
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/organization/companies")
            .WithTags("Organization");

        group.MapGet("/{id}", CompanyFindOne)
            .WithName("GetOrganizationCompany")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", CompanyFindAll)
            .WithName("GetOrganizationCompanies")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/tenants", CompanyFindAllTenants)
            .WithName("GetOrganizationCompanyTenants")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CompanyCreate)
            .WithName("CreateOrganizationCompany")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPut("/{id}", CompanyUpdate)
            .WithName("UpdateOrganizationCompany")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", CompanyDelete)
            .WithName("DeleteCatalogCompany")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> CompanyFindOne(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyFindOneQuery(id))).Result;

        return result.Value == null ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value)) :
            TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, NotFound, ProblemHttpResult>> CompanyFindAllTenants(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindAllQuery { CompanyId = id })).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CompanyModel>>, ProblemHttpResult>> CompanyFindAll(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new CompanyFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Company>, IEnumerable<CompanyModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<CompanyModel>, ProblemHttpResult>> CompanyCreate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/companies/{result.Value.Id}", mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<CompanyModel>, ProblemHttpResult>> CompanyUpdate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyUpdateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> CompanyDelete(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyDeleteCommand(id))).Result;

        return result.HasError<EntityNotFoundResultError>() ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok() :
            TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}