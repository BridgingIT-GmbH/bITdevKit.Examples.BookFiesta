// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Domain.Model;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public abstract class TenantAwareEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property<TenantId>("TenantId")
            .HasConversion(
                id => id.Value,
                value => TenantId.Create(value))
            .IsRequired();

        builder.HasIndex("TenantId");

        builder.HasOne<TenantReference>()
            .WithMany()
            .HasForeignKey("TenantId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TenantReferenceEntityTypeConfiguration : IEntityTypeConfiguration<TenantReference>
{
    public void Configure(EntityTypeBuilder<TenantReference> builder)
    {
        builder.ToTable("Tenants", "organization")
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => TenantId.Create(value));

        builder.ToTable(tb => tb.ExcludeFromMigrations());
    }
}

public class TenantReference
{
    public TenantId Id { get; set; }
}