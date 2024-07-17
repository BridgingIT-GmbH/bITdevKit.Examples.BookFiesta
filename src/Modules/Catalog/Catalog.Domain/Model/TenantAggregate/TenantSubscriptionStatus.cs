// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class TenantSubscriptionStatus(int id, string name, string description) : Enumeration(id, name)
{
    public static TenantSubscriptionStatus Pending = new(1, nameof(Pending), "Lorem Ipsum");
    public static TenantSubscriptionStatus Approved = new(2, nameof(Approved), "Lorem Ipsum");
    public static TenantSubscriptionStatus Cancelled = new(3, nameof(Cancelled), "Lorem Ipsum");
    public static TenantSubscriptionStatus Ended = new(4, nameof(Ended), "Lorem Ipsum");

    public string Description { get; } = description;

    public static IEnumerable<TenantSubscriptionStatus> GetAll() => GetAll<TenantSubscriptionStatus>();
}
