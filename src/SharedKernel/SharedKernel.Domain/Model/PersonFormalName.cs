// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Name={ToString()}")]
public partial class PersonFormalName : ValueObject
{
    private static readonly Regex NamePartRegex = Regexes.NamePartRegex();
    private static readonly Regex TitleSuffixRegex = Regexes.TitleSuffixRegex();

    private PersonFormalName() { } // Private constructor required by EF Core

    private PersonFormalName(string[] parts, string title = null, string suffix = null)
    {
        Validate(parts, title, suffix);

        this.Title = title;
        this.Parts = parts;
        this.Suffix = suffix;
    }

    public string Title { get; private set; }

    public IEnumerable<string> Parts { get; }

    public string Suffix { get; private set; }

    public string Full
    {
        get => this.ToString();
        set // needs to be private
            => _ = value;
    }

    public static implicit operator string(PersonFormalName name)
    {
        return name?.ToString();
        // allows a PersonFormalName value to be implicitly converted to a string.
    }

    public static PersonFormalName Create(IEnumerable<string> parts, string title = null, string suffix = null)
    {
        return Create(parts?.ToArray(), title, suffix);
    }

    public static PersonFormalName Create(string[] parts, string title = null, string suffix = null)
    {
        return new PersonFormalName(parts, title, suffix);
    }

    public override string ToString()
    {
        var fullName = string.Join(" ", this.Parts);

        if (!string.IsNullOrEmpty(this.Title))
        {
            fullName = $"{this.Title} {fullName}";
        }

        if (!string.IsNullOrEmpty(this.Suffix))
        {
            fullName = $"{fullName}, {this.Suffix}";
        }

        return fullName.Trim().Trim(',');
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Title;

        foreach (var part in this.Parts)
        {
            yield return part;
        }

        yield return this.Suffix;
    }

    private static void Validate(string[] parts, string title, string suffix)
    {
        if (!parts.SafeAny())
        {
            throw new ArgumentException("PersonFormalName parts cannot be empty.");
        }

        foreach (var part in parts)
        {
            ValidateNamePart(part);
        }

        ValidateTitleSuffix(title, nameof(Title));
        ValidateTitleSuffix(suffix, nameof(Suffix));
    }

    private static void ValidateNamePart(string namePart)
    {
        if (string.IsNullOrWhiteSpace(namePart))
        {
            throw new ArgumentException("PersonFormalName part cannot be empty.");
        }

        if (!NamePartRegex.IsMatch(namePart))
        {
            throw new ArgumentException("PersonFormalName part contains invalid characters.");
        }
    }

    private static void ValidateTitleSuffix(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!TitleSuffixRegex.IsMatch(value))
        {
            throw new ArgumentException($"PersonFormalName {propertyName.ToLower()} contains invalid characters.");
        }
    }

    public static partial class Regexes
    {
        // Update the regular expression pattern in the PersonFormalName class

        [GeneratedRegex(@"^[\p{L}\p{M}.]+([\p{L}\p{M}'-]*[\p{L}\p{M}])?$", RegexOptions.Compiled)]
        public static partial Regex NamePartRegex();

        [GeneratedRegex(@"^[\p{L}\p{M}\.\-'\s]+$", RegexOptions.Compiled)]
        public static partial Regex TitleSuffixRegex();
    }
}