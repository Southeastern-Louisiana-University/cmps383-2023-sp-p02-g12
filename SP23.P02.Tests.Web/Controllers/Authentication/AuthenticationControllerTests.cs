using System.Net;
using FluentAssertions;
using SP23.P02.Tests.Web.Dtos;
using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web.Controllers.Authentication;

[TestClass]
public class AuthenticationControllerTests
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
    public async Task Login_BadUsername_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
        {
            UserName = "bob",
            Password = Guid.NewGuid().ToString("N")
        });

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect POST /api/authentication/login with a bad username or password to be rejected with an HTTP 400");
    }

    [TestMethod]
    public async Task Login_AsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
        {
            UserName = "bob",
            Password = AuthenticationHelpers.DefaultUserPassword
        });

        //assert
        var result = await responseMessage.AssertLoginFunctions();
        result.Roles.Should().Contain("User", "we expect the bob login to be in the role User");
    }

    [TestMethod]
    public async Task Login_AsAdmin_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
        {
            UserName = "galkadi",
            Password = AuthenticationHelpers.DefaultUserPassword
        });

        //assert
        var result = await responseMessage.AssertLoginFunctions();
        result.Roles.Should().Contain("Admin", "we expect the galkadi login to be in the role User");
    }

    [TestMethod]
    public async Task Me_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/authentication/me");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect GET /api/authentication/me without being logged in to be rejected with an HTTP 401");
    }

    [TestMethod]
    public async Task Me_LoggedInAsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsBobAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the bob user can login");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync("/api/authentication/me");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect GET /api/authentication/me being logged in as bob return an HTTP 200");
        var data = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        data.Should().NotBeNull("GET /api/authentication/me to return a UserDto");
        data.Should().BeEquivalentTo(currentLogin, "we expect the login and me endpoints to return identical data");
    }

    [TestMethod]
    public async Task Me_LoggedInAsAdmin_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync("/api/authentication/me");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect GET /api/authentication/me being logged in as admin return an HTTP 200");
        var data = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        data.Should().NotBeNull("GET /api/authentication/me to return a UserDto");
        data.Should().BeEquivalentTo(currentLogin, "we expect the login and me endpoints to return identical data");
    }

    [TestMethod]
    public async Task Logout_LoggedInAsAdmin_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var currentLogin = await webClient.LoginAsAdminAsync();
        if (currentLogin == null)
        {
            Assert.Fail("You are not ready for this test - make sure the admin user can login");
            return;
        }

        //act
        var httpResponse = await webClient.PostAsync("/api/authentication/logout", null);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect GET /api/authentication/logout being logged in as admin return an HTTP 200");
        httpResponse.AssertLogoutFunctions();
        var meAfterLogout = await webClient.GetAsync("/api/authentication/me");
        meAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect GET /api/authentication/me to return HTTP 401 after logout");
    }
}
