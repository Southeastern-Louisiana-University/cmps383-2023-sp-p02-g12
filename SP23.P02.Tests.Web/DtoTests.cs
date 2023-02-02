using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web;

[TestClass]
public class DtoTests
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
    public async Task EnsureOnlyDtosAreUsedInApi()
    {
        var webClient = context.GetStandardWebClient();
        var httpResponse = await webClient.GetAsync("/swagger/v1/swagger.json");
        var apiSpec = await httpResponse.Content.ReadAsJsonAsync<OpenApiSpec>();

        if (apiSpec == null)
        {
            Assert.Fail("Swagger should be working");
            return;
        }
        var violations = apiSpec.Components.Schemas.Where(x => !x.Key.EndsWith("Dto")).Select(x => x.Key).ToList();
        Assert.IsTrue(violations.Count == 0, $"You have entities being sent in your API:\r\n{string.Join("\r\n", violations)}");
    }

    public class Schema
    {
        public string? Type { get; set; }
    }

    public class Components
    {
        public Dictionary<string, Schema> Schemas { get; set; } = new();
    }

    public class OpenApiSpec
    {
        public Components Components { get; set; } = new();
    }
}
