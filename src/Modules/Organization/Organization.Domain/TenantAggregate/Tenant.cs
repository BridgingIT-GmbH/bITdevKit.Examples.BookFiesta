// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

/// <summary>
///     Represents the client organization or individual using the shop platform.
/// </summary>
[DebuggerDisplay("Id={Id}, Name={Name}")]
public class Tenant : AuditableAggregateRoot<TenantId>, IConcurrent
{
    private readonly List<TenantSubscription> subscriptions = [];

    private Tenant() { } // Private constructor required by EF Core

    private Tenant(CompanyId companyId, string name, EmailAddress contactEmail)
    {
        this.SetCompany(companyId);
        this.SetName(name);
        this.SetContactEmail(contactEmail);
        this.Activate();
    }

    public CompanyId CompanyId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public bool Activated { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public IEnumerable<TenantSubscription> Subscriptions
        => this.subscriptions.OrderBy(e => e.Schedule);

    public TenantBranding Branding { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Tenant Create(CompanyId companyId, string name, EmailAddress contactEmail)
    {
        _ = companyId ?? throw new DomainRuleException("Tenant CompanyId cannot be empty.");

        var tenant = new Tenant(companyId, name, contactEmail);

        tenant.DomainEvents.Register(new TenantCreatedDomainEvent(tenant), true);

        return tenant;
    }

    public bool IsActive()
    {
        return this.IsActive(DateOnly.FromDateTime(DateTime.Now));
    }

    public bool IsActive(DateOnly date)
    {
        return this.Activated && this.subscriptions.Any(e => e.IsActive(date));
    }

    public Tenant SetCompany(CompanyId companyId)
    {
        _ = companyId ?? throw new DomainRuleException("Tenant CompanyId cannot be empty.");

        if (companyId != this.CompanyId)
        {
            this.CompanyId = companyId;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
                this.DomainEvents.Register(new TenantReassignedCompanyDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetName(string name)
    {
        _ = name ?? throw new DomainRuleException("Tenant Name cannot be empty.");

        if (name != this.Name)
        {
            this.Name = name;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetDescription(string description)
    {
        if (description != this.Name)
        {
            this.Description = description;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant Deactivate()
    {
        if (this.Activated)
        {
            this.Activated = false;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantDeactivatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant Activate()
    {
        if (!this.Activated)
        {
            this.Activated = true;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantActivatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetContactEmail(EmailAddress email)
    {
        _ = email ?? throw new DomainRuleException("Tenant ContactEmail cannot be empty.");

        if (email != this.ContactEmail)
        {
            this.ContactEmail = email;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetBranding(TenantBranding branding)
    {
        _ = branding ?? throw new DomainRuleException("Tenant Branding cannot be empty.");

        if (branding.TenantId != null && branding.TenantId != this.Id)
        {
            throw new InvalidOperationException("Branding does not belong to this tenant.");
        }

        this.Branding = branding;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription AddSubscription()
    {
        var subscription = TenantSubscription.Create(
            this,
            TenantSubscriptionPlanType.Free,
            DateSchedule.Create(DateOnly.FromDateTime(DateTime.Now)));

        this.subscriptions.Add(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

        return subscription;
    }

    public Tenant AddSubscription(TenantSubscription subscription)
    {
        if (subscription.Tenant != null && subscription.Tenant != this)
        {
            throw new InvalidOperationException("Subscription does not belong to this tenant.");
        }

        this.subscriptions.Add(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

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

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true)
                    .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
            }
        }

        return this;
    }

    public Tenant RemoveSubscription(TenantSubscriptionId id)
    {
        var subscription = this.subscriptions.Find(c => c.Id == id);
        if (subscription == null)
        {
            return this;
        }

        this.subscriptions.Remove(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true)
                .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
        }

        return this;
    }
}