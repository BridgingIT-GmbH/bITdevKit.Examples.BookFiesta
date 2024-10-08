// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantBrandingModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string PrimaryColor { get; set; }

    public string SecondaryColor { get; set; }

    public string LogoUrl { get; set; }

    public string FaviconUrl { get; set; }

    public string CustomCss { get; set; }
}