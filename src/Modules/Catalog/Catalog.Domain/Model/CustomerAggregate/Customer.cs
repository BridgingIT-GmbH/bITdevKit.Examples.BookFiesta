// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

public class Customer : AuditableAggregateRoot<CustomerId/*, Guid*/>, IConcurrent
{
    private Customer()
    {
    }

    private Customer(string firstName, string lastName, EmailAddress email, Address address = null)
    {
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Email = email;
        this.Address = address;
    }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress Email { get; private set; }

    public Guid Version { get; set; }

    public static Customer Create(string firstName, string lastName, EmailAddress email, Address address = null)
    {
        var customer = new Customer(firstName, lastName, email, address);

        customer.DomainEvents.Register(
            new CustomerCreatedDomainEvent(customer));

        return customer;
    }

    public Customer ChangeName(string firstName, string lastName)
    {
        //if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        //{
        //    return this;
        //}

        this.FirstName = firstName;
        this.LastName = lastName;

        this.DomainEvents.Register(
            new CustomerUpdatedDomainEvent(this), true);

        return this;
    }

    public Customer ChangeAddress(string line1, string line2, string postalCode, string city, string country)
    {
        var address = Address.Create(line1, line2, postalCode, city, country);
        if (this.Address?.Equals(address) == false)
        {
            this.Address = address;

            this.DomainEvents.Register(
                new CustomerUpdatedDomainEvent(this), true);
            this.DomainEvents.Register(
                new CustomerAddressUpdatedDomainEvent(this), true);
        }

        return this;
    }
}