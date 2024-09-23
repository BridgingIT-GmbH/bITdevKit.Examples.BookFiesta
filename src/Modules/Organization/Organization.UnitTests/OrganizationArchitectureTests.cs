// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.UnitTests;

using System.Reflection;
using DevKit.Application.Commands;
using DevKit.Application.Queries;
using DevKit.Domain.Model;
using Dumpify;
using NetArchTest.Rules;

#pragma warning disable CS9113 // Parameter is unread.
//[Module("Organization")]
[Category("SystemTest")]
[SystemTest("Organization:Architecture")]
//[UnitTest("Architecture")]
public class OrganizationArchitectureTests(ITestOutputHelper output, TypesFixture fixture) : IClassFixture<TypesFixture>
#pragma warning restore CS9113 // Parameter is unread.
{
    // [Fact]
    // public void Module_ShouldNot_HaveDependencyOnOtherModules()
    // {
    //     var result = fixture.Types
    //         //.That()
    //             //.DoNotResideInNamespaceContaining("Contracts")
    //         .Slice().ByNamespacePrefix("BridgingIT.DevKit.Examples.BookFiesta.Modules")
    //         .Should()
    //             .NotHaveDependenciesBetweenSlices().GetResult();
    //
    //     // TODO: references to Modules.OTHER.Application.Contracts should be ignored as they are allowed (cross module refs)
    //
    //     output.WriteLine(result.LoadedTypes.DumpText());
    //     result.IsSuccessful.ShouldBeTrue("Module should not have dependencies on other modules.\n" + result.FailingTypes.DumpText());
    // }

    [Fact]
    public void ApplicationCommand_Should_ResideInApplication()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ImplementInterface(typeof(ICommandRequest<>))
            .And()
            .DoNotResideInNamespace("BridgingIT.DevKit.Application")
            .Should()
            .ResideInNamespaceContaining("BridgingIT.DevKit.Examples.BookFiesta.Application")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Application command should reside in Application.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void ApplicationQuery_Should_ResideInApplication()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ImplementInterface(typeof(IQueryRequest<>))
            .And()
            .DoNotResideInNamespace("BridgingIT.DevKit.Application")
            .Should()
            .ResideInNamespaceContaining("BridgingIT.DevKit.Examples.BookFiesta.Application")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Application query should reside in Application.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Application")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Application")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Presentation")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnApplication()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Domain")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Application")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Domain")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Domain")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Presentation")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void Infrastructure_ShouldNot_HaveDependencyOnPresentation()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ResideInNamespace("BridgingIT.DevKit.Examples.BookFiesta.Infrastructure")
            .ShouldNot()
            .HaveDependencyOnAny("BridgingIT.DevKit.Examples.BookFiesta.Presentation")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain layer has not allowed dependencies.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainEntity_ShouldNot_HavePublicConstructor()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ImplementInterface(typeof(IEntity))
            .ShouldNot()
            .HavePublicConstructor()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain entity should not have a public constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainEntity_Should_HaveParameterlessConstructor()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .ImplementInterface(typeof(IEntity))
            .Should()
            .HaveParameterlessConstructor()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain entity should have a parameterless constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainValueObject_ShouldNot_HavePublicConstructor()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .Inherit(typeof(ValueObject))
            .ShouldNot()
            .HavePublicConstructor()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain valueobjects should not have a public constructor.\n" + result.FailingTypes.DumpText());
    }

    [Fact]
    public void DomainValueObject_Should_HaveParameterlessConstructor()
    {
        var result = fixture.Types.That()
            .HaveNameStartingWith("BridgingIT.DevKit.Examples.BookFiesta")
            .And()
            .Inherit(typeof(ValueObject))
            .Should()
            .HaveParameterlessConstructor()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            "Domain valueobject should have a parameterless constructor.\n" + result.FailingTypes.DumpText());
    }

    //public void Test2()
    //{
    //    var result = fixture.Types.InCurrentDomain()
    //              .Slice()
    //              .ByNamespacePrefix("BridgingIT.DevKit.Examples.BookFiesta")
    //              .Should()
    //              .NotHaveDependenciesBetweenSlices()
    //              .GetResult();
    //}
}

public class TypesFixture
{
    public Types Types { get; } = Types.FromPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
}