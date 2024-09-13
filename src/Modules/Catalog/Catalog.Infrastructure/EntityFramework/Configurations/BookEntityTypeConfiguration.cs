// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Book>, IEntityTypeConfiguration<BookKeyword>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        ConfigureBooks(builder);
        ConfigureBookAuthors(builder);
        ConfigureBookCategories(builder);
        ConfigureBookChapters(builder);
        ConfigureBookPublisher(builder);
    }

    public void Configure(EntityTypeBuilder<BookKeyword> builder)
    {
        ConfigureBookKeywords(builder);
    }

    private static void ConfigureBooks(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books")
            .HasKey(e => e.Id)
            .IsClustered(false);

        //builder.Navigation(e => e.BookAuthors).AutoInclude();
        builder.Navigation(e => e.Categories)
            .AutoInclude();
        builder.Navigation(e => e.Chapters)
            .AutoInclude();
        builder.Navigation(e => e.Tags)
            .AutoInclude();
        builder.Navigation(e => e.Keywords)
            .AutoInclude();

        builder.Property(e => e.Version)
            .IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => BookId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value))
            .IsRequired();

        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.Edition)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Property(e => e.Description)
            .IsRequired(false);

        builder.Property(e => e.PublishedDate)
            .IsRequired(false);

        builder.OwnsOne(e => e.Isbn,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("Isbn")
                    .IsRequired()
                    .HasMaxLength(32);
                b.Property(e => e.Type)
                    .HasColumnName("IsbnType")
                    .IsRequired(false)
                    .HasMaxLength(32);

                b.HasIndex(nameof(BookIsbn.Value))
                    .IsUnique();
            });

        builder.OwnsOne(e => e.AverageRating,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("AverageRating")
                    //.HasDefaultValue(0m)
                    .IsRequired(false)
                    .HasColumnType("decimal(5,2)");

                b.Property(e => e.Amount)
                    .HasColumnName("AverageRatingAmount")
                    .HasDefaultValue(0)
                    .IsRequired();
            });
        builder.Navigation(e => e.AverageRating)
            .IsRequired();

        builder.OwnsOne(e => e.Price,
            b =>
            {
                b.Property(e => e.Amount)
                    .HasColumnName("Price")
                    .HasDefaultValue(0)
                    .IsRequired()
                    .HasColumnType("decimal(5,2)");

                b.OwnsOne(e => e.Currency,
                    b =>
                    {
                        b.Property(e => e.Code)
                            .HasColumnName("PriceCurrency")
                            .HasDefaultValue("USD")
                            .IsRequired()
                            .HasMaxLength(8);
                    });
            });

        builder.HasMany(e => e.Tags) // unidirectional many-to-many relationship https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#unidirectional-many-to-many
            .WithMany()
            .UsingEntity(b => b.ToTable("BookTags"));

        builder.HasMany(b => b.Keywords)
            .WithOne()
            .HasForeignKey(ki => ki.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        builder.Metadata.FindNavigation(nameof(Book.Authors))
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureBookAuthors(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsMany(e => e.Authors,
            b =>
            {
                b.ToTable("BookAuthors")
                    .HasKey("BookId", "AuthorId");
                b.HasIndex("BookId", "AuthorId");

                b.WithOwner()
                    .HasForeignKey("BookId");

                b.Property(r => r.AuthorId)
                    .IsRequired()
                    .HasConversion(id => id.Value, value => AuthorId.Create(value));
                b.HasOne(typeof(Author))
                    .WithMany()
                    .HasForeignKey(nameof(AuthorId)); // FK -> Author.Id

                b.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(2048);

                b.Property(r => r.Position)
                    .IsRequired()
                    .HasDefaultValue(0);
            });
    }

    private static void ConfigureBookCategories(EntityTypeBuilder<Book> builder)
    {
        // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#many-to-many-with-join-table-foreign-key-names
        builder.HasMany(e => e.Categories)
            .WithMany(e => e.Books)
            .UsingEntity("BookCategories",
                l => l.HasOne(typeof(Category))
                    .WithMany()
                    .HasForeignKey(nameof(CategoryId)),
                r => r.HasOne(typeof(Book))
                    .WithMany()
                    .HasForeignKey(nameof(BookId)));
    }

    private static void ConfigureBookChapters(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsMany(e => e.Chapters,
            b =>
            {
                b.ToTable("BookChapters");
                b.WithOwner()
                    .HasForeignKey("BookId");
                b.HasKey("Id", "BookId");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => BookChapterId.Create(value));

                b.Property("Title")
                    .IsRequired()
                    .HasMaxLength(256);
                b.Property("Content")
                    .IsRequired(false);
            });
    }

    private static void ConfigureBookPublisher(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsOne(e => e.Publisher,
            b =>
            {
                b.Property(r => r.PublisherId)
                    .HasColumnName("PublisherId")
                    .IsRequired()
                    .HasConversion(id => id.Value, value => PublisherId.Create(value));
                b.HasOne(typeof(Publisher))
                    .WithMany()
                    .HasForeignKey(nameof(PublisherId)); // FK -> Publisher.Id

                b.Property(e => e.Name)
                    .HasColumnName("PublisherName")
                    .IsRequired()
                    .HasMaxLength(512);
            });
    }

    private static void ConfigureBookKeywords(EntityTypeBuilder<BookKeyword> builder)
    {
        builder.ToTable("BookKeywords")
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => BookKeywordId.Create(value));

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