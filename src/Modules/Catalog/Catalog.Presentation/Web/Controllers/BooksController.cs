// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Presentation.Web.Controllers;

using System.Threading;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.Presentation;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IMapper mapper, IMediator mediator) : ControllerBase // TODO: use the new IEndpoints from bitdevkit, see Maps below
{
    private readonly IMediator mediator = mediator;
    private readonly IMapper mapper = mapper;

    //[HttpGet("{id}", Name = nameof(Get))]
    //public async Task<ActionResult<BookModel>> Get(string id, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        new BookFindOneQuery(id), cancellationToken)).Result;
    //    return result.ToOkActionResult<Book, BookModel>(this.mapper);
    //}

    [HttpGet]
    public async Task<ActionResult<ICollection<BookModel>>> GetAll(CancellationToken cancellationToken)
    {
        var result = (await this.mediator.Send(
            new BookFindAllQuery(), cancellationToken)).Result;
        return result.ToOkActionResult<Book, BookModel>(this.mapper);
    }

    //[HttpPost]
    //public async Task<ActionResult<BookModel>> PostAsync([FromBody] BookModel model, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        this.mapper.Map<BookModel, BookCreateCommand>(model), cancellationToken)).Result;
    //    return result.ToCreatedActionResult<Book, BookModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
    //}

    //[HttpPut("{id}")]
    //public async Task<ActionResult<BookModel>> PutAsync(string id, [FromBody] BookModel model, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(
    //        this.mapper.Map<BookModel, BookUpdateCommand>(model), cancellationToken)).Result;
    //    return result.ToUpdatedActionResult<Book, BookModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
    //}

    //[HttpDelete("{id}")]
    //public async Task<ActionResult<BookModel>> DeleteAsync(string id, CancellationToken cancellationToken)
    //{
    //    var result = (await this.mediator.Send(new BookDeleteCommand { Id = id }, cancellationToken)).Result;
    //    return result.ToDeletedActionResult<BookModel>(); // TODO: remove generic BookModel
    //}
}

//app.MapGet("/api/customers/{id}", async(string id, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new BookFindOneQuery(id), cancellationToken)).Result;
//    return result.ToOkActionResult<Book, BookModel>(mapper);
//}).WithName("GetBook");

//// Endpoint for GetAll action
//app.MapGet("/api/customers", async (IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new BookFindAllQuery(), cancellationToken)).Result;
//    return result.ToOkActionResult<Book, BookModel>(mapper);
//});

//// Endpoint for PostAsync action
//app.MapPost("/api/customers", async (BookModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(mapper.Map<BookModel, BookCreateCommand>(model), cancellationToken)).Result;
//    return result.ToCreatedActionResult<Book, BookModel>(mapper, "GetBook", new { id = result.Value?.Id });
//});

//// Endpoint for PutAsync action
//app.MapPut("/api/customers/{id}", async (string id, BookModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(mapper.Map<BookModel, BookUpdateCommand>(model), cancellationToken)).Result;
//    return result.ToUpdatedActionResult<Book, BookModel>(mapper, "GetBook", new { id = result.Value?.Id });
//});

//// Endpoint for DeleteAsync action
//app.MapDelete("/api/customers/{id}", async (string id, IMediator mediator, CancellationToken cancellationToken) =>
//{
//    var result = (await mediator.Send(new BookDeleteCommand { Id = id }, cancellationToken)).Result;
//    return result.ToDeletedActionResult<BookModel>();
//});