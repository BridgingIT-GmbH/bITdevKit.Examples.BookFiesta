// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Name={Name")]
[TypedEntityId<Guid>]
public class Publisher : AuditableAggregateRoot<PublisherId>, IConcurrent
{
    //private readonly List<PublisherBook> book = [];

    private Publisher() { } // Private constructor required by EF Core

    private Publisher(TenantId tenantId, string name, string description, EmailAddress contactEmail = null, Address address = null, Website website = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetDescription(description);
        this.SetContactEmail(contactEmail);
        this.SetAddress(address);
        this.SetDescription(website);
    }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public Website Website { get; private set; }

    public Guid Version { get; set; }

    //public IEnumerable<PublisherBook> Books => this.books;

    public static Publisher Create(TenantId tenantId, string name, string description, EmailAddress contactEmail = null, Address address = null, Website website = null)
    {
        var publisher = new Publisher(tenantId, name, description, contactEmail, address, website);

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

    public Publisher SetContactEmail(EmailAddress email)
    {
        if (email != this.ContactEmail)
        {
            this.ContactEmail = email;
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