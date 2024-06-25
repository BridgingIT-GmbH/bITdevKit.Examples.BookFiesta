// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Presentation.Web.Controllers;

using System.Threading;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.GettingStarted.Application;
using BridgingIT.DevKit.Examples.GettingStarted.Domain;
using BridgingIT.DevKit.Examples.GettingStarted.Presentation;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(IMapper mapper, IMediator mediator) : ControllerBase // TODO: use the new IEndpoints from bitdevkit, see Maps below
{
    private readonly IMediator mediator = mediator;
    private readonly IMapper mapper = mapper;

    //[HttpGet("{id}", Name = nameof(Get))]
    //public async Task<ActionResult<CategoryModel>> Get(string id, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        new CategoryFindOneQuery(id), cancellationToken)).Result;
    //    return result.ToOkActionResult<Category, CategoryModel>(this.mapper);
    //}

    [HttpGet]
    public async Task<ActionResult<ICollection<CategoryModel>>> GetAll(CancellationToken cancellationToken)
    {
        var result = (await this.mediator.Send(
            new CategoryFindAllQuery(), cancellationToken)).Result;
        return result.ToOkActionResult<Category, CategoryModel>(this.mapper);
    }

    //[HttpPost]
    //public async Task<ActionResult<CategoryModel>> PostAsync([FromBody] CategoryModel model, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        this.mapper.Map<CategoryModel, CategoryCreateCommand>(model), cancellationToken)).Result;
    //    return result.ToCreatedActionResult<Category, CategoryModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
    //}

    //[HttpPut("{id}")]
    //public async Task<ActionResult<CategoryModel>> PutAsync(string id, [FromBody] CategoryModel model, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        this.mapper.Map<CategoryModel, CategoryUpdateCommand>(model), cancellationToken)).Result;
    //    return result.ToUpdatedActionResult<Category, CategoryModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
    //}

    //[HttpDelete("{id}")]
    //public async Task<ActionResult<CategoryModel>> DeleteAsync(string id, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(new CategoryDeleteCommand { Id = id }, cancellationToken)).Result;
    //    return result.ToDeletedActionResult<CategoryModel>(); // TODO: remove generic CategoryModel
    //}
}

//app.MapGet("/api/customers/{id}", async(string id, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new CategoryFindOneQuery(id), cancellationToken)).Result;
//    return result.ToOkActionResult<Category, CategoryModel>(mapper);
//}).WithName("GetCategory");

//// Endpoint for GetAll action
//app.MapGet("/api/customers", async (IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new CategoryFindAllQuery(), cancellationToken)).Result;
//    return result.ToOkActionResult<Category, CategoryModel>(mapper);
//});

//// Endpoint for PostAsync action
//app.MapPost("/api/customers", async (CategoryModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(mapper.Map<CategoryModel, CategoryCreateCommand>(model), cancellationToken)).Result;
//    return result.ToCreatedActionResult<Category, CategoryModel>(mapper, "GetCategory", new { id = result.Value?.Id });
//});

//// Endpoint for PutAsync action
//app.MapPut("/api/customers/{id}", async (string id, CategoryModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(mapper.Map<CategoryModel, CategoryUpdateCommand>(model), cancellationToken)).Result;
//    return result.ToUpdatedActionResult<Category, CategoryModel>(mapper, "GetCategory", new { id = result.Value?.Id });
//});

//// Endpoint for DeleteAsync action
//app.MapDelete("/api/customers/{id}", async (string id, IMediator mediator, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new CategoryDeleteCommand { Id = id }, cancellationToken)).Result;
//    return result.ToDeletedActionResult<CategoryModel>();
//});