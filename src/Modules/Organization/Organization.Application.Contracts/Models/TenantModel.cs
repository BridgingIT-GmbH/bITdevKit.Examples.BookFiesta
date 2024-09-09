// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantModel
{
    public string Id { get; set; }

    public string CompanyId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ContactEmail { get; set; }

    public TenantBrandingModel Branding { get; set; }

    public TenantSubscriptionModel[] Subscriptions { get; set; }
}