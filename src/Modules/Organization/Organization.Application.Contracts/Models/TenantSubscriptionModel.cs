﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using SharedKernel.Application;

public class TenantSubscriptionModel
{
    public string Id { get; set; }

    public int PlanType { get; }

    public int Status { get; }

    public DateScheduleModel Schedule { get; }

    public int BillingCycle { get; }
}