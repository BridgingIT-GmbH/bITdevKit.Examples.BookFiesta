﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PublisherEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Publisher>
{
    public override void Configure(EntityTypeBuilder<Publisher> builder)
    {
        base.Configure(builder);

        ConfigurePublishersTable(builder);
    }

    private static void ConfigurePublishersTable(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable("Publishers").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => PublisherId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(512);

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

        builder.Property(e => e.ContactEmail)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value))
            .IsRequired(false)
            .HasMaxLength(256);
        builder.HasIndex(nameof(Publisher.TenantId), nameof(Publisher.ContactEmail)).IsUnique();

        builder.OwnsOne(
            e => e.Website,
            b =>
            {
                b.Property(e => e.Value).HasColumnName(nameof(Publisher.Website)).IsRequired(false).HasMaxLength(512);
            });
        builder.Navigation(e => e.Website).IsRequired();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}