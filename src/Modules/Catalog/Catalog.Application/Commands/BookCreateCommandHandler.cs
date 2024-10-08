// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Money = BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain.Money;

public class BookCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository)
    : CommandHandlerBase<BookCreateCommand, Result<Book>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Book>>> Process(
        BookCreateCommand command,
        CancellationToken cancellationToken)
    {
        var publisher = await publisherRepository.FindOneAsync(
                PublisherId.Create(command.Model.Publisher.Id),
                cancellationToken: cancellationToken)
            .AnyContext(); // TODO: Check if publisher exists

        var book = Book.Create(
            TenantId.Create(command.TenantId),
            command.Model.Title,
            command.Model.Edition,
            command.Model.Description,
            ProductSku.Create(command.Model.Sku),
            BookIsbn.Create(command.Model.Isbn),
            Money.Create(command.Model.Price),
            publisher,
            command.Model.PublishedDate);

        foreach (var bookAuthorModel in command.Model.Authors)
        {
            var author = await authorRepository.FindOneAsync(
                    AuthorId.Create(bookAuthorModel.Id),
                    cancellationToken: cancellationToken)
                .AnyContext(); // TODO: Check if author exists

            book.AssignAuthor(author, bookAuthorModel.Position);
        }

        await DomainRules.ApplyAsync([
                BookRules.IsbnMustBeUnique(bookRepository, book)
            ],
            cancellationToken);

        await bookRepository.InsertAsync(book, cancellationToken).AnyContext();

        return CommandResponse.Success(book);
    }
}