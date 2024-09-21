﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Queries;
using Domain;
using FluentValidation;
using FluentValidation.Results;

public class CategoryFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Category>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string CategoryId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CategoryFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.CategoryId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}