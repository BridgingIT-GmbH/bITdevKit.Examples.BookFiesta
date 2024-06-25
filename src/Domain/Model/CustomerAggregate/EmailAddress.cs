namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class EmailAddress : ValueObject
{
    private EmailAddress()
    {
    }

    private EmailAddress(string value)
    {
        this.Value = value;
    }

    public string Value { get; private set; }

    public static implicit operator string(EmailAddress email) => email.Value;

    public static EmailAddress Create(string value)
    {
        var email = new EmailAddress(value?.Trim()?.ToLowerInvariant());
        Check.Throw(
            CustomerRules.IsValid(email));

        return email;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}