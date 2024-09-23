// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerDeleteCommand(string tenantId, string id) : CommandRequestBase<Result<Customer>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string Id { get; } = id;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerDeleteCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}