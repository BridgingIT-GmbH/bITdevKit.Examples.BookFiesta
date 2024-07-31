// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
public class Company : AuditableAggregateRoot<CompanyId>, IConcurrent
{
    private readonly List<TenantId> tenantIds = new();

    private Company() { } // Private constructor required by EF Core

    private Company(string name, string registrationNumber, EmailAddress contactEmail, Address address = null)
    {
        this.SetName(name);
        this.SetContactEmail(contactEmail);
        this.SetRegistrationNumber(registrationNumber);
        this.SetAddress(address);
    }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public string RegistrationNumber { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public PhoneNumber ContactPhone { get; private set; }

    public Url Website { get; private set; }

    public VatNumber VatNumber { get; private set; }

    //public IReadOnlyCollection<TenantId> TenantIds => this.tenantIds.AsReadOnly(); // TODO

    public Guid Version { get; set; }

    public static Company Create(string name, string registrationNumber, EmailAddress contactEmail, Address address = null)
    {
        var company = new Company(name, registrationNumber, contactEmail, address);

        company.DomainEvents.Register(
            new CompanyCreatedDomainEvent(company), true);

        return company;
    }

    public Company SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleNotSatisfiedException("Company name cannot be empty.");
        }

        if (name != this.Name)
        {
            this.Name = name;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetAddress(Address address)
    {
        if (address != this.Address)
        {
            this.Address = address;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetRegistrationNumber(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new BusinessRuleNotSatisfiedException("Company registration number cannot be empty.");
        }

        if (registrationNumber != this.RegistrationNumber)
        {
            this.RegistrationNumber = registrationNumber;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetContactEmail(EmailAddress email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessRuleNotSatisfiedException("Company contact email cannot be empty.");
        }

        if (email != this.ContactEmail)
        {
            this.ContactEmail = email;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetContactPhone(PhoneNumber phone)
    {
        if (phone != this.ContactPhone)
        {
            this.ContactPhone = phone;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetWebsite(Url website)
    {
        if (website != this.Website)
        {
            this.Website = website;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetVatNumber(VatNumber vatNumber)
    {
        if (vatNumber != this.VatNumber)
        {
            this.VatNumber = vatNumber;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company AddTenant(TenantId tenantId)
    {
        if (!this.tenantIds.Contains(tenantId))
        {
            this.tenantIds.Add(tenantId);

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company RemoveTenant(TenantId tenantId)
    {
        if (this.tenantIds.Remove(tenantId))
        {
            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }
}