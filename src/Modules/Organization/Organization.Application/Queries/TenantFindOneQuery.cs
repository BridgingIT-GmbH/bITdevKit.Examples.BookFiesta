// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindOneQuery(string tenantId) : QueryRequestBase<Result<Tenant>>
{
    public string TenantId { get; } = tenantId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<TenantFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}