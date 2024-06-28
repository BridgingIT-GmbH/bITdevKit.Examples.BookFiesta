﻿namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

using FluentValidation;

public class CatalogModuleConfiguration
{
    public IReadOnlyDictionary<string, string> ConnectionStrings { get; set; }

    public string SeederTaskStartupDelay { get; set; } = "00:00:05";

    public class Validator : AbstractValidator<CatalogModuleConfiguration>
    {
        public Validator()
        {
            this.RuleFor(c => c.ConnectionStrings)
                .NotNull().NotEmpty()
                .Must(c => c.ContainsKey("Default"))
                .WithMessage("Connection string with name 'Default' is required");

            this.RuleFor(c => c.SeederTaskStartupDelay)
                .NotNull().NotEmpty()
                .WithMessage("SeederTaskStartupDelay cannot be null or empty");
        }
    }
}