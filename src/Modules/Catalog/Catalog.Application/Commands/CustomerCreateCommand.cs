// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerCreateCommand(
    string tenantId,
    CustomerModel model)
    : CommandRequestBase<Result<Customer>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public CustomerModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model)
                .SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CustomerModel>
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
                this.RuleFor(m => m.PersonName)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.PersonName.Parts)
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
                this.RuleFor(m => m.Email)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.");
            }
        }
    }
}