namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using BridgingIT.DevKit.Domain;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class EmailAddressIsValidRule(EmailAddress email) : IBusinessRule
{
    private static readonly Regex Regex = new( // // TODO: change to compiled regex (source gen)
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.Compiled);

    private readonly string value = email?.Value;

    public string Message => "Not a valid email address";

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(!string.IsNullOrEmpty(this.value) && this.value.Length <= 255 && Regex.IsMatch(this.value));
}

public static partial class CustomerRules
{
    public static IBusinessRule IsValid(EmailAddress email) =>
        new EmailAddressIsValidRule(email);
}