// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Application.Queries;
using Common;
using Domain;
using FluentValidation;
using FluentValidation.Results;

public class CustomerFindOneQuery(string tenantId, string customerId) : QueryRequestBase<Result<Customer>>
{
    public string TenantId { get; } = tenantId;

    public string CustomerId { get; } = customerId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.CustomerId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}