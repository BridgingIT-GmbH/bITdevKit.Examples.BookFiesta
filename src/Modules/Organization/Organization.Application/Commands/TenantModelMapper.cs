//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BridgingIT.DevKit.Domain.Model;
//using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

//public static class TenantModelMapper
//{
//    public static Tenant Map(TenantModel source, Tenant destination = null)
//    {
//        destination ??= Tenant.Create(
//            source.CompanyId,
//            source.Name,
//            EmailAddress.Create(source.ContactEmail));

//        if (destination.Id != null)
//        {
//            destination.SetCompany(source.CompanyId)
//                       .SetName(source.Name)
//                       .SetContactEmail(EmailAddress.Create(source.ContactEmail));
//        }

//        destination.SetDescription(source.Description);
//        MapSubscriptions(source, destination);
//        MapBranding(source, destination);

//        return destination;
//    }

//    private static void MapSubscriptions(TenantModel source, Tenant destination)
//    {
//        var sourceSubscriptionModels = (source.Subscriptions ?? []).ToDictionary(s => s.Id, s => s);
//        var destinationSubscriptions = destination.Subscriptions.ToDictionary(s => s.Id.Value.ToString(), s => s);

//        foreach (var existingId in destinationSubscriptions.Keys.Except(sourceSubscriptionModels.Keys))
//        {
//            destination.RemoveSubscription(destinationSubscriptions[existingId]);
//        }

//        foreach (var (id, sourceSubscriptionModel) in sourceSubscriptionModels)
//        {
//            if (destinationSubscriptions.TryGetValue(id, out var destinationSubscription))
//            {
//                destinationSubscription
//                    .SetPlanType(Enumeration.FromId<TenantSubscriptionPlanType>(sourceSubscriptionModel.PlanType))
//                    .SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(sourceSubscriptionModel.Status))
//                    .SetSchedule(DateSchedule.Create(sourceSubscriptionModel.Schedule.StartDate, sourceSubscriptionModel.Schedule.EndDate))
//                    .SetBillingCycle(Enumeration.FromId<TenantSubscriptionBillingCycle>(sourceSubscriptionModel.BillingCycle));
//            }
//            else
//            {
//                destination.AddSubscription(
//                    CreateSubscription(sourceSubscriptionModel, destination));
//            }
//        }
//    }

//    private static TenantSubscription CreateSubscription(TenantSubscriptionModel source, Tenant destination)
//    {
//        return TenantSubscription.Create(
//                destination,
//                Enumeration.FromId<TenantSubscriptionPlanType>(source.PlanType),
//                DateSchedule.Create(source.Schedule.StartDate, source.Schedule.EndDate))
//            .SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(source.Status))
//            .SetBillingCycle(Enumeration.FromId<TenantSubscriptionBillingCycle>(source.BillingCycle));
//    }

//    private static void MapBranding(TenantModel source, Tenant destination)
//    {
//        var brandingModel = source.Branding;
//        if (brandingModel == null)
//        {
//            destination.SetBranding(null);
//            return;
//        }

//        var branding = destination.Branding ?? TenantBranding.Create();
//        destination.SetBranding(branding);

//        branding.SetPrimaryColor(HexColor.Create(brandingModel.PrimaryColor))
//            .SetSecondaryColor(HexColor.Create(brandingModel.SecondaryColor))
//            .SetLogoUrl(Url.Create(brandingModel.LogoUrl))
//            .SetFaviconUrl(Url.Create(brandingModel.FaviconUrl))
//            .SetCustomCss(brandingModel.CustomCss);
//    }
//}