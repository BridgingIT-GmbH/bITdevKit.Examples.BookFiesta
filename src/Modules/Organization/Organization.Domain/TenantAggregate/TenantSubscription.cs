// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

/// <summary>
/// Represents the commercial agreements for the tenant.
/// </summary>
[DebuggerDisplay("Id={Id}, TenantId={Tenant?.Id}, Status={Status}")]
[TypedEntityId<Guid>]
public class TenantSubscription : Entity<TenantSubscriptionId>, IConcurrent
{
    private TenantSubscription() { } // Private constructor required by EF Core

    private TenantSubscription(Tenant tenant, TenantSubscriptionPlanType planType, DateSchedule schedule)
    {
        this.Tenant = tenant;
        this.SetPlanType(planType);
        this.SetStatus(TenantSubscriptionStatus.Pending);
        this.SetSchedule(schedule);
    }

    public Tenant Tenant { get; private set; }

    public TenantSubscriptionPlanType PlanType { get; private set; }

    public TenantSubscriptionStatus Status { get; private set; }

    public DateSchedule Schedule { get; private set; }

    public TenantSubscriptionBillingCycle BillingCycle { get; private set; }

    public Guid Version { get; set; }

    public static TenantSubscription Create(
        Tenant tenant,
        TenantSubscriptionPlanType planType,
        DateSchedule schedule)
    {
        var subscription = new TenantSubscription(tenant, planType, schedule);

        tenant.DomainEvents.Register(
                new TenantSubscriptionCreatedDomainEvent(subscription), true);

        return subscription;
    }

    public bool IsActive(DateOnly date) =>
        this.Status == TenantSubscriptionStatus.Approved && this.Schedule.IsActive(date);

    public TenantSubscription SetPlanType(TenantSubscriptionPlanType planType)
    {
        if (planType == null)
        {
            throw new DomainRuleException("Plan type cannot be null.");
        }

        if (planType != this.PlanType)
        {
            this.PlanType = planType;

            // Set default billing cycle for free plans
            var plan = Enumeration.FromId<TenantSubscriptionPlanType>(this.PlanType.Id);
            if (!planType.IsPaid)
            {
                this.SetBillingCycle(TenantSubscriptionBillingCycle.Never);
            }

            // Set default billing cycle for paid plans
            if (planType.IsPaid &&
                this.BillingCycle == TenantSubscriptionBillingCycle.Never)
            {
                this.SetBillingCycle(TenantSubscriptionBillingCycle.Monthly);
            }

            this.Tenant.DomainEvents.Register(
                new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetSchedule(DateSchedule schedule)
    {
        if (schedule == null)
        {
            throw new DomainRuleException("Schedule cannot be null.");
        }

        if (schedule != this.Schedule)
        {
            this.Schedule = schedule;

            this.Tenant.DomainEvents.Register(
                new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetBillingCycle(TenantSubscriptionBillingCycle billingCycle)
    {
        if (billingCycle != this.BillingCycle)
        {
            if (this.PlanType.IsPaid && billingCycle == TenantSubscriptionBillingCycle.Never)
            {
                throw new DomainRuleException("Subscription billing cycle should not be 'never' for paid plans.");
            }

            this.BillingCycle = billingCycle ?? TenantSubscriptionBillingCycle.Monthly;

            this.Tenant.DomainEvents.Register(
                new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetStatus(TenantSubscriptionStatus status)
    {
        // Validate name
        if (status != this.Status)
        {
            // TODO: check valid transitions
            this.Status = status;

            this.Tenant.DomainEvents.Register(
                new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }
}
