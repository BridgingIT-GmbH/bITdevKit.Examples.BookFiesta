// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
[TypedEntityId<Guid>]
public class Publisher : AuditableAggregateRoot<PublisherId>, IConcurrent
{
    //private readonly List<PublisherBook> book = [];

    private Publisher() { } // Private constructor required by EF Core

    private Publisher(
        TenantId tenantId,
        string name,
        string description = null,
        EmailAddress contactEmail = null,
        Address address = null,
        Website website = null)
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

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    //public IEnumerable<PublisherBook> Books => this.books;

    public static Publisher Create(
        TenantId tenantId,
        string name,
        string description = null,
        EmailAddress contactEmail = null,
        Address address = null,
        Website website = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var publisher = new Publisher(tenantId, name, description, contactEmail, address, website);

        publisher.DomainEvents.Register(new PublisherCreatedDomainEvent(publisher), true);

        return publisher;
    }

    public Publisher SetName(string name)
    {
        _ = name ?? throw new ArgumentException("Publisher Name cannot be empty.");

        // Validate name
        if (this.Name == name)
        {
            return this;
        }

        this.Name = name;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.Description = description;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetContactEmail(EmailAddress email)
    {
        if (this.ContactEmail == email)
        {
            return this;
        }

        this.ContactEmail = email;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetAddress(Address address)
    {
        // Validate address
        if (this.Address == address)
        {
            return this;
        }

        this.Address = address;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetWebsite(Website website)
    {
        // Validate website
        if (this.Website == website)
        {
            return this;
        }

        this.Website = website;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }
}