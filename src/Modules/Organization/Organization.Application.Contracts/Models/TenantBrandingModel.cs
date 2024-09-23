// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantBrandingModel
{
    public string Id { get; set; }

    public string TenantId { get; }

    public string PrimaryColor { get; }

    public string SecondaryColor { get; }

    public string LogoUrl { get; }

    public string FaviconUrl { get; }

    public string CustomCss { get; }
}