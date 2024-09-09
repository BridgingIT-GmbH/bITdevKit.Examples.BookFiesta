// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, TenantId={TenantId}")]
[TypedEntityId<Guid>]
public class TenantBranding : Entity<TenantBrandingId>
{
    private TenantBranding() { } // Private constructor required by EF Core

    private TenantBranding(HexColor primaryColor = null, HexColor secondaryColor = null, Url logoUrl = null, Url faviconUrl = null)
    {
        this.SetPrimaryColor(primaryColor);
        this.SetSecondaryColor(secondaryColor);
        this.SetLogoUrl(logoUrl);
        this.SetFaviconUrl(faviconUrl);
    }

    public TenantId TenantId { get; private set; }

    public HexColor PrimaryColor { get; private set; }

    public HexColor SecondaryColor { get; private set; }

    public Url LogoUrl { get; private set; }

    public Url FaviconUrl { get; private set; }

    public string CustomCss { get; private set; }

    public static TenantBranding Create(HexColor primaryColor = null, HexColor secondaryColor = null, Url logoUrl = null, Url faviconUrl = null)
    {
        return new TenantBranding(primaryColor, secondaryColor, logoUrl, faviconUrl);
    }

    public TenantBranding SetPrimaryColor(HexColor color)
    {
        this.PrimaryColor = color;
        return this;
    }

    public TenantBranding SetSecondaryColor(HexColor color)
    {
        this.SecondaryColor = color;
        return this;
    }

    public TenantBranding SetLogoUrl(Url url)
    {
        this.LogoUrl = url;
        return this;
    }

    public TenantBranding SetFaviconUrl(Url url)
    {
        this.FaviconUrl = url;
        return this;
    }

    public TenantBranding SetCustomCss(string customCss)
    {
        this.CustomCss = customCss; // TODO: check if valid css (xss)
        return this;
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}