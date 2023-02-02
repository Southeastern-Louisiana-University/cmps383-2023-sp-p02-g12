using System.Net;
using FluentAssertions;
using SP23.P02.Tests.Web.Controllers.Authentication;
using SP23.P02.Tests.Web.Dtos;
using SP23.P02.Tests.Web.Helpers;

namespace SP23.P02.Tests.Web.Controllers.Stations;

internal static class TrainStationsHelpers
{
    internal static async Task<IAsyncDisposable?> CreateTrainStation(this HttpClient webClient, TrainStationDto request)
    {
        try
        {
            await webClient.AssertLoggedInAsAdmin();
            var httpResponse = await webClient.PostAsJsonAsync("/api/stations", request);
            var resultDto = await AssertCreateTrainStationFunctions(httpResponse, request, webClient);
            await webClient.AssertLoggedOut();
            request.Id = resultDto.Id;
            return new DeleteTrainStation(resultDto, webClient);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<List<TrainStationDto>?> GetTrainStations(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/stations");
            var getAllResult = await AssertTrainStationListAllFunctions(getAllRequest);
            return getAllResult.ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<TrainStationDto?> GetTrainStation(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/stations");
            var getAllResult = await AssertTrainStationListAllFunctions(getAllRequest);
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task AssertTrainStationUpdateFunctions(this HttpResponseMessage httpResponse, TrainStationDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling PUT /api/stations/{id} with valid data to update a station");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TrainStationDto>();
        resultDto.Should().BeEquivalentTo(request, "We expect the update station endpoint to return the result");

        var getByIdResult = await webClient.GetAsync($"/api/stations/{request.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the updated station by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<TrainStationDto>();
        dtoById.Should().BeEquivalentTo(request, "we expect the same result to be returned by an update station call as what you'd get from get station by id");

        var getAllRequest = await webClient.GetAsync("/api/stations");
        var listAllData =  await AssertTrainStationListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/stations");
        listAllData.Should().NotBeEmpty("list all should have something if we just updated a station");
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created station by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated station as what you'd get from get getting all stations");
    }

    internal static async Task<TrainStationDto> AssertCreateTrainStationFunctions(this HttpResponseMessage httpResponse, TrainStationDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/stations with valid data to create a new station");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TrainStationDto>();
        Assert.IsNotNull(resultDto, "We expect json data when calling POST /api/stations");

        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created station to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create station endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/stations/{resultDto.Id}", "we expect the location header to point to the get station by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/stations/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created station by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<TrainStationDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create station as what you'd get from get station by id");

        var getAllRequest = await webClient.GetAsync("/api/stations");
        var listAllData =  await AssertTrainStationListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/stations");
        listAllData.Should().NotBeEmpty("list all should have something if we just created a station");
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created station by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created station as what you'd get from get getting all stations");

        return resultDto;
    }

    internal static async Task<List<TrainStationDto>> AssertTrainStationListAllFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/stations");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<TrainStationDto>>();
        Assert.IsNotNull(resultDto, "We expect json data when calling GET /api/stations");
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 stations when calling GET /api/stations");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all stations to have names");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all stations to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every station");
        return resultDto;
    }

    private sealed class DeleteTrainStation : IAsyncDisposable
    {
        private readonly TrainStationDto request;
        private readonly HttpClient webClient;

        public DeleteTrainStation(TrainStationDto request, HttpClient webClient)
        {
            this.request = request;
            this.webClient = webClient;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await webClient.DeleteAsync($"/api/stations/{request.Id}");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
