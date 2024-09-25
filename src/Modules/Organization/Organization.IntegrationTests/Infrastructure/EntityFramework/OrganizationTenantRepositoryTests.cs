// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

[IntegrationTest("Infrastructure")]
[Collection(nameof(TestEnvironmentCollection))] // https://xunit.net/docs/shared-context#collection-fixture
public class OrganizationTenantRepositoryTests
{
    private readonly TestEnvironmentFixture fixture;
    private readonly OrganizationDbContext context;

    public OrganizationTenantRepositoryTests(ITestOutputHelper output, TestEnvironmentFixture fixture)
    {
        this.fixture = fixture.WithOutput(output);
        this.context = this.fixture.CreateSqlServerDbContext();
    }
}