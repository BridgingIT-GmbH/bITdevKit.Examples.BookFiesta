// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        ConfigureCustomersTable(builder);
    }

    private static void ConfigureCustomersTable(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => CustomerId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.OwnsOne(
            e => e.PersonName,
            b =>
            {
                b.Property(e => e.Title)
                    .HasColumnName("PersonNameTitle").IsRequired(false).HasMaxLength(64);
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

        builder.OwnsOne(
            e => e.Address,
            b =>
            {
                b.Property(e => e.Name).HasColumnName("AddressName").HasMaxLength(512).IsRequired();

                b.Property(e => e.Line1).HasColumnName("AddressLine1").HasMaxLength(256).IsRequired();

                b.Property(e => e.Line2).HasColumnName("AddressLine2").HasMaxLength(256);

                b.Property(e => e.City).HasColumnName("AddressCity").HasMaxLength(128).IsRequired();

                b.Property(e => e.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(32).IsRequired();

                b.Property(e => e.Country).HasColumnName("AddressCountry").HasMaxLength(128).IsRequired();
            });

        builder.Property(e => e.Email)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value))
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(nameof(Customer.TenantId), nameof(Customer.Email)).IsUnique();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}