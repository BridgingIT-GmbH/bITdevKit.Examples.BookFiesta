// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Application.Commands;
using Common;
using Domain;
using FluentValidation;
using FluentValidation.Results;

public class CompanyUpdateCommand(CompanyModel model) : CommandRequestBase<Result<Company>>
{
    public CompanyModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CompanyUpdateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model)
                .SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CompanyModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id)
                    .MustNotBeDefaultOrEmptyGuid()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.Name)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.RegistrationNumber)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.ContactEmail)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
            }
        }
    }
}