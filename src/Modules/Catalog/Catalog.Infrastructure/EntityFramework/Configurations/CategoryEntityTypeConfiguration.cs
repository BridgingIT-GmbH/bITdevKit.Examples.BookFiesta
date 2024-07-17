﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories")
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder.Navigation(e => e.Parent).AutoInclude(false);
        //builder.Navigation(e => e.Children).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => CategoryId.Create(value));

        builder.Property(e => e.Title)
            .IsRequired().HasMaxLength(512);

        builder.Property(e => e.Description)
            .IsRequired(false);

        builder.Property(e => e.Order)
            .IsRequired(true).HasDefaultValue(0);

        builder.HasMany(c => c.Children)
            .WithOne(c => c.Parent)
            .OnDelete(DeleteBehavior.Restrict);

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}