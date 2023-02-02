using System.Net;
using FluentAssertions;
using SP23.P02.Tests.Web.Dtos;
using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web.Controllers.Authentication;

internal static class AuthenticationHelpers
{
    internal const string DefaultUserPassword = "Password123!";

    internal static async Task<bool> LogoutAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsync("/api/authentication/logout", null);
            AssertLogoutFunctions(responseMessage);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static async Task<UserDto?> LoginAsAdminAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
            {
                UserName = "galkadi",
                Password = DefaultUserPassword
            });
            return await AssertLoginFunctions(responseMessage);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<UserDto?> LoginAsBobAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
            {
                UserName = "bob",
                Password = DefaultUserPassword
            });
            return await AssertLoginFunctions(responseMessage);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<UserDto?> LoginAsSueAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
            {
                UserName = "sue",
                Password = DefaultUserPassword
            });
            return await AssertLoginFunctions(responseMessage);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<UserDto> AssertLoginFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling POST /api/authentication/login when a valid username / password is given");
        httpResponse.Headers.Should().ContainKey("Set-Cookie", "we expect that a login operation will use Set-Cookie to log the user in");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        resultDto.Should().NotBeNull("we expect a UserDto as the result of logging in");
        Assert.IsNotNull(resultDto);
        resultDto.Id.Should().BeGreaterThan(0, "we should have a valid user Id returned after logging in");
        resultDto.UserName.Should().NotBeNullOrEmpty("we should have a valid user name returned after logging in");

        return resultDto;
    }

    internal static void AssertLogoutFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling POST /api/authentication/logout while logged in");
        httpResponse.Headers.Should().ContainKey("Set-Cookie", "we expect that a logout operation will use Set-Cookie to log the user out");
    }

    internal static async Task AssertLoggedOut(this HttpClient webClient)
    {
        if (!await webClient.LogoutAsync())
        {
            Assert.Fail("You are not ready for this test - make sure logout works");
        }
    }

    internal static async Task AssertLoggedInAsSue(this HttpClient webClient)
    {
        if (await webClient.LoginAsSueAsync() == null)
        {
            Assert.Fail("You are not ready for this test - logging as 'sue' (a user) should work first");
        }
    }

    internal static async Task AssertLoggedInAsBob(this HttpClient webClient)
    {
        if (await webClient.LoginAsBobAsync() == null)
        {
            Assert.Fail("You are not ready for this test - logging as 'bob' (a user) should work first");
        }
    }

    internal static async Task AssertLoggedInAsAdmin(this HttpClient webClient)
    {
        if (await webClient.LoginAsAdminAsync() == null)
        {
            Assert.Fail("You are not ready for this test - logging as 'galkadi' (an admin) should work first");
        }
    }
}
