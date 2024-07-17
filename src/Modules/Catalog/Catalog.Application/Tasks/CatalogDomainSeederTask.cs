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
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Tenant> tenantRepository,
    IGenericRepository<Tag> tagRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Publisher> publisherRepository,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository) : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var companies = await this.SeedCompanies(companyRepository);
        var tenants = await this.SeedTenants(tenantRepository, companies);
        var customers = await this.SeedCustomers(customerRepository, tenants);
        var tags = await this.SeedTags(tagRepository, tenants);
        var categories = await this.SeedCategories(categoryRepository, tenants);
        var publishers = await this.SeedPublishers(publisherRepository, tenants);
        var authors = await this.SeedAuthors(authorRepository, tenants);
        var books = await this.SeedBooks(bookRepository, tenants, tags, categories, publishers, authors);
    }

    private async Task<Tenant[]> SeedTenants(IGenericRepository<Tenant> repository, Company[] companies)
    {
        var tenants = CoreSeedModels.Tenants.Create(companies);

        foreach (var tenant in tenants)
        {
            if (!await repository.ExistsAsync(tenant.Id))
            {
                await repository.InsertAsync(tenant);
            }
        }

        return tenants;
    }

    private async Task<Customer[]> SeedCustomers(IGenericRepository<Customer> repository, Tenant[] tenants)
    {
        var customers = CoreSeedModels.Customers.Create(tenants);

        foreach (var customer in customers)
        {
            if (!await repository.ExistsAsync(customer.Id))
            {
                await repository.InsertAsync(customer);
            }
        }

        return customers;
    }

    private async Task<Company[]> SeedCompanies(IGenericRepository<Company> repository)
    {
        var companies = CoreSeedModels.Companies.Create();

        foreach (var company in companies)
        {
            if (!await repository.ExistsAsync(company.Id))
            {
                await repository.InsertAsync(company);
            }
        }

        return companies;
    }

    private async Task<Tag[]> SeedTags(IGenericRepository<Tag> repository, Tenant[] tenants)
    {
        var tags = CoreSeedModels.Tags.Create(tenants);

        foreach (var tag in tags)
        {
            if (!await repository.ExistsAsync(tag.Id))
            {
                await repository.InsertAsync(tag);
            }
        }

        return tags;
    }

    private async Task<Category[]> SeedCategories(IGenericRepository<Category> repository, Tenant[] tenants)
    {
        var categories = CoreSeedModels.Categories.Create(tenants);

        foreach (var category in categories)
        {
            if (!await repository.ExistsAsync(category.Id))
            {
                await repository.InsertAsync(category);
            }
        }

        return categories;
    }

    private async Task<Publisher[]> SeedPublishers(IGenericRepository<Publisher> repository, Tenant[] tenants)
    {
        var publishers = CoreSeedModels.Publishers.Create(tenants);

        foreach (var publisher in publishers)
        {
            if (!await repository.ExistsAsync(publisher.Id))
            {
                await repository.InsertAsync(publisher);
            }
        }

        return publishers;
    }

    private async Task<Book[]> SeedBooks(IGenericRepository<Book> repository, Tenant[] tenants, Tag[] tags, Category[] categories, Publisher[] publishers, Author[] authors)
    {
        var books = CoreSeedModels.Books.Create(tenants, tags, categories, publishers, authors);

        foreach (var book in books)
        {
            if (!await repository.ExistsAsync(book.Id))
            {
                await repository.InsertAsync(book);
            }
        }

        return books;
    }

    private async Task<Author[]> SeedAuthors(IGenericRepository<Author> repository, Tenant[] tenants)
    {
        var authors = CoreSeedModels.Authors.Create(tenants);

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