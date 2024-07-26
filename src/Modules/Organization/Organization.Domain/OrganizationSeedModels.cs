// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Domain;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;

public static class OrganizationSeedModels
{
    private static string GetSuffix(long ticks) => ticks > 0 ? $"-{GetSuffix(ticks)}" : string.Empty;

    public static class Companies
    {
        public static Company[] Create(long ticks = 0) =>
            [.. new[]
            {
                Company.Create(
                    name: $"Acme Corporation{GetSuffix(ticks)}",
                    registrationNumber: "AC123456",
                    contactEmail: EmailAddress.Create($"contact{GetSuffix(ticks)}@acme.com"),
                    address: Address.Create($"Acme Corporation{GetSuffix(ticks)}", "123 Business Ave", "Suite 100", "90210", "Los Angeles", "USA"))
                    .SetContactPhone(PhoneNumber.Create("+1234567890"))
                    .SetWebsite(Url.Create("https://www.acme.com"))
                    .SetVatNumber(VatNumber.Create("US12-3456789")),

                Company.Create(
                    name: $"TechInnovate GmbH{GetSuffix(ticks)}",
                    registrationNumber: "HRB987654",
                    contactEmail: EmailAddress.Create($"info{GetSuffix(ticks)}@techinnovate.de"),
                    address: Address.Create($"TechInnovate GmbH{GetSuffix(ticks)}", "Innovationsstraße 42", string.Empty, "10115", "Berlin", "Germany"))
                    .SetContactPhone(PhoneNumber.Create("+49301234567"))
                    .SetWebsite(Url.Create("https://www.techinnovate.de"))
                    .SetVatNumber(VatNumber.Create("DE123456789")),

                Company.Create(
                    name: $"Global Trade Ltd{GetSuffix(ticks)}",
                    registrationNumber: "GTL789012",
                    contactEmail: EmailAddress.Create($"enquiries{GetSuffix(ticks)}@globaltrade.co.uk"),
                    address: Address.Create($"Global Trade Ltd{GetSuffix(ticks)}", "1 Commerce Street", "Floor 15", "EC1A 1BB", "London", "United Kingdom"))
                    .SetContactPhone(PhoneNumber.Create("+442071234567"))
                    .SetWebsite(Url.Create("https://www.globaltrade.co.uk"))
                    .SetVatNumber(VatNumber.Create("GB123456789"))
            }.ForEach(e => e.Id = CompanyId.Create($"{GuidGenerator.Create($"Company_{e.Name}")}"))];
    }

    public static class Tenants
    {
        public static Tenant[] Create(Company[] companies, long ticks = 0) =>
            [.. new[]
            {
                Tenant.Create(
                    companies[0],
                    $"AcmeBooks{GetSuffix(ticks)}",
                    $"books@acme{GetSuffix(ticks)}.com")
                .AddSubscription()
                    .SetSchedule(Schedule.Create(
                        DateOnly.FromDateTime(new DateTime(2020, 1, 1)),
                        DateOnly.FromDateTime(new DateTime(2022, 12, 31))))
                    .SetPlanType(TenantSubscriptionPlanType.Free).Tenant
                .AddSubscription()
                    .SetSchedule(Schedule.Create(
                        DateOnly.FromDateTime(new DateTime(2023, 1, 1))))
                    .SetPlanType(TenantSubscriptionPlanType.Basic)
                    .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly).Tenant
                .SetBranding(TenantBranding.Create("#000000", "#AAAAAA")),

                Tenant.Create(
                    companies[0],
                    $"TechBooks{GetSuffix(ticks)}",
                    $"books@techinnovate{GetSuffix(ticks)}.de")
                .AddSubscription()
                    .SetSchedule(Schedule.Create(
                        DateOnly.FromDateTime(new DateTime(2020, 1, 1))))
                    .SetPlanType(TenantSubscriptionPlanType.Premium)
                    .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly).Tenant
                .SetBranding(TenantBranding.Create("#000000", "#AAAAAA"))
            }.ForEach(e => e.Id = TenantIdFactory.CreateForName($"Tenant_{e.Name}"))];
    }
}