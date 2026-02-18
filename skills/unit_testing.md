---
name: Unit Testing Standards
description: Standards and patterns for writing unit tests in the HR Certification Portal.
---

# Unit Testing Standards

All new features must include unit tests. We use **xUnit**, **Moq**, and **FluentAssertions**.

## Project Structure
- Tests are located in `SeHrCertificationPortal.Tests` (Need to create this project if it doesn't exist).

## Naming Convention
- Project: `SeHrCertificationPortal.Tests`
- Classes: `[ClassName]Tests`
- Methods: `[MethodName]_[Scenario]_[ExpectedResult]`

## Pattern (AAA)
- **Arrange**: Setup mocks and data.
- **Act**: Call the method under test.
- **Assert**: Verify the result using FluentAssertions.

## Example
```csharp
public class CertificationServiceTests
{
    [Fact]
    public void AddCertification_ValidData_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<ICertificationRepository>();
        var service = new CertificationService(mockRepo.Object);
        var cert = new Certification { Name = "AWS" };

        // Act
        var result = service.AddCertification(cert);

        // Assert
        result.Should().BeTrue();
        mockRepo.Verify(r => r.Add(It.IsAny<Certification>()), Times.Once);
    }
}
```
