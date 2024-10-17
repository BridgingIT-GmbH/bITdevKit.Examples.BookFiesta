// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

/// <summary>
///     Handles the creation or update of a Book entity.
/// </summary>
/// <remarks>
/// The handler uses various repositories to manage the book data, including repositories for books, authors, and publishers.
/// It processes commands to either create a new book or update an existing one, ensuring that all domain rules are applied.
/// </remarks>
public class BookCreateOrUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository)
    : CommandHandlerBase<BookCreateOrUpdateCommand, Result<Book>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Book>>> Process(BookCreateOrUpdateCommand command, CancellationToken cancellationToken)
    {
        var result = await this.ProcessOperationAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CommandResponse.For(result);
        }

        await DomainRules.ApplyAsync([
                BookRules.IsbnMustBeUnique(bookRepository, result.Value)
            ],
            cancellationToken);

        await bookRepository.UpsertAsync(result.Value, cancellationToken).AnyContext(); // TODO: use new UpsertResultAsync (resultextension)

        return CommandResponse.Success(result.Value);
    }

    private async Task<Result<Book>> ProcessOperationAsync(BookCreateOrUpdateCommand command, CancellationToken cancellationToken)
    {
        var publisherResult = await this.GetPublisherAsync(command, cancellationToken);
        if (publisherResult.IsFailure)
        {
            return Result<Book>.Failure(publisherResult.Messages).WithErrors(publisherResult.Errors);
        }

        Result<Book> result;
        if (command.OperationType == UpsertOperationType.Create)
        {
            result = Result<Book>.Success(
                this.CreateBook(command, publisherResult.Value));
        }
        else
        {
            result = await bookRepository.FindOneResultAsync(
                BookId.Create(command.Model.Id),
                cancellationToken: cancellationToken).AnyContext();
            if (result.IsFailure)
            {
                return result;
            }

            this.UpdateBook(result.Value, command, publisherResult.Value);
        }

        await this.AssignAuthorsAsync(result.Value, command, cancellationToken);

        return result;
    }

    private Book CreateBook(BookCreateOrUpdateCommand command, Publisher publisher)
    {
        return Book.Create(
            TenantId.Create(command.TenantId),
            command.Model.Title,
            command.Model.Edition,
            command.Model.Description,
            Language.Create(command.Model.Language),
            ProductSku.Create(command.Model.Sku),
            BookIsbn.Create(command.Model.Isbn),
            Money.Create(command.Model.Price),
            publisher,
            command.Model.PublishedDate);
    }

    private void UpdateBook(Book book, BookCreateOrUpdateCommand command, Publisher publisher)
    {
        book.SetTitle(command.Model.Title);
        book.SetEdition(command.Model.Edition);
        book.SetDescription(command.Model.Description);
        book.SetLanguage(Language.Create(command.Model.Language));
        book.SetSku(ProductSku.Create(command.Model.Sku));
        book.SetIsbn(BookIsbn.Create(command.Model.Isbn));
        book.SetPrice(Money.Create(command.Model.Price));
        book.SetPublisher(publisher);
        book.SetPublishedDate(command.Model.PublishedDate);
    }

    private async Task AssignAuthorsAsync(Book book, BookCreateOrUpdateCommand command, CancellationToken cancellationToken)
    {
        foreach (var authorModel in command.Model.Authors)
        {
            var author = await authorRepository.FindOneAsync(
                AuthorId.Create(authorModel.Id),
                cancellationToken: cancellationToken).AnyContext();

            if (author != null)
            {
                book.AssignAuthor(author, authorModel.Position);
            }
        }
    }

    private async Task<Result<Publisher>> GetPublisherAsync(BookCreateOrUpdateCommand command, CancellationToken cancellationToken)
    {
        return await publisherRepository.FindOneResultAsync(
            PublisherId.Create(command.Model.Publisher.Id),
            cancellationToken: cancellationToken).AnyContext();
    }
}