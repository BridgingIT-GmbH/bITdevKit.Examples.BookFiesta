// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Title={Title}, Order={Order}, ParentId={Parent?.Id}")]
[TypedEntityId<Guid>]
public class
    Category : AuditableEntity<CategoryId>, IConcurrent // TODO: make this an aggregate root?
{
    private readonly List<Book> books = [];
    private readonly List<Category> children = [];

    private Category() { } // Private constructor required by EF Core

    private Category(
        TenantId tenantId,
        string title,
        string description = null,
        int order = 0,
        Category parent = null)
    {
        this.TenantId = tenantId;
        this.SetTitle(title);
        this.SetDescription(description);
        this.SetParent(parent);
        this.Order = order;
    }

    public TenantId TenantId { get; private set; }

    public string Title { get; private set; }

    public int Order { get; private set; }

    public string Description { get; private set; }

    public Category Parent { get; private set; }

    public IEnumerable<Book> Books
        => this.books.OrderBy(e => e.Title);

    public IEnumerable<Category> Children
        => this.children.OrderBy(e => e.Order)
            .ThenBy(e => e.Title);

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Category Create(
        TenantId tenantId,
        string title,
        string description = null,
        int order = 0,
        Category parent = null)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");

        var category = new Category(tenantId, title, description, order, parent);

        // category.DomainEvents.Register(
        //     new CategoryCreatedDomainEvent(category));

        return category;
    }

    public Category SetTitle(string title)
    {
        _ = title ?? throw new DomainRuleException("Category Title cannot be empty.");

        if (this.Title == title)
        {
            return this;
        }

        this.Title = title;

        // if (this.Id?.IsEmpty == false)
        // {
        //     this.DomainEvents.Register(
        //         new CategoryUpdatedDomainEvent(this), true);
        // }

        return this;
    }

    public Category SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.Description = description;

        // if (this.Id?.IsEmpty == false)
        // {
        //     this.DomainEvents.Register(
        //         new CategoryUpdatedDomainEvent(this), true);
        // }

        return this;
    }

    public Category AddBook(Book book)
    {
        _ = book ?? throw new DomainRuleException("Category Book cannot be empty.");

        if (this.books.Contains(book))
        {
            return this;
        }

        this.books.Add(book);

        return this;
    }

    public Category RemoveBook(Book book)
    {
        _ = book ?? throw new DomainRuleException("Category Book cannot be empty.");

        this.books.Remove(book);

        return this;
    }

    public Category AddChild(Category category)
    {
        _ = category ?? throw new DomainRuleException("Category cannot be empty.");

        if (this.children.Contains(category))
        {
            return this;
        }

        this.children.Add(category);
        category.SetParent(this);

        return this;
    }

    public Category RemoveChild(Category category)
    {
        _ = category ?? throw new DomainRuleException("Category cannot be empty.");

        if (!this.children.Contains(category))
        {
            return this;
        }

        this.children.Remove(category);
        category.RemoveParent();

        return this;
    }

    private void SetParent(Category parent)
    {
        this.Parent = parent;

        if (parent != null)
        {
            //this.ParentId = CategoryId.Create(parent.Id.Value);
        }
    }

    private void RemoveParent()
    {
        this.Parent = null;
    }
}