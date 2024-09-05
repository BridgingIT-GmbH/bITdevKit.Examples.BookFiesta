// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}")]

public partial class EmailAddress : ValueObject
{
    private EmailAddress() { } // Private constructor required by EF Core

    private EmailAddress(string email)
    {
        this.Value = email;
    }

    public string Value { get; private set; }

    public static implicit operator string(EmailAddress email) => email.Value;
    public static implicit operator EmailAddress(string email) => Create(email);

    public static EmailAddress Create(string email)
    {
        email = Normalize(email);
        if (!IsValid(email))
        {
            throw new DomainRuleException("Invalid email address");
        }

        return new EmailAddress(email);
    }

    public override string ToString() => this.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string email)
    {
        return email?.Trim()?.ToLowerInvariant() ?? string.Empty;
    }

    [GeneratedRegex(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex IsValidRegex();

    private static bool IsValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Length <= 255 && IsValidRegex().IsMatch(email);
    }
}