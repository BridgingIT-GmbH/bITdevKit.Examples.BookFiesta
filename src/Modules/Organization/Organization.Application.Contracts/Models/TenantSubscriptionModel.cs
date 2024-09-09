// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class TenantSubscriptionModel
{
    public string Id { get; set; }

    public int PlanType { get; private set; }

    public int Status { get; private set; }

    public DateScheduleModel Schedule { get; private set; }

    public int BillingCycle { get; private set; }
}
