namespace BridgingIT.DevKit.Examples.GettingStarted.Application;

using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.GettingStarted.Domain;

public class CustomerEmailMustBeUniqueRule(
    IGenericRepository<Customer> repository,
    Customer customer) : IBusinessRule
{
    public string Message => "Customer email should be unique";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            CustomerSpecifications.ForEmail(customer.Email), cancellationToken: cancellationToken)).SafeAny();
    }
}

public static partial class CustomerRules
{
    public static IBusinessRule EmailMustBeUnique(
        IGenericRepository<Customer> repository,
        Customer customer) => new CustomerEmailMustBeUniqueRule(repository, customer);
}