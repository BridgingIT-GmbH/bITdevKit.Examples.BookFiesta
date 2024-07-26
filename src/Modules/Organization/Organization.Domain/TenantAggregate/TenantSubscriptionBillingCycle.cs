// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Domain;

public class TenantSubscriptionBillingCycle(int id, string name, string description, bool autoRenew)
    : Enumeration(id, name)
{
    public static TenantSubscriptionBillingCycle Never = new(0, nameof(Never), "Lorem Ipsum", false);
    public static TenantSubscriptionBillingCycle Monthly = new(1, nameof(Monthly), "Lorem Ipsum", true);
    public static TenantSubscriptionBillingCycle Yearly = new(2, nameof(Yearly), "Lorem Ipsum", true);

    public string Description { get; } = description;

    public bool AutoRenew { get; } = autoRenew;

    public static IEnumerable<TenantSubscriptionBillingCycle> GetAll() => GetAll<TenantSubscriptionBillingCycle>();
}