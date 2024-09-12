// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

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
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customers = await this.SeedCustomers(customerRepository, tenantIds);
        var tags = await this.SeedTags(tagRepository, tenantIds);
        var categories = await this.SeedCategories(categoryRepository, tenantIds);
        var publishers = await this.SeedPublishers(publisherRepository, tenantIds);
        var authors = await this.SeedAuthors(authorRepository, tenantIds, tags);
        var books = await this.SeedBooks(bookRepository, tenantIds, tags, categories, publishers, authors);
    }

    private async Task<Customer[]> SeedCustomers(IGenericRepository<Customer> repository, TenantId[] tenantIds)
    {
        var entities = CatalogSeedEntities.Customers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<Tag[]> SeedTags(IGenericRepository<Tag> repository, TenantId[] tenantIds)
    {
        var entities = CatalogSeedEntities.Tags.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<Category[]> SeedCategories(IGenericRepository<Category> repository, TenantId[] tenantIds)
    {
        var entities = CatalogSeedEntities.Categories.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<Publisher[]> SeedPublishers(IGenericRepository<Publisher> repository, TenantId[] tenantIds)
    {
        var entities = CatalogSeedEntities.Publishers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<Book[]> SeedBooks(IGenericRepository<Book> repository, TenantId[] tenantIds, Tag[] tags, Category[] categories, Publisher[] publishers, Author[] authors)
    {
        var entities = CatalogSeedEntities.Books.Create(tenantIds, tags, categories, publishers, authors);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }

    private async Task<Author[]> SeedAuthors(IGenericRepository<Author> repository, TenantId[] tenantIds, Tag[] tags)
    {
        var entities = CatalogSeedEntities.Authors.Create(tenantIds, tags);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
        }

        return entities;
    }
}