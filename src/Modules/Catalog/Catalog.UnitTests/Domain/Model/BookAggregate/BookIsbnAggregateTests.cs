namespace BridgingIT.DevKit.Examples.BookStore.Catalog.UnitTests.Domain;

using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class BookIsbnAggregateTests
{
    [Theory]
    [InlineData("ISBN 978-3-16-148410-0", true, "978-3-16-148410-0", "ISBN-13")]
    [InlineData("978-3-16-148410-0", true, "978-3-16-148410-0", "ISBN-13")]
    [InlineData("3-16-148410-X", true, "3-16-148410-X", "ISBN-10")]
    [InlineData(" 3-16-148410-X ", true, "3-16-148410-X", "ISBN-10")]
    [InlineData("ISBN-10 0-306-40615-2", true, "0-306-40615-2", "ISBN-10")]
    [InlineData("0 471 48648 5", true, "0 471 48648 5", "ISBN-10")]
    [InlineData("0471486485", true, "0471486485", "ISBN-10")]
    [InlineData("ISBN-13 978 0 306 40615 7", true, "978 0 306 40615 7", "ISBN-13")]
    [InlineData("123456789X", true, "123456789X", "ISBN-10")]
    [InlineData("9783161484100", true, "9783161484100", "ISBN-13")]
    [InlineData("ISBN 978-3-16-148410-0-0", false, null, null)] // Invalid ISBN-13
    //[InlineData("ISBN 1234567890123456", false, null, null)] // Invalid ISBN-13
    //[InlineData("ISBN 978316", false, null, null)] // Invalid ISBN-10
    [InlineData("ISBN 978-3-16", false, null, null)] // Invalid ISBN
    [InlineData("Invalid ISBN", false, null, null)] // Non-numeric
    [InlineData(null, false, null, null)]
    public void Create_WithIsbn_CreatesValidInstance(string isbnValue, bool expectedIsValid, string expectedIsbnValue, string expectedTypeValue)
    {
        // Arrange
        if (expectedIsValid)
        {
            // Act
            var sut = BookIsbn.Create(isbnValue);

            // Assert
            sut.Value.ShouldBe(expectedIsbnValue);
            sut.Type.ShouldBe(expectedTypeValue);
        }
        else
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() =>
                BookIsbn.Create(isbnValue));
        }
    }
}