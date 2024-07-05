// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;

using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        ConfigureCustomersTable(builder);
    }

    private static void ConfigureCustomersTable(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers")
               .HasKey(d => d.Id);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value));

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.LastName)
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
                .HasColumnName(nameof(Customer.Email))
                .IsRequired(false)
                .HasMaxLength(256);

            b.HasIndex(nameof(Customer.Email.Value))
                .IsUnique();
        });
        builder.Navigation(e => e.Email).IsRequired();

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}