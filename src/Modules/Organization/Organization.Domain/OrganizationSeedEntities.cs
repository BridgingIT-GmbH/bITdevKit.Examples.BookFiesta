// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

using BridgingIT.DevKit.Common;

public static class OrganizationSeedEntities
{
    private static string GetSuffix(long ticks)
    {
        return ticks > 0 ? $"-{ticks}" : string.Empty;
    }

    public static class Companies
    {
        public static Company[] Create(long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Company.Create(
                            $"Acme Corporation{GetSuffix(ticks)}",
                            "AC123456",
                            EmailAddress.Create($"contact{GetSuffix(ticks)}@acme.com"),
                            Address.Create(
                                $"Acme Corporation{GetSuffix(ticks)}",
                                "123 Business Ave",
                                "Suite 100",
                                "90210",
                                "Los Angeles",
                                "USA"))
                        .SetContactPhone(PhoneNumber.Create("+1234567890"))
                        .SetWebsite(Url.Create("https://www.acme.com"))
                        .SetVatNumber(VatNumber.Create("US12-3456789")),
                    Company.Create(
                            $"TechInnovate GmbH{GetSuffix(ticks)}",
                            "HRB987654",
                            EmailAddress.Create($"info{GetSuffix(ticks)}@techinnovate.de"),
                            Address.Create(
                                $"TechInnovate GmbH{GetSuffix(ticks)}",
                                "Innovationsstraße 42",
                                string.Empty,
                                "10115",
                                "Berlin",
                                "Germany"))
                        .SetContactPhone(PhoneNumber.Create("+49301234567"))
                        .SetWebsite(Url.Create("https://www.techinnovate.de"))
                        .SetVatNumber(VatNumber.Create("DE123456789")),
                    Company.Create(
                            $"Global Trade Ltd{GetSuffix(ticks)}",
                            "GTL789012",
                            EmailAddress.Create($"enquiries{GetSuffix(ticks)}@globaltrade.co.uk"),
                            Address.Create(
                                $"Global Trade Ltd{GetSuffix(ticks)}",
                                "1 Commerce Street",
                                "Floor 15",
                                "EC1A 1BB",
                                "London",
                                "United Kingdom"))
                        .SetContactPhone(PhoneNumber.Create("+442071234567"))
                        .SetWebsite(Url.Create("https://www.globaltrade.co.uk"))
                        .SetVatNumber(VatNumber.Create("GB123456789"))
                }.ForEach(e => e.Id = CompanyId.Create($"{GuidGenerator.Create($"Company_{e.Name}")}"))
            ];
        }
    }

    public static class Tenants
    {
        public static Tenant[] Create(Company[] companies, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Tenant.Create(companies[0].Id, $"AcmeBooks{GetSuffix(ticks)}", $"books@acme{GetSuffix(ticks)}.com")
                        .AddSubscription()
                        .SetSchedule(
                            DateSchedule.Create(
                                DateOnly.FromDateTime(new DateTime(2020, 1, 1)),
                                DateOnly.FromDateTime(new DateTime(2022, 12, 31))))
                        .SetPlanType(TenantSubscriptionPlanType.Free)
                        .Tenant.AddSubscription()
                        .SetSchedule(DateSchedule.Create(DateOnly.FromDateTime(new DateTime(2023, 1, 1))))
                        .SetPlanType(TenantSubscriptionPlanType.Basic)
                        .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly)
                        .Tenant.SetBranding(TenantBranding.Create("#000000", "#AAAAAA")),
                    Tenant.Create(
                            companies[0].Id,
                            $"TechBooks{GetSuffix(ticks)}",
                            $"books@techinnovate{GetSuffix(ticks)}.de")
                        .AddSubscription()
                        .SetSchedule(DateSchedule.Create(DateOnly.FromDateTime(new DateTime(2020, 1, 1))))
                        .SetPlanType(TenantSubscriptionPlanType.Premium)
                        .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly)
                        .Tenant.SetBranding(TenantBranding.Create("#000000", "#AAAAAA"))
                }.ForEach(e => e.Id = TenantIdFactory.CreateForName($"Tenant_{e.Name}"))
            ];
        }
    }
}