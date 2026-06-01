# Generate Tests

Generate comprehensive C# test cases for controllers, services, or utilities.

## Usage

Use this command when you need to:
- Create Unit tests for new C# code.
- Add missing test coverage to Services.
- Create Integration tests for EF Core or API endpoints.

## Template

Please generate tests for:

**Target**: [Specify file, class, or method]

**Test Type**:
- [ ] Unit tests (Services, Utilities)
- [ ] Integration tests (Controllers, API, Database)
- [ ] UI / E2E tests (Playwright)

**Coverage Goals**:
- [ ] Happy path scenarios
- [ ] Edge cases
- [ ] Error handling & Exceptions
- [ ] Null Argument handling

**Test Framework**:
- Framework: `xUnit`
- Mocking: `Moq`
- Assertions: `FluentAssertions` (or standard `Assert`)

**Requirements**:
- ✅ Use C# strongly typed tests.
- ✅ Follow AAA pattern (Arrange, Act, Assert).
- ✅ Include descriptive test method names (e.g., `MethodName_StateUnderTest_ExpectedBehavior`).
- ✅ Mock external dependencies (`AppDbContext`, `IHttpClientFactory`, external APIs).
- ✅ Test expected exceptions (`Assert.ThrowsAsync`).

## Example

```
Please generate tests for:

**Target**: Services/UserService.cs

**Test Type**:
- [x] Unit tests

**Coverage Goals**:
- [x] Happy path (User created successfully)
- [x] Error handling (Duplicate email throws exception)
- [x] Null handling (Null DTO throws ArgumentNullException)

**Requirements**:
- Use Moq to mock IUserRepository.
- Use xUnit for test runner.
```

## Test Structure Example

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Arrange (Global)
        _userRepoMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepoMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new UserDto { Email = "test@example.com" };
        _userRepoMock.Setup(repo => repo.GetByEmailAsync(dto.Email))
                     .ReturnsAsync((User)null);

        // Act
        var result = await _userService.CreateUserAsync(dto);

        // Assert
        Assert.NotNull(result);
        _userRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }
}
```
