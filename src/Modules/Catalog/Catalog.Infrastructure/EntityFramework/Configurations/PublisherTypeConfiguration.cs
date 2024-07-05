// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PublisherTypeConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        ConfigurePublishersTable(builder);
    }

    private static void ConfigurePublishersTable(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable("Publishers")
               .HasKey(d => d.Id);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => PublisherId.Create(value));

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(512);

        builder.OwnsOne(e => e.Address, b =>
        {
            b.Property(e => e.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(256)
                .IsRequired();

            b.Property(e => e.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(256);

            b.Property(e => e.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(128)
                .IsRequired();

            b.Property(e => e.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(32)
                .IsRequired();

            b.Property(e => e.Country)
                .HasColumnName("AddressCountry")
                .HasMaxLength(128)
                .IsRequired();
        });

        builder.OwnsOne(e => e.Email, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(Publisher.Email))
                .IsRequired(false)
                .HasMaxLength(256);

            b.HasIndex(nameof(Customer.Email.Value))
                .IsUnique();
        });
        builder.Navigation(e => e.Email).IsRequired();

        builder.OwnsOne(e => e.Website, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(Publisher.Website))
                .IsRequired(false)
                .HasMaxLength(512);
        });
        builder.Navigation(e => e.Website).IsRequired();

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}