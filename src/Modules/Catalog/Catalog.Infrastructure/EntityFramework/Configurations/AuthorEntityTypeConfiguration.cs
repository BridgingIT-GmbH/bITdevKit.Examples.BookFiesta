// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AuthorEntityTypeConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        builder.HasKey(a => a.Id);
        builder.Navigation(e => e.Tags).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => AuthorId.Create(value));

        builder.OwnsMany(e => e.Books, rb =>
        {
            rb.ToTable("AuthorBooks");

            rb.WithOwner().HasForeignKey("AuthorId");
            // TODO: BookId foreign key is missing in migration
            rb.HasKey("Id");

            rb.Property(r => r.Id);

            rb.Property(r => r.BookId)
                .IsRequired()
                .HasConversion(
                    id => id.Value,
                    value => BookId.Create(value));
        });

        builder.Property(a => a.Biography)
            .IsRequired(false).HasMaxLength(4096);

        builder.OwnsOne(e => e.PersonName, b =>
        {
            b.Property(e => e.Title)
                .HasColumnName("PersonNameTitle")
                .IsRequired(false).HasMaxLength(64);
            b.Property(e => e.Parts)
                .HasColumnName("PersonNameParts")
                .IsRequired(true).HasMaxLength(1024)
                .HasConversion(
                    parts => string.Join("|", parts),
                    value => value.Split("|", StringSplitOptions.RemoveEmptyEntries),
                    new ValueComparer<IEnumerable<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.AsEnumerable()));
            b.Property(e => e.Suffix)
                .HasColumnName("PersonNameSuffix")
                .IsRequired(false).HasMaxLength(64);
            b.Property(e => e.Full)
                .HasColumnName("PersonNameFull")
                .IsRequired(true).HasMaxLength(2048);
        });

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        // Configure relationships
        // Assuming a many-to-many relationship is managed through BookEntityTypeConfiguration

        builder.Metadata.FindNavigation(nameof(Author.Books))
           .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
