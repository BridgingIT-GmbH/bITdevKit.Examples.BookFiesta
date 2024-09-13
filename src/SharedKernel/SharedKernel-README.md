SharedKernel![bITDevKit](https://raw.githubusercontent.com/bridgingIT/bITdevKit.Examples.BookFiesta/main/bITDevKit_Logo.png)
=====================================

# SharedKernel Component Overview

> The SharedKernel is a crucial component in our Domain-Driven Design (DDD) architecture. It
> contains common elements, concepts, and utilities that are shared across multiple bounded contexts
> or modules in the system. These shared elements can come from any layer of the application
> architecture, including the domain layer, application layer, infrastructure layer, and
> presentation
> layer.

## Domain Layer

The following value objects are part of the domain layer within the SharedKernel:

### 1. Address

- Represents a general physical address
- Contains fields for name, line1, line2, postal code, city, and country

```mermaid
classDiagram
    class Address {
        +string Name
        +string Line1
        +string Line2
        +string PostalCode
        +string City
        +string Country
        +Create(name, line1, line2, postalCode, city, country)
    }
```

### 2. AverageRating

- Represents an average rating with the total number of ratings
- Provides methods to add or remove individual ratings

```mermaid
classDiagram
    class AverageRating {
        +double? Value
        +int Amount
        +Create(value, numRatings)
        +Add(Rating)
        +Remove(Rating)
    }
```

### 3. Currency

- Represents a monetary currency
- Provides a list of world currencies with their symbols

```mermaid
classDiagram
    class Currency {
        +string Code
        +string Symbol
        +Create(code)
        +static Currency USDollar
        +static Currency Euro
        +static Currency GBPound
    }
```

### 4. EmailAddress

- Represents a valid email address
- Validates the email format using a regular expression

```mermaid
classDiagram
    class EmailAddress {
        +string Value
        +Create(email)
    }
```

### 5. HexColor

- Represents a color in hexadecimal format
- Provides methods to create from string or RGB values

```mermaid
classDiagram
    class HexColor {
        +string Value
        +Create(color)
        +Create(r, g, b)
        +ToRGB()
    }
```

### 6. Money

- Represents a monetary amount with a specific currency
- Provides arithmetic operations (addition, subtraction)

```mermaid
classDiagram
    class Money {
        +decimal Amount
        +Currency Currency
        +Create(amount, currency)
        +operator +(Money, Money)
        +operator -(Money, Money)
    }
```

### 7. PersonFormalName

- Represents a person's formal name
- Stores name parts, title, and suffix separately

```mermaid
classDiagram
    class PersonFormalName {
        +string Title
        +string[] Parts
        +string Suffix
        +string Full
        +Create(parts, title, suffix)
    }
```

### 8. PhoneNumber

- Represents a phone number with country code
- Validates phone number format

```mermaid
classDiagram
    class PhoneNumber {
        +string CountryCode
        +string Number
        +Create(phoneNumber)
    }
```

### 9. Rating

- Represents a single rating value
- Provides static methods for common ratings (Poor, Fair, Good, etc.)

```mermaid
classDiagram
    class Rating {
        +int Value
        +Create(value)
        +static Rating Poor()
        +static Rating Fair()
        +static Rating Good()
        +static Rating VeryGood()
        +static Rating Excellent()
    }
```

### 10. Schedule

- Represents a time period with a start date and an optional end date
- Supports open-ended schedules

```mermaid
classDiagram
    class Schedule {
        +DateOnly StartDate
        +DateOnly? EndDate
        +bool IsOpenEnded
        +Create(startDate, endDate)
        +IsActive(date)
        +OverlapsWith(Schedule)
    }
```

### 11. TenantId

- Represents a unique identifier for a tenant
- Implements the AggregateRootId<Guid> class

```mermaid
classDiagram
    class TenantId {
        +Guid Value
        +bool IsEmpty
        +Create()
        +Create(Guid)
        +Create(string)
    }
```

### 12. Url

- Represents a URL (Uniform Resource Locator)
- Supports absolute, relative, and local URLs

```mermaid
classDiagram
    class Url {
        +string Value
        +UrlType Type
        +Create(url)
        +IsAbsolute()
        +IsRelative()
        +IsLocal()
        +ToAbsolute(baseUrl)
    }
```

### 13. VatNumber

- Represents a VAT (Value Added Tax) or EIN (Employer Identification Number)
- Supports country-specific formatting

```mermaid
classDiagram
    class VatNumber {
        +string CountryCode
        +string Number
        +Create(vatNumber)
        +IsValid()
    }
```

### 14. Website

- Represents a website address
- Normalizes and validates website URLs

```mermaid
classDiagram
    class Website {
        +string Value
        +Create(website)
    }
```

## Usage

These domain layer components are designed to be immutable and self-validating, ensuring that domain
logic is consistently applied across the entire system.

Example usage:

```csharp
var address = Address.Create("John Doe", "123 Main St", null, "12345", "Anytown", "USA");
var averageRating = AverageRating.Create(4.5, 10);
var currency = Currency.USDollar;
var email = EmailAddress.Create("user@example.com");
var color = HexColor.Create("#FF5733");
var money = Money.Create(100.50m, Currency.Euro);
var name = PersonFormalName.Create(new[] { "John", "Doe" }, "Mr.", "Jr.");
var phone = PhoneNumber.Create("+1234567890");
var rating = Rating.Create(4);
var schedule = Schedule.Create(new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31));
var url = Url.Create("https://example.com");
var vatNumber = VatNumber.Create("DE123456789");
var website = Website.Create("www.example.com");
```