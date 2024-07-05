// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class CatalogDomainSeederTask(
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<Tag> tagRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Publisher> publisherRepository,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository) : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var customers = await this.SeedCustomers(customerRepository);
        var tags = await this.SeedTags(tagRepository);
        var categories = await this.SeedCategories(categoryRepository);
        var publishers = await this.SeedPublishers(publisherRepository);
        var authors = await this.SeedAuthors(authorRepository);
        var books = await this.SeedBooks(bookRepository, tags, categories, publishers, authors);
    }

    private async Task<Customer[]> SeedCustomers(IGenericRepository<Customer> repository)
    {
        var customers = CoreSeedModels.Customers.Create();

        foreach (var customer in customers)
        {
            if (!await repository.ExistsAsync(customer.Id))
            {
                await repository.InsertAsync(customer);
            }
        }

        return customers;
    }

    private async Task<Tag[]> SeedTags(IGenericRepository<Tag> repository)
    {
        var tags = CoreSeedModels.Tags.Create();

        foreach (var tag in tags)
        {
            if (!await repository.ExistsAsync(tag.Id))
            {
                await repository.InsertAsync(tag);
            }
        }

        return tags;
    }

    private async Task<Category[]> SeedCategories(IGenericRepository<Category> repository)
    {
        var categories = CoreSeedModels.Categories.Create();

        foreach (var category in categories)
        {
            if (!await repository.ExistsAsync(category.Id))
            {
                await repository.InsertAsync(category);
            }
        }

        return categories;
    }

    private async Task<Publisher[]> SeedPublishers(IGenericRepository<Publisher> repository)
    {
        var publishers = CoreSeedModels.Publishers.Create();

        foreach (var publisher in publishers)
        {
            if (!await repository.ExistsAsync(publisher.Id))
            {
                await repository.InsertAsync(publisher);
            }
        }

        return publishers;
    }

    private async Task<Book[]> SeedBooks(IGenericRepository<Book> repository, Tag[] tags, Category[] categories, Publisher[] publishers, Author[] authors)
    {
        var books = CoreSeedModels.Books.Create(tags, categories, publishers, authors);

        foreach (var book in books)
        {
            if (!await repository.ExistsAsync(book.Id))
            {
                await repository.InsertAsync(book);
            }
        }

        return books;
    }

    private async Task<Author[]> SeedAuthors(IGenericRepository<Author> repository)
    {
        var authors = CoreSeedModels.Authors.Create();

        foreach (var author in authors)
        {
            if (!await repository.ExistsAsync(author.Id))
            {
                await repository.InsertAsync(author);
            }
        }

        return authors;
    }
}