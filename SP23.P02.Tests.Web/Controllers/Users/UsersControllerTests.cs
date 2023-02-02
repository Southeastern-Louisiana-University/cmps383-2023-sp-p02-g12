using System.Net;
using FluentAssertions;
using SP23.P02.Tests.Web.Controllers.Authentication;
using SP23.P02.Tests.Web.Dtos;
using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web.Controllers.Users;

[TestClass]
public class UsersControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task CreateUser_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = GetNewUser();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect POST /api/users without being logged in to be rejected with an HTTP 401");
    }

    [TestMethod]
    public async Task CreateUser_EmptyUsername_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.UserName = "";

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with an empty username to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_DuplicateUsername_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.UserName = "bob";

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with a duplicate username to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_NoSuchRole_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.Roles = new[]{"NotARole"};

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with an invalid role to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_EmptyRole_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.Roles = Array.Empty<string>();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with an empty role list to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_NoPassword_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.Password = string.Empty;

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with no password to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_BadPassword_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();
        target.Password = "password";

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/users with a bad password to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task CreateUser_ValidUserWhileLoggedIn_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewUser();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect POST /api/users with valid data and logged in as an admin to work");
        var createdUser = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        createdUser.Should().NotBeNull("we expect POST /api/users to return a UserDto after a user was created");
        createdUser.Should().BeEquivalentTo(new
        {
            target.UserName, target.Roles
        }, "we expect the created user to match what was sent");

        var loginAsThatUser = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
        {
            UserName = target.UserName,
            Password = target.Password
        });
        var loginResult = await loginAsThatUser.AssertLoginFunctions();
        loginResult.Should().BeEquivalentTo(createdUser, "we expect our created user to match our login");
    }

    [TestMethod]
    public async Task CreateUser_ValidAdminWhileLoggedIn_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }
        var target = GetNewAdmin();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/users", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect POST /api/users with valid data and logged in as an admin to work");
        var createdUser = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        createdUser.Should().NotBeNull("we expect POST /api/users to return a UserDto after an admin was created");
        createdUser.Should().BeEquivalentTo(new
        {
            target.UserName,
            target.Roles
        }, "we expect the created admin to match what was sent");

        var loginAsThatUser = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
        {
            UserName = target.UserName,
            Password = target.Password
        });
        var loginResult = await loginAsThatUser.AssertLoginFunctions();
        loginResult.Should().BeEquivalentTo(createdUser, "we expect our created user to match our login");
    }

    private static CreateUserDto GetNewUser()
    {
        return new CreateUserDto
        {
            UserName = Guid.NewGuid().ToString("N"),
            Password = Guid.NewGuid().ToString("N") + "aSd!@#",
            Roles = new[] { "User" }
        };
    }

    private static CreateUserDto GetNewAdmin()
    {
        return new CreateUserDto
        {
            UserName = Guid.NewGuid().ToString("N"),
            Password = Guid.NewGuid().ToString("N") + "aSd!@#",
            Roles = new[] { "Admin" }
        };
    }
}
