// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Order={Order}, Title={Title}, ParentId={Parent?.Id}")]
public class Category : AuditableEntity<CategoryId>, IConcurrent // TODO: make this an aggregate root?
{
    private readonly List<Book> books = [];
    private readonly List<Category> children = [];

    private Category() { } // Private constructor required by EF Core

    private Category(string title, string description = null, int order = 0, Category parent = null)
    {
        this.SetTitle(title);
        this.SetDescription(description);
        this.SetParent(parent);
        this.Order = order;
    }

    public string Title { get; private set; }

    public int Order { get; private set; }

    public string Description { get; private set; }

    public Category Parent { get; private set; }

    public IEnumerable<Book> Books => this.books.OrderBy(e => e.Title);

    public IEnumerable<Category> Children => this.children.OrderBy(e => e.Order).ThenBy(e => e.Title);

    public Guid Version { get; set; }

    public static Category Create(string title, string description = null, int order = 0, Category parent = null)
    {
        return new Category(title, description, order, parent);
    }

    public Category SetTitle(string title)
    {
        // Validate title
        this.Title = title;
        return this;
    }

    public Category SetDescription(string description)
    {
        // Validate content
        this.Description = description;
        return this;
    }

    public Category AddBook(Book book)
    {
        if (!this.books.Contains(book))
        {
            this.books.Add(book);
        }

        return this;
    }

    public Category RemoveBook(Book book)
    {
        this.books.Remove(book);

        return this;
    }

    public Category AddChild(Category child)
    {
        if (!this.children.Contains(child))
        {
            this.children.Add(child);
            child.SetParent(this);
        }

        return this;
    }

    public Category RemoveChild(Category child)
    {
        if (this.children.Contains(child))
        {
            this.children.Remove(child);
            child.RemoveParent();
        }

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