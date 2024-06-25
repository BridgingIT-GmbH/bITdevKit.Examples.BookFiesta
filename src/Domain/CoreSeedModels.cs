// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using BridgingIT.DevKit.Common;

public static class CoreSeedModels
{
    public static class Customers
    {
        public static Customer[] Create(long ticks = 0) =>
            [.. new[]
            {
                Customer.Create($"John", $"Doe", EmailAddress.Create($"john.doe{ticks}@example.com"), Address.Create("Main Street", string.Empty, "17100", "Anytown", "USA")),
                Customer.Create($"Mary", $"Jane", EmailAddress.Create($"mary.jane{ticks}@example.com"), Address.Create("Maple Street", string.Empty, "17101", "Anytown", "USA"))
            }.ForEach(e => e.Id = AuthorId.Create($"{GuidGenerator.Create($"Customer_{e.FirstName}")}"))];
    }

    public static class Authors
    {
        public static Author[] Create(long ticks = 0) =>
            [.. new[]
            {
                Author.Create(PersonFormalName.Create([$"John", $"Doe"], "Dr.", "Jr."), "Bio"),
                Author.Create(PersonFormalName.Create([$"Mary", $"Jane"]), "Bio")
            }.ForEach(e => e.Id = AuthorId.Create($"{GuidGenerator.Create($"Author_{e.PersonName.Full}")}"))];
    }

    public static class Tags
    {
        public static Tag[] Create(long ticks = 0) =>
            [.. new[]
            {
                Tag.Create($"tagA-{ticks}"),
                Tag.Create($"tagB-{ticks}"),
                Tag.Create($"tagC-{ticks}"),
                Tag.Create($"tagD-{ticks}")
            }.ForEach(e => e.Id = TagId.Create($"{GuidGenerator.Create($"Tag_{e.Name}")}"))];
    }

    public static class Categories
    {
        public static Category[] Create(long ticks = 0) =>
            [.. new[]
            {
                Category.Create($"category1-{ticks}", "Lorem", 0)
                    .AddChild(Category.Create($"category1A-{ticks}", "Lorem", 0)
                        .AddChild(Category.Create($"category1A1-{ticks}", "Lorem", 0)))
                    .AddChild(Category.Create($"category1B-{ticks}", "Lorem", 1)
                        .AddChild(Category.Create($"category1B1-{ticks}", "Lorem", 0))
                        .AddChild(Category.Create($"category1B2-{ticks}", "Lorem", 1))
                        .AddChild(Category.Create($"category1B3-{ticks}", "Lorem", 2))),
                Category.Create($"category2-{ticks}", "Lorem", 1)
                    .AddChild(Category.Create($"category2A-{ticks}", "Lorem", 0)
                        .AddChild(Category.Create($"category2A1-{ticks}", "Lorem", 0)))
                    .AddChild(Category.Create($"category2B-{ticks}", "Lorem", 1)
                        .AddChild(Category.Create($"category2B1-{ticks}", "Lorem", 0))),
                Category.Create($"category3-{ticks}", "Lorem", 2)
            }.ForEach(e => e.Id = CategoryId.Create($"{GuidGenerator.Create($"Category_{e.Title}")}"))];
    }

    public static class Books
    {
        public static Book[] Create(Tag[] tags, Category[] categories, long ticks = 0)
        {
            //var categories = Categories.Create(ticks).ToArray();
            //var tags = Tags.Create(ticks).ToArray();
            return [.. new[]
            {
                Book.Create(
                    $"TitleA-{ticks}",
                    "Lorem",
                    BookIsbn.Create("978-3-16-148410-0"),
                    Money.Create(12.99m))
                        .AddTag(tags[0]).AddTag(tags[1])
                        .AddCategory(categories[0]) // category1
                        .AddCategory(categories[0].Children.ToArray()[0].Children.ToArray()[0])
                        .AddChapter("chap2")
                        .AddChapter("chap3")
                        .AddChapter("chap5")
                        .AddChapter("chap1", 1)
                        .AddChapter("chap4", 4)
                        .AddChapter("AppendixI", 10)
                        .AddChapter("AppendixII"),
                Book.Create(
                    $"TitleB-{ticks}",
                    "Lorem",
                    BookIsbn.Create("978-3-16-148410-1"),
                    Money.Create(24.95m))
                        .AddTag(tags[1]).AddTag(tags[2])
                        .AddCategory(categories[0]) // category1
                        .AddCategory(categories[0].Children.ToArray()[1].Children.ToArray()[0]) // category1B1
                        .AddChapter("chap1"),
                Book.Create(
                    $"TitleC-{ticks}",
                    "Lorem",
                    BookIsbn.Create("978-3-16-148410-2"),
                    Money.Create(19.99m))
                        .AddTag(tags[0]).AddTag(tags[1]).AddTag(tags[2])
                        .AddCategory(categories[1]) // category2
                        .AddCategory(categories[1].Children.ToArray()[1].Children.ToArray()[0]) // category2B1
                        .AddChapter("chap1")
                        .AddChapter("chap2"),
            }.ForEach(e => e.Id = BookId.Create($"{GuidGenerator.Create($"Book_{e.Title}")}"))];
        }
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    private static (string title, (string firstName, string lastName)[] authors, string summary, int publishedYear, decimal price, string category, string publisher)[] Data
#pragma warning restore SA1201 // Elements should appear in the correct order
        => new (string, (string, string)[], string, int, decimal, string, string)[]
        {
            // generated by https://chatgpt.com/share/aec33ce4-9b71-4fcc-b30f-920138f178bc TODO add ISBN 10 & 13, PageCount, Language, etc.
            ("The Pragmatic Programmer: Your Journey to Mastery", new (string, string)[] { ("David", "Thomas"), ("Andrew", "Hunt") }, "A guide to mastering the art of software development, offering practical advice and techniques to improve your programming skills and productivity.", 2019, 49.99m, "Software Development", "Addison-Wesley"),
            ("Clean Architecture: A Craftsman's Guide to Software Structure and Design", new (string, string)[] { ("Robert", "Martin") }, "A guide to designing robust and maintainable software architectures using principles and patterns that promote clean code.", 2017, 43.99m, "Software Architecture", "Prentice Hall"),
            ("Design Patterns: Elements of Reusable Object-Oriented Software", new (string, string)[] { ("Erich", "Gamma"), ("Richard", "Helm"), ("Ralph", "Johnson"), ("John", "Vlissides") }, "A classic book that introduces and explains 23 design patterns to solve common software design problems and improve code reuse and maintainability.", 1994, 55.00m, "Software Design", "Addison-Wesley"),
            ("Code: The Hidden Language of Computer Hardware and Software", new (string, string)[] { ("Charles", "Petzold") }, "An exploration of the inner workings of computers, explaining the fundamental principles of hardware and software through accessible examples.", 1999, 41.99m, "Computer Science", "Microsoft Press"),
            ("The Clean Coder: A Code of Conduct for Professional Programmers", new (string, string)[] { ("Robert", "Martin") }, "A guide to the professional conduct and ethics of software developers, offering advice on communication, accountability, and continuous learning.", 2011, 39.99m, "Software Development", "Prentice Hall"),
            ("Refactoring: Improving the Design of Existing Code", new (string, string)[] { ("Martin", "Fowler") }, "A guide to improving the design of existing code by applying refactoring techniques to make it more maintainable and extensible.", 1999, 47.99m, "Software Design", "Addison-Wesley"),
            ("Clean Code: A Handbook of Agile Software Craftsmanship", new (string, string)[] { ("Robert", "Martin") }, "A deep dive into the principles, patterns, and practices of writing clean code, emphasizing readability, simplicity, and craftsmanship.", 2008, 40.99m, "Software Development", "Prentice Hall"),
            ("The Pragmatic Programmer: From Journeyman to Master", new (string, string)[] { ("Andrew", "Hunt"), ("David", "Thomas") }, "A comprehensive guide for software developers that covers practical advice and techniques to improve your programming skills and productivity.", 1999, 42.99m, "Software Development", "Addison-Wesley"),
            ("Design Patterns: Elements of Reusable Object-Oriented Software", new (string, string)[] { ("Erich", "Gamma"), ("Richard", "Helm"), ("Ralph", "Johnson"), ("John", "Vlissides") }, "A classic book that introduces and explains 23 design patterns to solve common software design problems and improve code reuse and maintainability.", 1994, 55.00m, "Software Design", "Addison-Wesley"),
            ("The C Programming Language", new (string, string)[] { ("Brian", "Kernighan"), ("Dennis", "Ritchie") }, "The definitive guide to C programming, providing a comprehensive introduction to the language and its key concepts.", 1978, 65.00m, "Programming Languages", "Prentice Hall"),
            ("Code Complete: A Practical Handbook of Software Construction", new (string, string)[] { ("Steve", "McConnell") }, "An in-depth guide to software construction best practices, covering coding, debugging, design, and testing techniques to create high-quality software.", 1993, 50.99m, "Software Development", "Microsoft Press"),
            ("Refactoring: Improving the Design of Existing Code", new (string, string)[] { ("Martin", "Fowler") }, "A guide to improving the design of existing code by applying refactoring techniques to make it more maintainable and extensible.", 1999, 47.99m, "Software Design", "Addison-Wesley"),
            ("Cracking the Coding Interview: 189 Programming Questions and Solutions", new (string, string)[] { ("Gayle", "Laakmann McDowell") }, "A comprehensive resource for preparing for technical job interviews, including programming questions and detailed solutions to help you succeed.", 2008, 35.99m, "Career Development", "CareerCup"),
            ("The Clean Architecture: A Craftsman's Guide to Software Structure and Design", new (string, string)[] { ("Robert", "Martin") }, "A guide to designing robust and maintainable software architectures using principles and patterns that promote clean code.", 2017, 43.99m, "Software Architecture", "Prentice Hall"),
            ("Effective Java", new (string, string)[] { ("Joshua", "Bloch") }, "A practical guide to Java programming, offering best practices and expert advice to help you write effective, efficient, and maintainable Java code.", 2001, 45.99m, "Programming Languages", "Addison-Wesley"),
            ("Programming Pearls", new (string, string)[] { ("Jon", "Bentley") }, "A collection of essays that provide insights into programming techniques, problem-solving strategies, and algorithm design.", 1986, 38.99m, "Programming Techniques", "Addison-Wesley"),
            ("Test-Driven Development: By Example", new (string, string)[] { ("Kent", "Beck") }, "An introduction to test-driven development (TDD) through practical examples, demonstrating how writing tests first can improve code quality and design.", 2002, 44.99m, "Software Development", "Addison-Wesley"),
            ("Object-Oriented Software Construction", new (string, string)[] { ("Bertrand", "Meyer") }, "An authoritative book on object-oriented programming, covering concepts, principles, and techniques for creating robust and reusable software.", 1988, 56.99m, "Software Development", "Prentice Hall"),
            ("Domain-Driven Design: Tackling Complexity in the Heart of Software", new (string, string)[] { ("Eric", "Evans") }, "A comprehensive guide to domain-driven design, offering strategies to manage complexity in software development by aligning the software model with business needs.", 2003, 57.99m, "Software Design", "Addison-Wesley"),
            ("Patterns of Enterprise Application Architecture", new (string, string)[] { ("Martin", "Fowler") }, "A catalog of patterns and best practices for designing and building enterprise applications, addressing common challenges and solutions.", 2002, 54.99m, "Software Design", "Addison-Wesley"),
            ("The Mythical Man-Month: Essays on Software Engineering", new (string, string)[] { ("Frederick", "Brooks") }, "A collection of essays on software engineering management, discussing timeless principles and challenges in large-scale software projects.", 1975, 39.99m, "Software Engineering", "Addison-Wesley"),
            ("Code: The Hidden Language of Computer Hardware and Software", new (string, string)[] { ("Charles", "Petzold") }, "An exploration of the inner workings of computers, explaining the fundamental principles of hardware and software through accessible examples.", 1999, 41.99m, "Computer Science", "Microsoft Press"),
            ("The Art of Computer Programming, Volumes 1-4A Boxed Set", new (string, string)[] { ("Donald", "Knuth") }, "A comprehensive and authoritative guide to algorithms and programming, covering fundamental concepts and techniques in computer science.", 1968, 220.00m, "Computer Science", "Addison-Wesley"),
            ("Working Effectively with Legacy Code", new (string, string)[] { ("Michael", "Feathers") }, "A practical guide to maintaining and improving legacy code, offering strategies to make existing codebases more understandable and adaptable.", 2004, 49.99m, "Software Development", "Prentice Hall"),
            ("Software Architecture in Practice", new (string, string)[] { ("Len", "Bass"), ("Paul", "Clements"), ("Rick", "Kazman") }, "A comprehensive guide that explores the role of the software architect, the architecture business cycle, and how to create and evaluate software architectures.", 1998, 59.99m, "Software Architecture", "Addison-Wesley"),
            ("Building Microservices: Designing Fine-Grained Systems", new (string, string)[] { ("Sam", "Newman") }, "An in-depth look at designing, building, and deploying microservices, offering practical advice and real-world examples to help you understand and apply microservices architecture.", 2015, 52.99m, "Software Architecture", "O'Reilly Media"),
            ("Software Architecture Patterns", new (string, string)[] { ("Mark", "Richards") }, "An insightful book that explores five different architecture patterns and their trade-offs, providing practical advice on when and how to use each pattern effectively.", 2015, 49.99m, "Software Architecture", "O'Reilly Media"),
            ("Fundamentals of Software Architecture: An Engineering Approach", new (string, string)[] { ("Mark", "Richards"), ("Neal", "Ford") }, "A comprehensive guide that covers the essential principles and practices of software architecture, including architectural styles, patterns, and methodologies.", 2020, 60.99m, "Software Architecture", "O'Reilly Media"),
            ("Microservices Patterns: With examples in Java", new (string, string)[] { ("Chris", "Richardson") }, "A practical guide to developing microservices, exploring patterns for building reliable and scalable services with real-world examples in Java.", 2018, 48.99m, "Software Architecture", "Manning Publications"),
            ("The Phoenix Project: A Novel About IT, DevOps, and Helping Your Business Win", new (string, string)[] { ("Gene", "Kim"), ("Kevin", "Behr"), ("George", "Spafford") }, "A novel that illustrates the principles of DevOps and IT operations, offering insights into improving IT performance and business outcomes through a compelling story.", 2013, 35.99m, "DevOps", "IT Revolution Press"),
            ("Release It!: Design and Deploy Production-Ready Software", new (string, string)[] { ("Michael", "Nygard") }, "A guide to designing and deploying production-ready software, addressing common challenges in software reliability, scalability, and maintenance.", 2007, 45.99m, "Software Development", "Pragmatic Bookshelf"),
            ("Software Systems Architecture: Working with Stakeholders Using Viewpoints and Perspectives", new (string, string)[] { ("Nick", "Rozanski"), ("Eoin", "Woods") }, "A practical guide to software architecture, emphasizing the importance of stakeholder communication and using viewpoints and perspectives to create effective architecture documentation.", 2005, 49.99m, "Software Architecture", "Addison-Wesley"),
            ("Clean Coder: A Code of Conduct for Professional Programmers", new (string, string)[] { ("Robert", "Martin") }, "A guide to the professional conduct and ethics of software developers, offering advice on communication, accountability, and continuous learning.", 2011, 39.99m, "Software Development", "Prentice Hall"),
        };
}