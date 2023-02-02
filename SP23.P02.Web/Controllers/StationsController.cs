using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP23.P02.Web.Data;
using SP23.P02.Web.Features.TrainStations;

namespace SP23.P02.Web.Controllers;

[Route("api/stations")]
[ApiController]
public class StationsController : ControllerBase
{
    private readonly DbSet<TrainStation> stations;
    private readonly DataContext dataContext;

    public StationsController(DataContext dataContext)
    {
        this.dataContext = dataContext;
        stations = dataContext.Set<TrainStation>();
    }

    [HttpGet]
    public IQueryable<TrainStationDto> GetAllStations()
    {
        return GetTrainStationDtos(stations);
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult<TrainStationDto> GetStationById(int id)
    {
        var result = GetTrainStationDtos(stations.Where(x => x.Id == id)).FirstOrDefault();
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public ActionResult<TrainStationDto> CreateStation(TrainStationDto dto)
    {
        if (IsInvalid(dto))
        {
            return BadRequest();
        }

        var station = new TrainStation
        {
            Name = dto.Name,
            Address = dto.Address,
        };
        stations.Add(station);

        dataContext.SaveChanges();

        dto.Id = station.Id;

        return CreatedAtAction(nameof(GetStationById), new { id = dto.Id }, dto);
    }

    [HttpPut]
    [Route("{id}")]
    public ActionResult<TrainStationDto> UpdateStation(int id, TrainStationDto dto)
    {
        if (IsInvalid(dto))
        {
            return BadRequest();
        }

        var station = stations.FirstOrDefault(x => x.Id == id);
        if (station == null)
        {
            return NotFound();
        }

        station.Name = dto.Name;
        station.Address = dto.Address;

        dataContext.SaveChanges();

        dto.Id = station.Id;

        return Ok(dto);
    }

    [HttpDelete]
    [Route("{id}")]
    public ActionResult DeleteStation(int id)
    {
        var station = stations.FirstOrDefault(x => x.Id == id);
        if (station == null)
        {
            return NotFound();
        }

        stations.Remove(station);

        dataContext.SaveChanges();

        return Ok();
    }

    private static bool IsInvalid(TrainStationDto dto)
    {
        return string.IsNullOrWhiteSpace(dto.Name) ||
               dto.Name.Length > 120 ||
               string.IsNullOrWhiteSpace(dto.Address);
    }

    private static IQueryable<TrainStationDto> GetTrainStationDtos(IQueryable<TrainStation> stations)
    {
        return stations
            .Select(x => new TrainStationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
            });
    }
}