// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantCreateCommand(
    TenantModel model) : CommandRequestBase<Result<Tenant>>
{
    public TenantModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<TenantCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model)
                .SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<TenantModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id)
                    .MustBeDefaultOrEmptyGuid()
                    .WithMessage("Must be empty.");
                this.RuleFor(m => m.CompanyId)
                    .MustNotBeDefaultOrEmptyGuid()
                    .WithMessage("Must be valid and not be empty.");
                this.RuleFor(m => m.Name)
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