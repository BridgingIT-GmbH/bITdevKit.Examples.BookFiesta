namespace BridgingIT.DevKit.Examples.BookStore.Catalog.UnitTests;

using System.Reflection;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Domain.Model;
using Dumpify;
using NetArchTest.Rules;
using Shouldly;

public class TypesFixture
{
    public Types Types { get; } = Types.FromPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
}

#pragma warning disable CS9113 // Parameter is unread.
public class ArchitectureTests(ITestOutputHelper output, TypesFixture fixture) : IClassFixture<TypesFixture>
#pragma warning restore CS9113 // Parameter is unread.
{
    [Fact]
    public void ApplicationCommand_Should_ResideInApplication()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ImplementInterface(typeof(ICommandRequest<>)).And().DoNotResideInNamespace("BridgingIT.DevKit.Application")
            .Should().ResideInNamespaceContaining(
                "BridgingIT.DevKit.Examples.BookStore.Application").GetResult();

        result.IsSuccessful.ShouldBeTrue("Application command should reside in Application.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void ApplicationQuery_Should_ResideInApplication()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ImplementInterface(typeof(IQueryRequest<>)).And().DoNotResideInNamespace("BridgingIT.DevKit.Application")
            .Should().ResideInNamespaceContaining(
                "BridgingIT.DevKit.Examples.BookStore.Application").GetResult();

        result.IsSuccessful.ShouldBeTrue("Application query should reside in Application.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Application")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Infrastructure").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Application")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Presentation").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnApplication()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Application").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Infrastructure").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Presentation").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Infrastructure_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ResideInNamespace("BridgingIT.DevKit.Examples.BookStore.Infrastructure")
            .ShouldNot().HaveDependencyOnAny(
                "BridgingIT.DevKit.Examples.BookStore.Presentation").GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainEntity_ShouldNot_HavePublicConstructor()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ImplementInterface(typeof(IEntity))
            .ShouldNot().HavePublicConstructor().GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain entity should not have a public constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainEntity_Should_HaveParameterlessConstructor()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .ImplementInterface(typeof(IEntity))
            .Should().HaveParameterlessConstructor().GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain entity should have a parameterless constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainValueObject_ShouldNot_HavePublicConstructor()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .Inherit(typeof(ValueObject))
            .ShouldNot().HavePublicConstructor().GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain valueobjects should not have a public constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainValueObject_Should_HaveParameterlessConstructor()
    {
        var result = fixture.Types
            .That().HaveNameStartingWith("BridgingIT.DevKit.Examples.BookStore").And()
                .Inherit(typeof(ValueObject))
            .Should().HaveParameterlessConstructor().GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain valueobject should have a parameterless constructor.\n" + result.FailingTypes.DumpText());
    }

    //public void Test2()
    //{
    //    var result = fixture.Types.InCurrentDomain()
    //              .Slice()
    //              .ByNamespacePrefix("BridgingIT.DevKit.Examples.BookStore")
    //              .Should()
    //              .NotHaveDependenciesBetweenSlices()
    //              .GetResult();
    //}
}
