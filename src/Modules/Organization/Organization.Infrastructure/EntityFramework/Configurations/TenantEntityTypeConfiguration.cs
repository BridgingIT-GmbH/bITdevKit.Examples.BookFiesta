// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

using BridgingIT.DevKit.Domain.Model;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TenantEntityTypeConfiguration :
    IEntityTypeConfiguration<Tenant>,
    IEntityTypeConfiguration<TenantBranding>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        ConfigureTenants(builder);
        ConfigureTenantSubscriptions(builder);
    }

    public void Configure(EntityTypeBuilder<TenantBranding> builder)
    {
        ConfigureTenantBranding(builder);
    }

    private static void ConfigureTenants(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants")
            .HasKey(d => d.Id)
            .IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => TenantId.Create(value));

        builder.Property(e => e.Name)
            .IsRequired().HasMaxLength(512);

        builder.Property(e => e.Description)
            .IsRequired(false);

        builder.OwnsOne(e => e.ContactEmail, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(Tenant.ContactEmail))
                .IsRequired(true)
                .HasMaxLength(256);
        });
        builder.Navigation(e => e.ContactEmail).IsRequired();

        builder.HasOne<Company>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }

    private static void ConfigureTenantSubscriptions(EntityTypeBuilder<Tenant> builder)
    {
        builder.OwnsMany(e => e.Subscriptions, b =>
        {
            b.ToTable("TenantSubscriptions")
                .HasKey(e => e.Id)
                .IsClustered(false);
            b.WithOwner(e => e.Tenant);

            b.Property(e => e.Version).IsConcurrencyToken();

            b.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasConversion(
                    id => id.Value,
                    id => TenantSubscriptionId.Create(id));

            b.Property(e => e.PlanType)
                //.HasConversion(
                //    status => status.Id,
                //    id => Enumeration.FromId<TenantSubscriptionPlanType>(id))
                .HasConversion(
                    new EnumerationConverter<TenantSubscriptionPlanType>())
                .IsRequired();

            b.Property(e => e.Status)
                //.HasConversion(
                //    status => status.Id,
                //    id => Enumeration.FromId<TenantSubscriptionStatus>(id))
                .HasConversion(
                    new EnumerationConverter<TenantSubscriptionStatus>())
                .IsRequired();

            b.Property(e => e.BillingCycle)
                //.HasConversion(
                //    status => status.Id,
                //    id => Enumeration.FromId<TenantSubscriptionBillingCycle>(id))
                .HasConversion(//.HasConversion(
                    new EnumerationConverter<TenantSubscriptionBillingCycle>())
                .IsRequired();

            b.OwnsOne(e => e.Schedule, b =>
            {
                b.Property(e => e.StartDate)
                    .IsRequired();

                b.Property(e => e.EndDate)
                    .IsRequired(false);
            });
            b.Navigation(e => e.Schedule).IsRequired();
        });
    }

    private static void ConfigureTenantBranding(EntityTypeBuilder<TenantBranding> builder)
    {
        builder.ToTable("TenantBrandings")
            .HasKey(e => e.Id)
            .IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => TenantBrandingId.Create(value));

        builder.Property(e => e.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.Create(value));

        builder.HasOne<Tenant>()
            .WithOne(e => e.Branding)
            .IsRequired()
            .HasForeignKey<TenantBranding>(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.PrimaryColor, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(TenantBranding.PrimaryColor))
                .IsRequired(false)
                .HasMaxLength(16);
        });
        builder.Navigation(e => e.PrimaryColor).IsRequired();

        builder.OwnsOne(e => e.SecondaryColor, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(TenantBranding.SecondaryColor))
                .IsRequired(false)
                .HasMaxLength(16);
        });
        builder.Navigation(e => e.SecondaryColor).IsRequired();

        builder.OwnsOne(e => e.LogoUrl, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(TenantBranding.LogoUrl))
                .IsRequired(false)
                .HasMaxLength(512);
        });
        builder.Navigation(e => e.LogoUrl).IsRequired();

        builder.OwnsOne(e => e.FaviconUrl, b =>
        {
            b.Property(e => e.Value)
                .HasColumnName(nameof(TenantBranding.FaviconUrl))
                .IsRequired(false)
                .HasMaxLength(512);
        });
        builder.Navigation(e => e.FaviconUrl).IsRequired();
    }
}