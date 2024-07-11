// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class BookKeywordEntityTypeConfiguration : IEntityTypeConfiguration<BookKeyword>
{
    public void Configure(EntityTypeBuilder<BookKeyword> builder)
    {
        builder.ToTable("BookKeywords");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => BookKeywordId.Create(value));

        builder.Property(e => e.Text)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(e => e.Text);

        builder.HasIndex(e => new { e.BookId, e.Text })
            .IsUnique();

        builder.HasOne<Book>()
            .WithMany(e => e.Keywords)
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}