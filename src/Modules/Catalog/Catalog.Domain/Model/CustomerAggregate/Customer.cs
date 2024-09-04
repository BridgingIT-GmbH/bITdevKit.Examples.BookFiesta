// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;

using System.Xml.Linq;

[TypedEntityId<Guid>]
public class Customer : AuditableAggregateRoot<CustomerId>, IConcurrent
{
    private Customer()
    {
    }

    private Customer(TenantId tenantId, PersonFormalName name, EmailAddress email, Address address = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetEmail(email);
        this.SetAddress(address);
    }

    public TenantId TenantId { get; private set; }

    public PersonFormalName PersonName { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress Email { get; private set; }

    public Guid Version { get; set; }

    public static Customer Create(TenantId tenantId, PersonFormalName name, EmailAddress email, Address address = null)
    {
        var customer = new Customer(tenantId, name, email, address);

        customer.DomainEvents.Register(
            new CustomerCreatedDomainEvent(customer));

        return customer;
    }

    public Customer SetName(PersonFormalName name)
    {
        if (name is null)
        {
            throw new DomainRuleException("Customer name cannot be empty.");
        }

        if (this.PersonName != name)
        {
            this.PersonName = name;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                    new CustomerUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Customer SetEmail(EmailAddress email)
    {
        if (email is null)
        {
            throw new DomainRuleException("Customer email cannot be empty.");
        }

        if (email != this.Email)
        {
            this.Email = email;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                    new CustomerUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Customer SetAddress(Address address)
    {
        if (address != this.Address)
        {
            this.Address = address;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(
                new CustomerUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }
}