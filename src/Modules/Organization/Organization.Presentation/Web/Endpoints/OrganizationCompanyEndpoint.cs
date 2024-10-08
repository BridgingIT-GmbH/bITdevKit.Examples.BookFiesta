namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Presentation.Web;
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
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet("/", CompanyFindAll)
            .WithName("GetOrganizationCompanies")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet("/{id}/tenants", CompanyFindAllTenants)
            .WithName("GetOrganizationCompanyTenants")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPost("/", CompanyCreate)
            .WithName("CreateOrganizationCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPut("/{id}", CompanyUpdate)
            .WithName("UpdateOrganizationCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", CompanyDelete)
            .WithName("DeleteCatalogCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> CompanyFindOne(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyFindOneQuery(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>() ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value)) :
            TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, NotFound, ProblemHttpResult>> CompanyFindAllTenants(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindAllQuery { CompanyId = id })).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CompanyModel>>, ProblemHttpResult>> CompanyFindAll(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new CompanyFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Company>, IEnumerable<CompanyModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<CompanyModel>, ProblemHttpResult>> CompanyCreate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/companies/{result.Value.Id}", mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> CompanyUpdate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyUpdateCommand(model))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> CompanyDelete(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyDeleteCommand(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok()
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}