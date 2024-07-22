// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class TenantSubscriptionPlanType(int id, string name, string code, string description, bool isPaid)
    : Enumeration(id, name)
{
    public static TenantSubscriptionPlanType Free = new(0, nameof(Free), "FRE", "Lorem Ipsum", false);
    public static TenantSubscriptionPlanType Basic = new(1, nameof(Basic), "BAS", "Lorem Ipsum", true);
    public static TenantSubscriptionPlanType Premium = new(2, nameof(Premium), "PRM", "Lorem Ipsum", true);

    public string Code { get; } = code;

    public string Description { get; } = description;

    public bool IsPaid { get; } = isPaid;

    public static IEnumerable<TenantSubscriptionPlanType> GetAll() => GetAll<TenantSubscriptionPlanType>();
}