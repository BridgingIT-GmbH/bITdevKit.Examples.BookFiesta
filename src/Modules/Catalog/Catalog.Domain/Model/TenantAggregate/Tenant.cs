// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

/// <summary>
/// Represents the client organization or individual using the shop platform.
/// </summary>
public class Tenant : AuditableAggregateRoot<TenantId>, IConcurrent
{
    private List<TenantSubscription> subscriptions = [];

    private Tenant() { } // Private constructor required by EF Core

    private Tenant(Company company, string name, EmailAddress contactEmail)
    {
        this.SetCompany(company);
        this.SetName(name);
        this.SetContactEmail(contactEmail);
    }

    public CompanyId CompanyId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public IEnumerable<TenantSubscription> Subscriptions => this.subscriptions; //.OrderBy(e => e.StartDate);

    public TenantBranding Branding { get; private set; }

    public Guid Version { get; set; }

    public static Tenant Create(Company company, string name, EmailAddress contactEmail)
    {
        var tenant = new Tenant(company, name, contactEmail);

        tenant.DomainEvents.Register(
                new TenantCreatedDomainEvent(tenant), true);

        return tenant;
    }

    public bool IsActive(DateOnly date) =>
        this.subscriptions.Any(e => e.IsActive(date));

    public Tenant SetCompany(Company company)
    {
        if (company == null)
        {
            throw new BusinessRuleNotSatisfiedException("Tenant company cannot be null.");
        }

        if (company.Id != this.CompanyId)
        {
            this.CompanyId = company.Id;

            this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);

            this.DomainEvents.Register(
                new TenantCompanyAssignedDomainEvent(this.Id, company), true);
        }

        return this;
    }

    public Tenant SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleNotSatisfiedException("Tenant name cannot be empty.");
        }

        if (name != this.Name)
        {
            this.Name = name;

            this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Tenant SetDescription(string description)
    {
        if (description != this.Name)
        {
            this.Description = description;

            this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Tenant SetContactEmail(EmailAddress contactEmail)
    {
        if (contactEmail != this.ContactEmail)
        {
            this.ContactEmail = contactEmail;

            this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Tenant SetBranding(TenantBranding branding)
    {
        if (branding.TenantId != null && branding.TenantId != this.Id)
        {
            throw new InvalidOperationException("Branding does not belong to this tenant.");
        }

        this.Branding = branding;

        this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);

        return this;
    }

    public TenantSubscription AddSubscription()
    {
        var subscription = TenantSubscription.Create(
            this, TenantSubscriptionPlanType.Free, Schedule.Create(DateOnly.FromDateTime(DateTime.Now)));

        this.subscriptions.Add(subscription);

        this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);

        return subscription;
    }

    public Tenant AddSubscription(TenantSubscription subscription)
    {
        if (subscription.Tenant != null && subscription.Tenant != this)
        {
            throw new InvalidOperationException("Subscription does not belong to this tenant.");
        }

        this.subscriptions.Add(subscription);

        this.DomainEvents.Register(
                new TenantUpdatedDomainEvent(this), true);

        return this;
    }

    public Tenant RemoveSubscription(TenantSubscription subscription)
    {
        if (subscription.Tenant != this)
        {
            throw new InvalidOperationException("Subscription does not belong to this tenant.");
        }

        if (!this.subscriptions.Contains(subscription))
        {
            this.subscriptions.Remove(subscription);

            this.DomainEvents
                .Register(new TenantUpdatedDomainEvent(this), true)
                .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
        }

        return this;
    }

    public Tenant RemoveSubscription(TenantSubscriptionId id)
    {
        var subscription = this.subscriptions.Find(c => c.Id == id);
        if (subscription != null)
        {
            this.subscriptions.Remove(subscription);

            this.DomainEvents
                    .Register(new TenantUpdatedDomainEvent(this), true)
                    .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
        }

        return this;
    }
}