// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

using Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyEntityTypeConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        ConfigureCompanies(builder);
    }

    private static void ConfigureCompanies(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies")
            .HasKey(d => d.Id)
            .IsClustered(false);

        builder.Property(e => e.Version)
            .IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => CompanyId.Create(value));

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(512);

        builder.OwnsOne(e => e.Address,
            b =>
            {
                b.Property(e => e.Name)
                    .HasMaxLength(512)
                    .IsRequired();

                b.Property(e => e.Line1)
                    .HasMaxLength(256)
                    .IsRequired();

                b.Property(e => e.Line2)
                    .HasMaxLength(256);

                b.Property(e => e.City)
                    .HasMaxLength(128)
                    .IsRequired();

                b.Property(e => e.PostalCode)
                    .HasMaxLength(32)
                    .IsRequired();

                b.Property(e => e.Country)
                    .HasMaxLength(128)
                    .IsRequired();
            });
        builder.Navigation(e => e.Address)
            .IsRequired();

        builder.Property(e => e.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(128);

        builder.OwnsOne(e => e.ContactEmail,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(Company.ContactEmail))
                    .IsRequired(true)
                    .HasMaxLength(256);
            });
        builder.Navigation(e => e.ContactEmail)
            .IsRequired();

        builder.OwnsOne(e => e.ContactPhone,
            b =>
            {
                b.Property(e => e.CountryCode)
                    .HasMaxLength(8)
                    .IsRequired(false);

                b.Property(e => e.Number)
                    .HasMaxLength(32)
                    .IsRequired(false);
            });
        builder.Navigation(e => e.ContactPhone)
            .IsRequired();

        builder.OwnsOne(e => e.Website,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(Company.Website))
                    .IsRequired(false)
                    .HasMaxLength(512);
            });
        builder.Navigation(e => e.Website)
            .IsRequired();

        builder.OwnsOne(e => e.VatNumber,
            b =>
            {
                b.Property(e => e.CountryCode)
                    .IsRequired(false)
                    .HasMaxLength(16);

                b.Property(e => e.Number)
                    .IsRequired(false)
                    .HasMaxLength(128);
            });
        builder.Navigation(e => e.VatNumber)
            .IsRequired();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}