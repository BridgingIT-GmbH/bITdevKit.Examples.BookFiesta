// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Infrastructure;

using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

public class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
    : ModuleDbContextBase(options), IDocumentStoreContext, IOutboxDomainEventContext, IOutboxMessageContext
{
    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<StorageDocument> StorageDocuments { get; set; }

    public DbSet<OutboxDomainEvent> OutboxDomainEvents { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}