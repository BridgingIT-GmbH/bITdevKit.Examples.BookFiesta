// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System;
using System.Diagnostics;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Id={Id}, Name={Name")]
public class Publisher : AuditableAggregateRoot<PublisherId/*, Guid*/>, IConcurrent
{
    //private readonly List<PublisherBook> book = [];

    private Publisher() { } // Private constructor required by EF Core

    private Publisher(string name, string description, Address address = null, EmailAddress email = null, Website website = null)
    {
        this.SetName(name);
        this.SetDescription(description);
        this.SetEmail(email);
        this.SetAddress(address);
        this.SetDescription(website);
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress Email { get; private set; }

    public Website Website { get; private set; }

    public Guid Version { get; set; }

    //public IEnumerable<PublisherBook> Books => this.books;

    public static Publisher Create(string name, string description, Address address = null, EmailAddress email = null, Website website = null)
    {
        var publisher = new Publisher(name, description, address, email, website);

        publisher.DomainEvents.Register(
                new PublisherCreatedDomainEvent(publisher), true);

        return publisher;
    }

    public Publisher SetName(string name)
    {
        // Validate name
        if (!string.IsNullOrEmpty(name) && name != this.Name)
        {
            this.Name = name;
            this.DomainEvents.Register(
                new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetDescription(string description)
    {
        // Validate description
        if (!string.IsNullOrEmpty(description) && description != this.Description)
        {
            this.Description = description;
            this.DomainEvents.Register(
                new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetEmail(EmailAddress email)
    {
        // Validate email
        if (email != null && email != this.Email)
        {
            this.Email = email;
            this.DomainEvents.Register(
                new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetAddress(Address address)
    {
        // Validate address
        if (address != null && address != this.Address)
        {
            this.Address = address;
            this.DomainEvents.Register(
                new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetWebsite(Website website)
    {
        // Validate website
        if (website != null && website != this.Website)
        {
            this.Website = website;
            this.DomainEvents.Register(
                new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }
}