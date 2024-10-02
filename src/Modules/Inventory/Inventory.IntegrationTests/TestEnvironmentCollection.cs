﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(TestEnvironmentCollection))]
public class TestEnvironmentCollection : ICollectionFixture<TestEnvironmentFixture> { }
// https://xunit.net/docs/shared-context#collection-fixture