// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Money = BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain.Money;

public class BookUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository)
    : CommandHandlerBase<BookUpdateCommand, Result<Book>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Book>>> Process(
        BookUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var bookResult = await bookRepository.FindOneResultAsync(
                BookId.Create(command.Model.Id),
                cancellationToken: cancellationToken)
            .AnyContext();
        var book = bookResult.Value;

        if(bookResult.IsFailure)
        {
            return CommandResponse.For(bookResult);
        }

        var publisherResult = await publisherRepository.FindOneResultAsync(
                PublisherId.Create(command.Model.Publisher.Id),
                cancellationToken: cancellationToken)
            .AnyContext(); // TODO: Check if publisher exists

        if(publisherResult.IsFailure)
        {
            return CommandResponse.For<Book>(publisherResult);
        }

        book.SetTitle(command.Model.Title);
        book.SetEdition(command.Model.Edition);
        book.SetDescription(command.Model.Description);
        book.SetLanguage(Language.Create(command.Model.Language));
        book.SetSku(ProductSku.Create(command.Model.Sku));
        book.SetIsbn(BookIsbn.Create(command.Model.Isbn));
        book.SetPrice(Money.Create(command.Model.Price));
        book.SetPublisher(publisherResult.Value);
        book.SetPublishedDate(command.Model.PublishedDate);

        foreach (var bookAuthorModel in command.Model.Authors)
        {
            var author = await authorRepository.FindOneAsync(
                    AuthorId.Create(bookAuthorModel.Id),
                    cancellationToken: cancellationToken)
                .AnyContext(); // TODO: Check if author exists

            if (author != null)
            {
                book.AssignAuthor(author, bookAuthorModel.Position);
            }
        }

        await DomainRules.ApplyAsync([
                BookRules.IsbnMustBeUnique(bookRepository, book)
            ],
            cancellationToken);

        await bookRepository.UpsertAsync(book, cancellationToken).AnyContext();

        return CommandResponse.Success(book);
    }
}