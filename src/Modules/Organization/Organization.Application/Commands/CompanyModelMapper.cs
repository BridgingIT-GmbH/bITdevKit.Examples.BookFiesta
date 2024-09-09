//namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
//using System;
//using System.Collections.Generic;
//using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

//public static class CompanyModelMapper
//{
//    private static readonly List<Action<CompanyModel, Company>> PropertyMappers = [];

//    public static Company Map(CompanyModel source, Company destination = null)
//    {
//        destination ??= Company.Create(
//            source.Name,
//            source.RegistrationNumber,
//            EmailAddress.Create(source.ContactEmail),
//            MapAddress(source.Address));

//        if (destination.Id != null)
//        {
//            destination.SetName(source.Name)
//               .SetRegistrationNumber(source.RegistrationNumber)
//               .SetContactEmail(EmailAddress.Create(source.ContactEmail))
//               .SetAddress(MapAddress(source.Address));
//        }

//        destination.SetContactPhone(PhoneNumber.Create(source.ContactPhone))
//            .SetWebsite(Url.Create(source.Website))
//            .SetVatNumber(VatNumber.Create(source.VatNumber));

//        return destination;
//    }

//    private static Address MapAddress(AddressModel source)
//    {
//        return Address.Create(
//            source?.Name,
//            source?.Line1,
//            source?.Line2,
//            source?.PostalCode,
//            source?.City,
//            source?.Country);
//    }
//}
