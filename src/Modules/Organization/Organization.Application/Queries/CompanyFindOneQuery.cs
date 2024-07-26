// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using FluentValidation;
using FluentValidation.Results;

public class CompanyFindOneQuery(string companyId) : QueryRequestBase<Result<Company>>
{
    public string CompanyId { get; } = companyId;

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<CompanyFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.CompanyId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}