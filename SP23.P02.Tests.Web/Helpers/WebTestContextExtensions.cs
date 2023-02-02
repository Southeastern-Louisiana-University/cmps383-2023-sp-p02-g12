using Microsoft.Extensions.DependencyInjection;

namespace SP23.P02.Tests.Web.Helpers;

public static class WebTestContextExtensions
{
    public static int GetBobUserId(this WebTestContext context)
    {
        return GetUserNameId(context, "bob");
    }

    public static int GetSueUserId(this WebTestContext context)
    {
        return GetUserNameId(context, "sue");
    }

    private static int GetUserNameId(WebTestContext context, string userName)
    {
        using var scope = context.GetServices().CreateScope();

        var dbContext = DataContextTests.GetDataContext(scope);
        if (dbContext == null)
        {
            Assert.Fail("Expected to be able to get DataContext for this test");
        }

        var users = DataContextTests.EnsureSet("User", dbContext);

        var user = users.FirstOrDefault(x => string.Equals(x.UserName, userName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            Assert.Fail($"Expected the user '{userName}' to be seeded as a user");
        }

        return user.Id;
    }
}