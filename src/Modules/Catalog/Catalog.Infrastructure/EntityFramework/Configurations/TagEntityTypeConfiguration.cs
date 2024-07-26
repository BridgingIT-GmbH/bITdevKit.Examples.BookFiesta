﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;

public class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags")
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => TagId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId)
        .HasConversion(
            id => id.Value,
            value => TenantId.Create(value));
        builder.HasIndex(e => e.TenantId);
        builder.HasOne("organization.Tenant")
            .WithMany()
            .HasForeignKey(nameof(TenantId))
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired().HasMaxLength(128);

        builder.HasIndex(nameof(Tag.Name))
             .IsUnique();
    }
}
