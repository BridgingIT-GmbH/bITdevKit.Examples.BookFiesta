﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

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

        builder.Property(e => e.Name)
            .IsRequired().HasMaxLength(128);

        builder.HasIndex(nameof(Tag.Name))
             .IsUnique();
    }
}
