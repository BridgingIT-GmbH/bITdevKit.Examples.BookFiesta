// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using DevKit.Infrastructure.EntityFramework;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Domain;

public class AuthorEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        ConfigureAuthors(builder);
        ConfigureAuthorBooks(builder);
    }

    private static void ConfigureAuthors(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors").HasKey(e => e.Id).IsClustered(false);

        builder.Navigation(e => e.Tags).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => AuthorId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();

        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants") // The navigation 'organization.Tenants' cannot be added to the entity type 'Author' because there is no corresponding CLR property on the underlying type and navigations properties cannot be added in shadow state.
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired()
        //    .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Biography).IsRequired(false).HasMaxLength(4096);

        builder.OwnsOne(
            e => e.PersonName,
            b =>
            {
                b.Property(e => e.Title).HasColumnName("PersonNameTitle").IsRequired(false).HasMaxLength(64);
                b.Property(e => e.Parts)
                    .HasColumnName("PersonNameParts")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasConversion(
                        parts => string.Join("|", parts),
                        value => value.Split("|", StringSplitOptions.RemoveEmptyEntries),
                        new ValueComparer<IEnumerable<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.AsEnumerable()));
                b.Property(e => e.Suffix).HasColumnName("PersonNameSuffix").IsRequired(false).HasMaxLength(64);
                b.Property(e => e.Full).HasColumnName("PersonNameFull").IsRequired().HasMaxLength(2048);
            });

        builder
            .HasMany(
                e => e.Tags) // unidirectional many-to-many relationship https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#unidirectional-many-to-many
            .WithMany()
            .UsingEntity(b => b.ToTable("AuthorTags"));

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        // Configure relationships
        // Assuming a many-to-many relationship is managed through BookEntityTypeConfiguration

        builder.Metadata.FindNavigation(nameof(Author.Books)).SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureAuthorBooks(EntityTypeBuilder<Author> builder)
    {
        builder.OwnsMany(
            e => e.Books,
            b =>
            {
                b.ToTable("AuthorBooks").HasKey("AuthorId", "BookId");
                b.HasIndex("AuthorId", "BookId");

                b.WithOwner().HasForeignKey("AuthorId");

                b.Property(r => r.BookId).IsRequired().HasConversion(id => id.Value, value => BookId.Create(value));
                b.HasOne(typeof(Book)).WithMany().HasForeignKey(nameof(BookId)); // FK -> Book.Id

                b.Property(r => r.Title).IsRequired().HasMaxLength(512);
            });
    }
}