// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using FluentValidation;
using FluentValidation.Results;

public class CustomerDeleteCommand(string tenantId, string id)
    : CommandRequestBase<Result>
{
    public string TenantId { get; } = tenantId;

    public string Id { get; } = id;

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<CustomerDeleteCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}