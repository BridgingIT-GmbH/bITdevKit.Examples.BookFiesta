// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Microsoft.Extensions.Logging.Abstractions;

public class CatalogDomainSeederTask(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tag> tagRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Book> bookRepository) : IStartupTask
{
    private readonly ILogger<CatalogDomainSeederTask> logger =
        loggerFactory?.CreateLogger<CatalogDomainSeederTask>() ??
        NullLoggerFactory.Instance.CreateLogger<CatalogDomainSeederTask>();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("{LogKey} seed catalog (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var tags = await this.SeedTags(tagRepository, tenantIds);
        var customers = await this.SeedCustomers(customerRepository, tenantIds);
        var authors = await this.SeedAuthors(authorRepository, tenantIds, tags);
        var publishers = await this.SeedPublishers(publisherRepository, tenantIds);
        var categories = await this.SeedCategories(categoryRepository, tenantIds);
        var books = await this.SeedBooks(bookRepository, tenantIds, tags, categories, publishers, authors);
    }

    private async Task<Tag[]> SeedTags(IGenericRepository<Tag> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed tags (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Tags.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Customer[]> SeedCustomers(IGenericRepository<Customer> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed customers (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Customers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Author[]> SeedAuthors(IGenericRepository<Author> repository, TenantId[] tenantIds, Tag[] tags)
    {
        this.logger.LogInformation("{LogKey} seed authors (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Authors.Create(tenantIds, tags);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Publisher[]> SeedPublishers(IGenericRepository<Publisher> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed publishers (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Publishers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Category[]> SeedCategories(IGenericRepository<Category> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed categories (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Categories.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Book[]> SeedBooks(
        IGenericRepository<Book> repository,
        TenantId[] tenantIds,
        Tag[] tags,
        Category[] categories,
        Publisher[] publishers,
        Author[] authors)
    {
        this.logger.LogInformation("{LogKey} seed books (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Books.Create(tenantIds, tags, categories, publishers, authors);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }
}