// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using System.Reflection;

public class BookEntityTypeConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(e => e.Id);
        //builder.Navigation(e => e.BookAuthors).AutoInclude();
        builder.Navigation(e => e.Categories).AutoInclude();
        builder.Navigation(e => e.Chapters).AutoInclude();
        builder.Navigation(e => e.Tags).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => BookId.Create(value));

        //builder.HasMany<BookAuthor>()
        //    .WithOne().HasForeignKey(e => e.BookId);

        // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#many-to-many-with-join-table-foreign-key-names
        builder.HasMany(e => e.Categories)
            .WithMany(e => e.Books).UsingEntity(
            "BookCategories",
            l => l.HasOne(typeof(Category)).WithMany().HasForeignKey(nameof(CategoryId)),
            r => r.HasOne(typeof(Book)).WithMany().HasForeignKey(nameof(BookId)));

        builder.Property(e => e.Title)
            .IsRequired().HasMaxLength(512);

        builder.Property(e => e.Description)
            .IsRequired(false);

        builder.OwnsOne(e => e.Isbn, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName("Isbn")
                .IsRequired().HasMaxLength(32);
            b.Property(e => e.Type)
                .HasColumnName("IsbnType")
                .IsRequired(false).HasMaxLength(32);

            b.HasIndex(nameof(BookIsbn.Value))
                .IsUnique();
        });

        builder.OwnsOne(e => e.Price, b =>
        {
            b.Property(e => e.Amount)
                .HasColumnName("Price")
                .HasDefaultValue(0)
                .IsRequired().HasColumnType("decimal(5,2)");

            b.OwnsOne(e => e.Currency, b =>
            {
                b.Property(e => e.Code)
                    .HasColumnName("PriceCurrency")
                    .HasDefaultValue("USD")
                    .IsRequired().HasMaxLength(8);
            });
        });

        builder.OwnsMany(e => e.Authors, rb =>
        {
            rb.ToTable("BookAuthors");

            rb.WithOwner().HasForeignKey("BookId");
            // TODO: AuthorId foreign key is missing in migration
            rb.HasKey("Id");

            rb.Property(r => r.Id);

            rb.Property(r => r.AuthorId)
                .IsRequired()
                .HasConversion(
                    id => id.Value,
                    value => AuthorId.Create(value));

            rb.Property(r => r.Position)
                .IsRequired().HasDefaultValue(0);
        });

        builder.OwnsMany(e => e.Chapters, b =>
        {
            b.ToTable("BookChapters");
            b.WithOwner().HasForeignKey("BookId");
            b.HasKey("Id", "BookId");

            b.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(
                    id => id.Value,
                    value => BookChapterId.Create(value));

            b.Property("Title")
                .IsRequired().HasMaxLength(256);
            b.Property("Content")
                .IsRequired(false);
        });

        builder.HasMany(e => e.Tags) // unidirectional many-to-many relationship https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#unidirectional-many-to-many
            .WithMany()
            .UsingEntity(b => b.ToTable("BookTags"));

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        builder.Metadata.FindNavigation(nameof(Book.Authors))
                    .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
