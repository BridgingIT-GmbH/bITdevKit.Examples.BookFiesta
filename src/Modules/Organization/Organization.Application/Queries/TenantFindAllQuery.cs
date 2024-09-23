// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindAllQuery : QueryRequestBase<Result<IEnumerable<Tenant>>>
{
    public string CompanyId { get; set; }

    public class Validator : AbstractValidator<TenantFindAllQuery> { }
}