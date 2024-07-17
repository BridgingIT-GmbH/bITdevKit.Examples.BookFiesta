// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

/// <summary>
/// Represents the commercial agreements for the tenant.
/// </summary>
public class TenantSubscription : AuditableEntity<TenantSubscriptionId>, IConcurrent
{
    private TenantSubscription() { } // Private constructor required by EF Core

    private TenantSubscription(Tenant tenant, TenantSubscriptionPlanType planType, Schedule schedule)
    {
        this.Tenant = tenant;
        this.SetPlanType(planType);
        this.SetSchedule(schedule);
    }

    public Tenant Tenant { get; private set; }

    public TenantSubscriptionPlanType PlanType { get; private set; }

    public TenantSubscriptionStatus Status { get; private set; }

    public Schedule Schedule { get; private set; }

    public TenantSubscriptionBillingCycle BillingCycle { get; private set; }

    public Guid Version { get; set; }

    public static TenantSubscription Create(
        Tenant tenant,
        TenantSubscriptionPlanType planType,
        Schedule schedule)
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
        if (planType != this.PlanType)
        {
            this.PlanType = planType;

            // Set default billing cycle for free plans
            var plan = Enumeration.From<TenantSubscriptionPlanType>(this.PlanType.Id);
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

    public TenantSubscription SetSchedule(Schedule schedule)
    {
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
                throw new BusinessRuleNotSatisfiedException("Subscription billing cycle should not be 'never' for paid plans.");
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
