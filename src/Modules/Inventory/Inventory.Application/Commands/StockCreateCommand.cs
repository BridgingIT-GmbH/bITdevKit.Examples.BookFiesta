// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockCreateCommand(string tenantId, StockModel model) : CommandRequestBase<Result<Stock>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public StockModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<StockModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
            }
        }
    }
}