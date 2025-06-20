using DiscountServer;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscountCodeServer.Tests;


public class DiscountServiceTests
{
    [Fact]
    public void GenerateCodes_ShouldGenerateUniqueCodes()
    {
        // Arrange
        var storageMock = new Mock<ICodeStorage>();
        storageMock.Setup(s => s.LoadCodes()).Returns(new HashSet<string>());
        var loggerMock = new Mock<ILogger<DiscountService>>();
        var service = new DiscountService(storageMock.Object, loggerMock.Object);

        // Act
        var result = service.GenerateCodes(10, 7);
        var codes = service.GetAllCodes();

        // Assert
        Assert.True(result);
        Assert.Equal(10, codes.Count);
        Assert.Equal(10, new HashSet<string>(codes).Count); // All unique
    }

    [Fact]
    public void UseCode_ShouldRemoveCode()
    {
        // Arrange
        var storageMock = new Mock<ICodeStorage>();
        storageMock.Setup(s => s.LoadCodes()).Returns(new HashSet<string>());
        var loggerMock = new Mock<ILogger<DiscountService>>();
        var service = new DiscountService(storageMock.Object, loggerMock.Object);
        service.GenerateCodes(1, 7);
        var code = service.GetAllCodes()[0];

        // Act
        var used = service.UseCode(code);
        var stillExists = service.GetAllCodes().Contains(code);

        // Assert
        Assert.True(used);
        Assert.False(stillExists);
    }

    [Fact]
    public void GenerateCodes_ShouldNotReturnDuplicates_WhenStorageHasExistingCodes()
    {
        // Arrange
        var existingCode = "ABCDEFG";
        var storageMock = new Mock<ICodeStorage>();
        storageMock.Setup(s => s.LoadCodes()).Returns(new HashSet<string> { existingCode });
        var loggerMock = new Mock<ILogger<DiscountService>>();
        var service = new DiscountService(storageMock.Object, loggerMock.Object);

        // Act
        var result = service.GenerateCodes(5, 7);
        var codes = service.GetAllCodes();

        // Assert
        Assert.True(result);
        Assert.Contains(existingCode, codes); // Existing code should still be present
        Assert.Equal(6, codes.Count); // 5 new + 1 existing
        Assert.Equal(6, new HashSet<string>(codes).Count); // All unique
    }
}