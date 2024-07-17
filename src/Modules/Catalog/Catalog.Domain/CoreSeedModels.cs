// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;

public static class CoreSeedModels
{
    private static string GetSuffix(long ticks) => ticks > 0 ? $"-{GetSuffix(ticks)}" : string.Empty;

    private static string GetIsbn(long ticks, string isbn) => ticks > 0 ? $"978-{new Random().NextInt64(1000000000000, 9999999999999)}" : isbn;

    public static class Companies
    {
        public static Company[] Create(long ticks = 0) =>
            [.. new[]
            {
                Company.Create(
                    name: $"Acme Corporation{GetSuffix(ticks)}",
                    registrationNumber: "AC123456",
                    contactEmail: EmailAddress.Create($"contact{GetSuffix(ticks)}@acme.com"),
                    address: Address.Create("123 Business Ave", "Suite 100", "90210", "Los Angeles", "USA"))
                    .SetContactPhone(PhoneNumber.Create("+1234567890"))
                    .SetWebsite(Url.Create("https://www.acme.com"))
                    .SetVatNumber(VatNumber.Create("US12-3456789")),

                Company.Create(
                    name: $"TechInnovate GmbH{GetSuffix(ticks)}",
                    registrationNumber: "HRB987654",
                    contactEmail: EmailAddress.Create($"info{GetSuffix(ticks)}@techinnovate.de"),
                    address: Address.Create("Innovationsstraße 42", string.Empty, "10115", "Berlin", "Germany"))
                    .SetContactPhone(PhoneNumber.Create("+49301234567"))
                    .SetWebsite(Url.Create("https://www.techinnovate.de"))
                    .SetVatNumber(VatNumber.Create("DE123456789")),

                Company.Create(
                    name: $"Global Trade Ltd{GetSuffix(ticks)}",
                    registrationNumber: "GTL789012",
                    contactEmail: EmailAddress.Create($"enquiries{GetSuffix(ticks)}@globaltrade.co.uk"),
                    address: Address.Create("1 Commerce Street", "Floor 15", "EC1A 1BB", "London", "United Kingdom"))
                    .SetContactPhone(PhoneNumber.Create("+442071234567"))
                    .SetWebsite(Url.Create("https://www.globaltrade.co.uk"))
                    .SetVatNumber(VatNumber.Create("GB123456789"))
            }.ForEach(e => e.Id = CompanyId.Create($"{GuidGenerator.Create($"Company_{e.Name}")}"))];
    }

    public static class Tenants
    {
        public static Tenant[] Create(Company[] companies, long ticks = 0) =>
            [.. new[]
            {
                Tenant.Create(
                    companies[0],
                    $"AcmeBooks{GetSuffix(ticks)}",
                    $"books@acme{GetSuffix(ticks)}.com")
                .AddSubscription()
                    .SetSchedule(Schedule.Create(
                        DateOnly.FromDateTime(new DateTime(2020, 1, 1)),
                        DateOnly.FromDateTime(new DateTime(2022, 12, 31))))
                    .SetPlanType(TenantSubscriptionPlanType.Free).Tenant
                .AddSubscription()
                    .SetSchedule(Schedule.Create(
                        DateOnly.FromDateTime(new DateTime(2023, 1, 1))))
                    .SetPlanType(TenantSubscriptionPlanType.Basic)
                    .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly).Tenant
                .SetBranding(TenantBranding.Create("#000000", "#AAAAAA"))
            }.ForEach(e => e.Id = TenantId.Create($"{GuidGenerator.Create($"Company_{e.Name}")}"))];
    }

    public static class Customers
    {
        public static Customer[] Create(Tenant[] tenants, long ticks = 0) =>
            [.. new[]
            {
                Customer.Create($"John", $"Doe", EmailAddress.Create($"john.doe{GetSuffix(ticks)}@example.com"), Address.Create("Main Street", string.Empty, "17100", "Anytown", "USA")),
                Customer.Create($"Mary", $"Jane", EmailAddress.Create($"mary.jane{GetSuffix(ticks)}@example.com"), Address.Create("Maple Street", string.Empty, "17101", "Anytown", "USA"))
            }.ForEach(e => e.Id = CustomerId.Create($"{GuidGenerator.Create($"Customer_{e.Email}")}"))];
    }

    public static class Authors
    {
        public static Author[] Create(Tenant[] tenants, long ticks = 0) =>
            [.. new[]
            {
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Martin", "Fowler"], string.Empty, string.Empty), "Martin Fowler is a British software developer, author and international public speaker on software development, specializing in object-oriented analysis and design, UML, patterns, and agile software development methodologies, including extreme programming."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Robert", "C.", "Martin"], string.Empty, string.Empty), "Robert C. Martin, colloquially called 'Uncle Bob', is an American software engineer, instructor, and best-selling author. He is most recognized for developing many software design principles and for being a founder of the influential Agile Manifesto."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Eric", "Evans"], string.Empty, string.Empty), "Eric Evans is a thought leader in software design and domain modeling. He is the author of 'Domain-Driven Design: Tackling Complexity in the Heart of Software'."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Gregor", "Hohpe"], string.Empty, string.Empty), "Gregor Hohpe is a software architect and author known for his work on enterprise integration patterns and cloud computing."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Sam", "Newman"], string.Empty, string.Empty), "Sam Newman is a technologist and consultant specializing in cloud computing, continuous delivery, and microservices."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Vaughn", "Vernon"], string.Empty, string.Empty), "Vaughn Vernon is a software developer and architect with more than 35 years of experience in a broad range of business domains."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Neal", "Ford"], string.Empty, string.Empty), "Neal Ford is a software architect, programmer, and author. He is an internationally recognized expert on software development and delivery, especially in the intersection of agile engineering techniques and software architecture."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Mark", "Richards"], string.Empty, string.Empty), "Mark Richards is an experienced, hands-on software architect involved in the architecture, design, and implementation of microservices architectures, service-oriented architectures, and distributed systems."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Dino", "Esposito"], string.Empty, string.Empty), "Dino Esposito is a well-known web development expert and the author of many popular books on ASP.NET, AJAX, and JavaScript."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Len", "Bass"], "Dr.", string.Empty), "Len Bass is a senior principal researcher at National ICT Australia Ltd. He has authored numerous books and articles on software architecture, programming, and product line engineering."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Erich", "Gamma"], "Dr.", string.Empty), "Erich Gamma is a Swiss computer scientist and co-author of the influential software engineering book 'Design Patterns: Elements of Reusable Object-Oriented Software'."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Richard", "Helm"], string.Empty, string.Empty), "Richard Helm is a co-author of the 'Gang of Four' book on Design Patterns and has extensive experience in object-oriented technology and software architecture."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Ralph", "Johnson"], "Dr.", string.Empty), "Ralph Johnson is a Research Associate Professor in the Department of Computer Science at the University of Illinois at Urbana-Champaign and a co-author of the 'Gang of Four' Design Patterns book."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["John", "Vlissides"], string.Empty, string.Empty), "John Vlissides was a software consultant, designer, and implementer with expertise in object-oriented technology and a co-author of Design Patterns."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Chris", "Richardson"], string.Empty, string.Empty), "Chris Richardson is a developer and architect. He is a Java Champion, a JavaOne rock star and the author of POJOs in Action, which describes how to build enterprise Java applications with frameworks such as Spring and Hibernate."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Grady", "Booch"], string.Empty, string.Empty), "Grady Booch is an American software engineer, best known for developing the Unified Modeling Language with Ivar Jacobson and James Rumbaugh."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Ivar", "Jacobson"], "Dr.", string.Empty), "Ivar Jacobson is a Swedish computer scientist and software engineer, known as a major contributor to UML, Objectory, RUP, and Aspect-oriented software development."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["James", "Rumbaugh"], "Dr.", string.Empty), "James Rumbaugh is an American computer scientist and object-oriented methodologist who is best known for his work in creating the Object Modeling Technique and the Unified Modeling Language."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Michael", "Feathers"], string.Empty, string.Empty), "Michael Feathers is a consultant and author in the field of software development. He is a specialist in software testing and process improvement."),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Brendan", "Burns"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["George", "Fairbanks"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Rebecca", "Parsons"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Patrick", "Kua"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Pramod", "Sadalage"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Paul", "Clements"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Felix", "Bachmann"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["David", "Garlan"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["James", "Ivers"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Reed", "Little"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Paulo", "Merson"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Robert", "Nord"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Judith", "Stafford"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Michael", "T.", "Nygard"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Martin", "L.", "Abbot"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Michael", "T.", "Fisher"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Cornelia", "Davies"], string.Empty, string.Empty), string.Empty),
                Author.Create(tenants[0].Id, PersonFormalName.Create(["Martin", "Kleppmann"], string.Empty, string.Empty), string.Empty) // 36
            }.ForEach(e => e.Id = AuthorId.Create($"{GuidGenerator.Create($"Author_{e.PersonName.Full}{GetSuffix(ticks)}")}"))];
    }

    public static class Publishers
    {
        public static Publisher[] Create(Tenant[] tenants, long ticks = 0) =>
            [.. new[]
            {
                Publisher.Create($"Addison-Wesley Professional{GetSuffix(ticks)}", "Addison-Wesley Professional is a publisher of textbooks and computer literature. It is an imprint of Pearson PLC, a global publishing and education company."),
                Publisher.Create($"O'Reilly Media{GetSuffix(ticks)}", "O'Reilly Media is an American learning company established by Tim O'Reilly that publishes books, produces tech conferences, and provides an online learning platform."),
                Publisher.Create($"Manning Publications{GetSuffix(ticks)}", "Manning Publications is an American publisher established in 1993 that specializes in computer books for software developers, engineers, architects, system administrators, and managers."),
                Publisher.Create($"Packt Publishing{GetSuffix(ticks)}", "Packt Publishing is a publisher of technology books, eBooks and video courses for IT developers, administrators, and users."),
                Publisher.Create($"Marschall & Brainerd{GetSuffix(ticks)}", string.Empty)
            }.ForEach(e => e.Id = PublisherId.Create($"{GuidGenerator.Create($"Publisher_{e.Name}")}"))];
    }

    public static class Tags
    {
        public static Tag[] Create(Tenant[] tenants, long ticks = 0) =>
            [.. new[]
            {
                Tag.Create($"SoftwareArchitecture{GetSuffix(ticks)}"),
                Tag.Create($"DomainDrivenDesign{GetSuffix(ticks)}"),
                Tag.Create($"Microservices{GetSuffix(ticks)}"),
                Tag.Create($"CleanArchitecture{GetSuffix(ticks)}"),
                Tag.Create($"DesignPatterns{GetSuffix(ticks)}"),
                Tag.Create($"CloudArchitecture{GetSuffix(ticks)}"),
                Tag.Create($"EnterpriseArchitecture{GetSuffix(ticks)}"),
                Tag.Create($"ArchitecturalPatterns{GetSuffix(ticks)}"),
                Tag.Create($"SystemDesign{GetSuffix(ticks)}"),
                Tag.Create($"SoftwareDesign{GetSuffix(ticks)}")
            }.ForEach(e => e.Id = TagId.Create($"{GuidGenerator.Create($"Tag_{e.Name}")}"))];
    }

    public static class Categories
    {
        public static Category[] Create(Tenant[] tenants, long ticks = 0) =>
            [.. new[]
            {
                Category.Create($"Software-Architecture{GetSuffix(ticks)}", "Software Architecture", 0)
                    .AddChild(Category.Create($"Design-Patterns{GetSuffix(ticks)}", "Design Patterns", 0))
                    .AddChild(Category.Create($"Architectural-Styles{GetSuffix(ticks)}", "Architectural Styles", 1)
                        .AddChild(Category.Create($"Microservices{GetSuffix(ticks)}", "Microservices Architecture", 0))
                        .AddChild(Category.Create($"SOA{GetSuffix(ticks)}", "Service-Oriented Architecture", 1)))
                    .AddChild(Category.Create($"Domain-Driven-Design{GetSuffix(ticks)}", "Domain-Driven Design", 2)),
                Category.Create($"Enterprise-Architecture{GetSuffix(ticks)}", "Enterprise Architecture", 1)
                    .AddChild(Category.Create($"Cloud-Architecture{GetSuffix(ticks)}", "Cloud Architecture", 0))
                    .AddChild(Category.Create($"Integration-Patterns{GetSuffix(ticks)}", "Integration Patterns", 1)),
                Category.Create($"Software-Design{GetSuffix(ticks)}", "Software Design", 2)
                    .AddChild(Category.Create($"Clean-Architecture{GetSuffix(ticks)}", "Clean Architecture", 0))
                    .AddChild(Category.Create($"SOLID-Principles{GetSuffix(ticks)}", "SOLID Principles", 1)),
                Category.Create($"Architectural-Practices{GetSuffix(ticks)}", "Architectural Practices", 3)
                    .AddChild(Category.Create($"Scalability{GetSuffix(ticks)}", "Scalability", 0))
                    .AddChild(Category.Create($"Security{GetSuffix(ticks)}", "Security", 1))
                    .AddChild(Category.Create($"Performance{GetSuffix(ticks)}", "Performance", 2))
            }.ForEach(e => e.Id = CategoryId.Create($"{GuidGenerator.Create($"Category_{e.Title}")}"))];
    }

    public static class Books
    {
        public static Book[] Create(Tenant[] tenants, Tag[] tags, Category[] categories, Publisher[] publishers, Author[] authors, long ticks = 0) =>
            [.. new[]
            {
                Book.Create(
                    tenants[0].Id,
                    $"Domain-Driven Design: Tackling Complexity in the Heart of Software{GetSuffix(ticks)}",
                    "Eric Evans' book on how domain-driven design works in practice.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321125217")),
                    Money.Create(54.99m),
                    publishers[0],
                    new DateOnly(2003, 8, 30))
                        .AssignAuthor(authors[2]) // Eric Evans
                        .AddTag(tags[1]).AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddChapter("Chapter 1: Putting the Domain Model to Work")
                        .AddChapter("Chapter 2: The Building Blocks of a Model-Driven Design")
                        .AddChapter("Chapter 3: Refactoring Toward Deeper Insight"),
                Book.Create(
                    tenants[0].Id,
                    $"Clean Architecture: A Craftsman's Guide to Software Structure and Design{GetSuffix(ticks)}",
                    "Robert C. Martin's guide to building robust and maintainable software systems.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0134494166")),
                    Money.Create(37.49m),
                    publishers[0],
                    new DateOnly(2017, 9, 10))
                        .AssignAuthor(authors[1]) // Robert C. Martin
                        .AddTag(tags[3]).AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[2].Children.ToArray()[0])
                        .AddChapter("Chapter 1: What Is Design and Architecture?")
                        .AddChapter("Chapter 2: A Tale of Two Values")
                        .AddChapter("Chapter 3: Paradigm Overview"),
                Book.Create(
                    tenants[0].Id,
                    $"Patterns of Enterprise Application Architecture{GetSuffix(ticks)}",
                    "Martin Fowler's guide to enterprise application design patterns.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321127426")),
                    Money.Create(59.99m),
                    publishers[0],
                    new DateOnly(2002, 11, 15))
                        .AssignAuthor(authors[0]) // Martin Fowler
                        .AddTag(tags[4]).AddTag(tags[6])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Layering")
                        .AddChapter("Chapter 2: Organizing Domain Logic")
                        .AddChapter("Chapter 3: Mapping to Relational Databases"),
                Book.Create(
                    tenants[0].Id,
                    $"Enterprise Integration Patterns{GetSuffix(ticks)}",
                    "Gregor Hohpe's comprehensive catalog of messaging patterns.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321200686")),
                    Money.Create(69.99m),
                    publishers[0],
                    new DateOnly(2003, 10, 10))
                        .AssignAuthor(authors[3]) // Gregor Hohpe
                        .AddTag(tags[6]).AddTag(tags[7])
                        .AddCategory(categories[1])
                        .AddCategory(categories[1].Children.ToArray()[1])
                        .AddChapter("Chapter 1: Introduction to Messaging Systems")
                        .AddChapter("Chapter 2: Integration Styles")
                        .AddChapter("Chapter 3: Messaging Systems"),
                Book.Create(
                    tenants[0].Id,
                    $"Building Microservices{GetSuffix(ticks)}",
                    "Sam Newman's guide to designing fine-grained systems.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1491950357")),
                    Money.Create(44.99m),
                    publishers[1],
                    new DateOnly(2015, 2, 20))
                        .AssignAuthor(authors[4]) // Sam Newman
                        .AddTag(tags[2]).AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[1].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Microservices")
                        .AddChapter("Chapter 2: The Evolutionary Architect")
                        .AddChapter("Chapter 3: How to Model Services"),
                Book.Create(
                    tenants[0].Id,
                    $"Implementing Domain-Driven Design{GetSuffix(ticks)}",
                    "Vaughn Vernon's practical guide to DDD implementation.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321834577")),
                    Money.Create(59.99m),
                    publishers[0],
                    new DateOnly(2013, 2, 6))
                        .AssignAuthor(authors[5]) // Vaughn Vernon
                        .AddTag(tags[1]).AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddChapter("Chapter 1: Getting Started with DDD")
                        .AddChapter("Chapter 2: Domains, Subdomains, and Bounded Contexts")
                        .AddChapter("Chapter 3: Context Maps"),
                Book.Create(
                    tenants[0].Id,
                    $"Software Architecture in Practice{GetSuffix(ticks)}",
                    "Len Bass's comprehensive examination of software architecture in practice.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321815736")),
                    Money.Create(64.99m),
                    publishers[0],
                    new DateOnly(2012, 9, 25))
                        .AssignAuthor(authors[9]) // Len Bass
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: What Is Software Architecture?")
                        .AddChapter("Chapter 2: Why Is Software Architecture Important?")
                        .AddChapter("Chapter 3: The Many Contexts of Software Architecture"),
                Book.Create(
                    tenants[0].Id,
                    $"Design Patterns: Elements of Reusable Object-Oriented Software{GetSuffix(ticks)}",
                    "The classic 'Gang of Four' book on design patterns.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0201633610")),
                    Money.Create(59.99m),
                    publishers[0],
                    new DateOnly(1994, 10, 31))
                        .AssignAuthor(authors[10]) // Erich Gamma
                        .AssignAuthor(authors[11]) // Richard Helm
                        .AssignAuthor(authors[12]) // Ralph Johnson
                        .AssignAuthor(authors[13]) // John Vlissides
                        .AddTag(tags[4]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: A Case Study: Designing a Document Editor")
                        .AddChapter("Chapter 3: Creational Patterns"),
                Book.Create(
                    tenants[0].Id,
                    $"Microservices Patterns{GetSuffix(ticks)}",
                    "Chris Richardson's guide to solving challenges in microservices architecture.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1617294549")),
                    Money.Create(49.99m),
                    publishers[2],
                    new DateOnly(2018, 11, 19))
                        .AssignAuthor(authors[14]) // Chris Richardson
                        .AddTag(tags[2]).AddTag(tags[7])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[1].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Escaping Monolithic Hell")
                        .AddChapter("Chapter 2: Decomposition Strategies")
                        .AddChapter("Chapter 3: Interprocess Communication in a Microservice Architecture"),
                Book.Create(
                    tenants[0].Id,
                    $"Object-Oriented Analysis and Design with Applications{GetSuffix(ticks)}",
                    "Grady Booch's classic text on OOAD.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0201895513")),
                    Money.Create(74.99m),
                    publishers[0],
                    new DateOnly(1994, 9, 30)
                    )
                        .AssignAuthor(authors[15]) // Grady Booch
                        .AddTag(tags[9]).AddTag(tags[4])
                        .AddCategory(categories[2])
                        .AddChapter("Chapter 1: Complexity")
                        .AddChapter("Chapter 2: The Object Model")
                        .AddChapter("Chapter 3: Classes and Objects"),
                Book.Create(
                    tenants[0].Id,
                    $"The Unified Modeling Language User Guide{GetSuffix(ticks)}",
                    "Comprehensive guide to UML by its creators.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321267974")),
                    Money.Create(64.99m),
                    publishers[0],
                    new DateOnly(1998, 9, 29))
                        .AssignAuthor(authors[15]) // Grady Booch
                        .AssignAuthor(authors[16]) // Ivar Jacobson
                        .AssignAuthor(authors[17]) // James Rumbaugh
                        .AddTag(tags[9]).AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddChapter("Chapter 1: Getting Started")
                        .AddChapter("Chapter 2: Classes")
                        .AddChapter("Chapter 3: Relationships"),
                Book.Create(
                    tenants[0].Id,
                    $"Working Effectively with Legacy Code{GetSuffix(ticks)}",
                    "Michael Feathers' strategies for dealing with large, untested legacy code bases.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0131177055")),
                    Money.Create(54.99m),
                    publishers[0],
                    new DateOnly(2004, 9, 1))
                        .AssignAuthor(authors[18]) // Michael Feathers
                        .AddTag(tags[9]).AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Changing Software")
                        .AddChapter("Chapter 2: Working with Feedback")
                        .AddChapter("Chapter 3: Sensing and Separation"),
                Book.Create(
                    tenants[0].Id,
                    $"Fundamentals of Software Architecture{GetSuffix(ticks)}",
                    "A comprehensive guide to software architecture fundamentals.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1492043454")),
                    Money.Create(59.99m),
                    publishers[1],
                    new DateOnly(2020, 2, 1))
                        .AssignAuthor(authors[7]) // Mark Richards
                        .AssignAuthor(authors[6]) // Neal Ford
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Architectural Thinking")
                        .AddChapter("Chapter 3: Modularity"),
                Book.Create(
                    tenants[0].Id,
                    $"Designing Data-Intensive Applications{GetSuffix(ticks)}",
                    "Martin Kleppmann's guide to the principles, practices, and patterns of modern data systems.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1449373320")),
                    Money.Create(49.99m),
                    publishers[1],
                    new DateOnly(2017, 3, 16))
                        .AssignAuthor(authors[36]) // Martin Kleppmann
                        .AddTag(tags[8]).AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3].Children.ToArray()[2])
                        .AddChapter("Chapter 1: Reliable, Scalable, and Maintainable Applications")
                        .AddChapter("Chapter 2: Data Models and Query Languages")
                        .AddChapter("Chapter 3: Storage and Retrieval"),
                Book.Create(
                    tenants[0].Id,
                    $"Cloud Native Patterns{GetSuffix(ticks)}",
                    "Designing change-tolerant software for cloud platforms.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1617294296")),
                    Money.Create(49.99m),
                    publishers[2],
                    new DateOnly(2019, 5, 13))
                        .AssignAuthor(authors[35]) // Cornelia Davis
                        .AddTag(tags[5]).AddTag(tags[7])
                        .AddCategory(categories[1])
                        .AddCategory(categories[1].Children.ToArray()[0])
                        .AddChapter("Chapter 1: You Keep Using That Word")
                        .AddChapter("Chapter 2: Running Cloud-Native Applications in Production")
                        .AddChapter("Chapter 3: The Platform"),
                Book.Create(
                    tenants[0].Id,
                    $"Refactoring: Improving the Design of Existing Code{GetSuffix(ticks)}",
                    "Martin Fowler's guide to refactoring and improving code design.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0134757599")),
                    Money.Create(49.99m),
                    publishers[0],
                    new DateOnly(1999, 7, 8))
                        .AssignAuthor(authors[0]) // Martin Fowler
                        .AddTag(tags[9]).AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Refactoring: A First Example")
                        .AddChapter("Chapter 2: Principles in Refactoring")
                        .AddChapter("Chapter 3: Bad Smells in Code"),
                Book.Create(
                    tenants[0].Id,
                    $"Clean Code: A Handbook of Agile Software Craftsmanship{GetSuffix(ticks)}",
                    "Robert C. Martin's guide to writing clean, maintainable code.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0132350884")),
                    Money.Create(44.99m),
                    publishers[0],
                    new DateOnly(2008, 8, 1))
                        .AssignAuthor(authors[1]) // Robert C. Martin
                        .AddTag(tags[9]).AddTag(tags[3])
                        .AddCategory(categories[2])
                        .AddCategory(categories[2].Children.ToArray()[1])
                        .AddChapter("Chapter 1: Clean Code")
                        .AddChapter("Chapter 2: Meaningful Names")
                        .AddChapter("Chapter 3: Functions"),
                Book.Create(
                    tenants[0].Id,
                    $"The Art of Scalability{GetSuffix(ticks)}",
                    "Principles and practices for scaling web architectures.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0134032801")),
                    Money.Create(59.99m),
                    publishers[0],
                    new DateOnly(2009, 12, 1))
                        .AssignAuthor(authors[33]) // Martin L. Abbott
                        .AssignAuthor(authors[34]) // Michael T. Fisher
                        .AddTag(tags[0]).AddTag(tags[8])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Scaling Concepts")
                        .AddChapter("Chapter 2: Principles of Scalability")
                        .AddChapter("Chapter 3: Processes for Scalable Architectures"),
                Book.Create(
                    tenants[0].Id,
                    $"Release It!: Design and Deploy Production-Ready Software{GetSuffix(ticks)}",
                    "Michael T. Nygard's guide to designing and architecting applications for the real world.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1680502398")),
                    Money.Create(47.99m),
                    publishers[2],
                    new DateOnly(2007, 3, 30))
                        .AssignAuthor(authors[31]) // Michael T. Nygard
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Living in Production")
                        .AddChapter("Chapter 2: Case Study: The Exception That Grounded an Airline")
                        .AddChapter("Chapter 3: Stability Antipatterns"),
                Book.Create(
                    tenants[0].Id,
                    $"Documenting Software Architectures: Views and Beyond{GetSuffix(ticks)}",
                    "A comprehensive guide to documenting software architectures.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321552686")),
                    Money.Create(69.99m),
                    publishers[0],
                    new DateOnly(2010, 10, 5))
                        .AssignAuthor(authors[24]) // Paul Clements
                        .AssignAuthor(authors[25]) // Felix Bachmann
                        .AssignAuthor(authors[9]) //  Len Bass,
                        .AssignAuthor(authors[26]) // David Garlan
                        .AssignAuthor(authors[27]) // James Ivers
                        .AssignAuthor(authors[28]) // Reed Little
                        .AssignAuthor(authors[29]) // Paulo Merson
                        .AssignAuthor(authors[30]) // Robert Nord
                        .AssignAuthor(authors[31]) // Judith Stafford
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Software Architecture Documentation in Practice")
                        .AddChapter("Chapter 3: A System of Views"),
                Book.Create(
                    tenants[0].Id,
                    $"Building Evolutionary Architectures{GetSuffix(ticks)}",
                    "Support Constant Change.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1491986360")),
                    Money.Create(39.99m),
                    publishers[1],
                    new DateOnly(2017, 10, 5))
                        .AssignAuthor(authors[6]) //  Neal Ford
                        .AssignAuthor(authors[21]) // Rebecca Parsons
                        .AssignAuthor(authors[22]) // Patrick Kua
                        .AssignAuthor(authors[23]) // Pramod Sadalage
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Software Architecture")
                        .AddChapter("Chapter 2: Evolutionary Architecture")
                        .AddChapter("Chapter 3: Engineering Incremental Change"),
                Book.Create(
                    tenants[0].Id,
                    $"Just Enough Software Architecture{GetSuffix(ticks)}",
                    "A Risk-Driven Approach.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0984618101")),
                    Money.Create(59.99m),
                    publishers[4],
                    new DateOnly(2010, 8, 1))
                        .AssignAuthor(authors[20]) // George Fairbanks
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Risk-Driven Model")
                        .AddChapter("Chapter 3: Engineering and Evaluating Software Architectures"),
                Book.Create(
                    tenants[0].Id,
                    $"Software Systems Architecture{GetSuffix(ticks)}",
                    "Working with Stakeholders Using Viewpoints and Perspectives.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0321112293")),
                    Money.Create(64.99m),
                    publishers[0],
                    new DateOnly(2005, 4, 1))
                        .AssignAuthor(authors[9]) // Len Bass
                        .AddTag(tags[0]).AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Software Architecture Concepts")
                        .AddChapter("Chapter 3: Viewpoints and Views"),
                Book.Create(
                    tenants[0].Id,
                    $"Domain-Driven Design Distilled{GetSuffix(ticks)}",
                    "Vaughn Vernon's concise guide to the fundamentals of DDD.",
                    BookIsbn.Create(GetIsbn(ticks, "978-0134434421")),
                    Money.Create(29.99m),
                    publishers[0],
                    new DateOnly(2016, 6, 1))
                        .AssignAuthor(authors[5]) // Vaughn Vernon
                        .AddTag(tags[1]).AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddChapter("Chapter 1: DDD for Me")
                        .AddChapter("Chapter 2: Strategic Design with Bounded Contexts and the Ubiquitous Language")
                        .AddChapter("Chapter 3: Strategic Design with Subdomains"),
                Book.Create(
                    tenants[0].Id,
                    $"Designing Distributed Systems{GetSuffix(ticks)}",
                    "Patterns and Paradigms for Scalable, Reliable Services.",
                    BookIsbn.Create(GetIsbn(ticks, "978-1491983645")),
                    Money.Create(39.99m),
                    publishers[1],
                    new DateOnly(2018, 2, 20))
                        .AssignAuthor(authors[19]) // Brendan Burns
                        .AddTag(tags[0]).AddTag(tags[8])
                        .AddCategory(categories[0])
                        .AddCategory(categories[1])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Single-Node Patterns")
                        .AddChapter("Chapter 3: Serving Patterns")
            }.ForEach(e => e.Id = BookId.Create($"{GuidGenerator.Create($"Book_{e.Title}")}"))];
    }
}