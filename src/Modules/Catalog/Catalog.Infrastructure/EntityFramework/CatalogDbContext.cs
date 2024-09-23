// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using DevKit.Infrastructure.EntityFramework;
using Domain;
using Microsoft.EntityFrameworkCore;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : ModuleDbContextBase(options), IDocumentStoreContext, IOutboxDomainEventContext, IOutboxMessageContext
{
    public DbSet<Book> Books { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Author> Authors { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<StorageDocument> StorageDocuments { get; set; }

    public DbSet<OutboxDomainEvent> OutboxDomainEvents { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagEntityTypeConfiguration).Assembly);

    //    base.OnModelCreating(modelBuilder); // applies the other EntityTypeConfigurations
    //}
}