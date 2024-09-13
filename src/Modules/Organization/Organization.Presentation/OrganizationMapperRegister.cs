// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;

using System.Linq;
using BridgingIT.DevKit.Domain.Model;
using Application;
using Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Mapster;

public class OrganizationMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterCompany(config);
        RegisterTenant(config);
    }

    private static void RegisterCompany(TypeAdapterConfig config)
    {
        config.ForType<CompanyModel, Company>()
            .IgnoreNullValues(true)
            .ConstructUsing(src => Company.Create(src.Name, src.RegistrationNumber, EmailAddress.Create(src.ContactEmail), MapAddress(src.Address)))
            .AfterMapping((src, dest) =>
            {
                if (dest.Id != null)
                {
                    dest.SetName(src.Name);
                    dest.SetRegistrationNumber(src.RegistrationNumber);
                    dest.SetContactEmail(EmailAddress.Create(src.ContactEmail));
                    dest.SetAddress(MapAddress(src.Address));
                }

                dest.SetContactPhone(PhoneNumber.Create(src.ContactPhone));
                dest.SetWebsite(Url.Create(src.Website));
                dest.SetVatNumber(VatNumber.Create(src.VatNumber));
            });

        config.ForType<AddressModel, Address>() // TODO: move to new SharedKernelMapperRegister
            .MapWith(src => MapAddress(src));
    }

    private static void RegisterTenant(TypeAdapterConfig config)
    {
        config.ForType<TenantModel, Tenant>()
            .IgnoreNullValues(true)
            .ConstructUsing(src => Tenant.Create(src.CompanyId, src.Name, EmailAddress.Create(src.ContactEmail)))
            .AfterMapping((src, dest) =>
            {
                if (dest.Id != null)
                {
                    dest.SetCompany(src.CompanyId);
                    dest.SetName(src.Name);
                    dest.SetContactEmail(EmailAddress.Create(src.ContactEmail));
                }

                dest.SetDescription(src.Description);
                MapSubscriptions(src.Subscriptions, dest);
                MapBranding(src.Branding, dest);
            });

        config.ForType<(TenantSubscriptionModel subscriptionModel, Tenant tenant), TenantSubscription>()
            .IgnoreNullValues(true)
            .ConstructUsing(src => TenantSubscription.Create(src.tenant,
                Enumeration.FromId<TenantSubscriptionPlanType>(src.subscriptionModel.PlanType),
                DateSchedule.Create(src.subscriptionModel.Schedule.StartDate, src.subscriptionModel.Schedule.EndDate)))
            .AfterMapping((src, dest) =>
            {
                if (dest.Id != null)
                {
                    dest.SetPlanType(Enumeration.FromId<TenantSubscriptionPlanType>(src.subscriptionModel.PlanType));
                    dest.SetSchedule(DateSchedule.Create(src.subscriptionModel.Schedule.StartDate, src.subscriptionModel.Schedule.EndDate));
                }

                dest.SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(src.subscriptionModel.Status));
                dest.SetBillingCycle(Enumeration.FromId<TenantSubscriptionBillingCycle>(src.subscriptionModel.BillingCycle));
            });

        config.ForType<TenantBrandingModel, TenantBranding>()
            .IgnoreNullValues(true)
            .ConstructUsing(src => TenantBranding.Create(HexColor.Create(src.PrimaryColor), HexColor.Create(src.SecondaryColor), Url.Create(src.LogoUrl), Url.Create(src.FaviconUrl)))
            .AfterMapping((src, dest) =>
            {
                if (dest.Id != null)
                {
                    dest.SetPrimaryColor(HexColor.Create(src.PrimaryColor));
                    dest.SetSecondaryColor(HexColor.Create(src.SecondaryColor));
                    dest.SetLogoUrl(Url.Create(src.LogoUrl));
                    dest.SetFaviconUrl(Url.Create(src.FaviconUrl));
                }

                dest.SetCustomCss(src.CustomCss);
            });
    }

    private static Address MapAddress(AddressModel source)
    {
        if (source == null)
        {
            return null;
        }

        return Address.Create(source.Name, source.Line1, source.Line2, source.PostalCode, source.City, source.Country);
    }

    private static void MapSubscriptions(TenantSubscriptionModel[] sources, Tenant destination)
    {
        var existingSubscriptions = destination.Subscriptions.ToList();
        var newSubscriptionModels = sources ?? [];

        foreach (var existingSubscription in existingSubscriptions)
        {
            if (!newSubscriptionModels.Any(s => s.Id == existingSubscription.Id.Value.ToString()))
            {
                destination.RemoveSubscription(existingSubscription);
            }
        }

        foreach (var subscriptionModel in newSubscriptionModels)
        {
            var existingSubscription = existingSubscriptions.Find(s => s.Id.Value.ToString() == subscriptionModel.Id);
            if (existingSubscription == null)
            {
                var newSubscription = (subscriptionModel, destination).Adapt<TenantSubscription>(); // use destination too (tenant)
                destination.AddSubscription(newSubscription);
            }
            else
            {
                subscriptionModel.Adapt(existingSubscription);
            }
        }
    }

    private static void MapBranding(TenantBrandingModel source, Tenant destination)
    {
        if (source == null)
        {
            destination.SetBranding(null);
            return;
        }

        var branding = destination.Branding ?? TenantBranding.Create();
        source.Adapt(branding);
        destination.SetBranding(branding);
    }
}