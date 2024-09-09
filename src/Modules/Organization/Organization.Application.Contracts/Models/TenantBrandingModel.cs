// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantBrandingModel
{
    public string Id { get; set; }

    public string TenantId { get; private set; }

    public string PrimaryColor { get; private set; }

    public string SecondaryColor { get; private set; }

    public string LogoUrl { get; private set; }

    public string FaviconUrl { get; private set; }

    public string CustomCss { get; private set; }
}