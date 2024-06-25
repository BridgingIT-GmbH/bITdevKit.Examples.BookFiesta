// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Model;

public partial class PersonFormalName : ValueObject
{
    private static readonly Regex NamePartRegex = Regexes.NamePartRegex();
    private static readonly Regex TitleSuffixRegex = Regexes.TitleSuffixRegex();

    // Private constructor to enforce the use of the Create method
    private PersonFormalName() { }

    // Private constructor used by the Create method to set the name parts
    private PersonFormalName(string[] parts, string title = null, string suffix = null)
    {
        Validate(parts, title, suffix);

        this.Title = title;
        this.Parts = parts;
        this.Suffix = suffix;
    }

    // Public properties for the name parts
    public string Title { get; private set; }

    public IEnumerable<string> Parts { get; private set; }

    public string Suffix { get; private set; }

    //public string Full => this.ToString();
    public string Full
    {
        get
        {
            return this.ToString();
        }
        set // needs to be private
        {
            _ = value;
        }
    }

    // Implicit conversion to string representing the full name
    public static implicit operator string(PersonFormalName name) => name.ToString();

    // Factory method to create a new FullName instance
    public static PersonFormalName Create(string[] parts, string title = null, string suffix = null)
    {
        return new PersonFormalName(parts, title, suffix);
    }

    // Override ToString method to return the full name
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

        return fullName;
    }

    // Method to get atomic values for equality check
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
            throw new ArgumentException("Person name parts cannot be empty.");
        }

        foreach (var part in parts)
        {
            ValidateNamePart(part);
        }

        ValidateTitleSuffix(title, nameof(Title));
        ValidateTitleSuffix(suffix, nameof(Suffix));
    }

    // Method to validate each name part
    private static void ValidateNamePart(string namePart)
    {
        if (string.IsNullOrWhiteSpace(namePart))
        {
            throw new ArgumentException("Person name part cannot be empty.");
        }

        if (!NamePartRegex.IsMatch(namePart))
        {
            throw new ArgumentException("Person name part contains invalid characters.");
        }
    }

    // Method to validate titles and suffixes
    private static void ValidateTitleSuffix(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!TitleSuffixRegex.IsMatch(value))
        {
            throw new ArgumentException($"Person name {propertyName.ToLower()} contains invalid characters.");
        }
    }

    public static partial class Regexes
    {
        [GeneratedRegex(@"^[\p{L}\p{M}]+([\p{L}\p{M}'-]*[\p{L}\p{M}])?$", RegexOptions.Compiled)]
        public static partial Regex NamePartRegex();

        [GeneratedRegex(@"^[\p{L}\p{M}\.\-'\s]+$", RegexOptions.Compiled)]
        public static partial Regex TitleSuffixRegex();
    }
}
