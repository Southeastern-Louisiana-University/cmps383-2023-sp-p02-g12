using System.Net;
using FluentAssertions;
using SP23.P02.Tests.Web.Controllers.Authentication;
using SP23.P02.Tests.Web.Dtos;
using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web.Controllers.Stations;

[TestClass]
public class StationsControllerTests
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
    public async Task ListAllStations_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/stations");

        //assert
        await httpResponse.AssertTrainStationListAllFunctions();
    }

    [TestMethod]
    public async Task GetStationById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTrainStation();
        if (target == null)
        {
            Assert.Fail("Make List All stations work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/stations/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/stations/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TrainStationDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get station by id to return the same data as the list all station endpoint");
    }

    [TestMethod]
    public async Task GetStationById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/stations/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/stations/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateStation_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TrainStationDto
        {
            Address = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/stations with no name");
    }

    [TestMethod]
    public async Task CreateStation_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TrainStationDto
        {
            Name = "a".PadLeft(121, '0'),
            Address = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/stations with a name that is too long");
    }

    [TestMethod]
    public async Task CreateStation_NoAddress_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTrainStation();
        await webClient.AssertLoggedInAsAdmin();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new TrainStationDto
        {
            Name = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/stations with no description");
    }

    [TestMethod]
    public async Task CreateStation_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        await httpResponse.AssertCreateTrainStationFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateStation_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/stations when not logged in");
    }

    [TestMethod]
    public async Task CreateStation_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/stations when logged in as bob");
    }

    [TestMethod]
    public async Task UpdateStation_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/stations/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateStation_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/stations/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateStation_NoAddress_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/stations/{id} with a missing description");
    }

    [TestMethod]
    public async Task UpdateStation_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var sueId = context.GetSueUserId();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = "cool new description";
        request.ManagerId = sueId;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        await httpResponse.AssertTrainStationUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateStation_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/stations/{id} without being logged in");
    }

    [TestMethod]
    public async Task UpdateStation_LoggedInAsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        request.Address = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        await httpResponse.AssertTrainStationUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateStation_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        request.Address = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/stations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/stations/{id} against a station bob manages while logged in as sue");
    }

    [TestMethod]
    public async Task DeleteStation_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Address = "asd",
            Name = "asd"
        };
        await using var itemHandle = await webClient.CreateTrainStation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/stations/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/stations/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteStation_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateTrainStation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/stations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/stations/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteStation_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TrainStationDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateTrainStation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        await webClient.DeleteAsync($"/api/stations/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/stations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/stations/{id} on the same item twice");
    }

    [TestMethod]
    public async Task DeleteStation_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TrainStationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateTrainStation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        //act
        await webClient.DeleteAsync($"/api/stations/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/stations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/stations/{id} against a station bob manages while logged in as sue");
    }
}
