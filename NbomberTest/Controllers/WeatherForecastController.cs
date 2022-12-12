using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace NbomberTest.Controllers;

[ApiController]
[Route("api")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IMemoryCache _memoryCache;
    
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }
    
    private IQueryable<WeatherForecast> WeatherForecasts => Enumerable.Range(1, 1000000).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddSeconds(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .AsQueryable();

    [HttpGet("Get")]
    public IActionResult Get()
    {
        return Ok(WeatherForecasts);
    }
    
    [HttpGet("GetCached")]
    public IActionResult GetCaching()
    {
        string weatherForecastKey = "weatherForecastKey";
        
        IEnumerable<WeatherForecast> weatherForecastCollection = null;
        
        // If found in cache, return cached data
        if (_memoryCache.TryGetValue(weatherForecastKey, out weatherForecastCollection))
        {
            return Ok(weatherForecastCollection);
        }

        // If not found, then calculate response
        weatherForecastCollection = WeatherForecasts;
        
        // Set cache options
        var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(3))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(20));
            
        // Set object in cache
        _memoryCache.Set(weatherForecastKey, weatherForecastCollection, cacheOptions);
        return Ok(weatherForecastCollection);
    }
    
    [HttpGet("GetPaged")]
    public IActionResult GetPaged([FromQuery] PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
        var pagedData = WeatherForecasts
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToList();
        return Ok(pagedData);
    }
    
    [HttpGet("GetPagedCache")]
    public IActionResult GetPagedCache([FromQuery] PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

        string weatherForecastKey = $"weatherForecastKey-{validFilter.PageNumber}-{validFilter.PageSize}";
        
        IEnumerable<WeatherForecast> weatherForecastCollection = null;
        
        // If found in cache, return cached data
        if (_memoryCache.TryGetValue(weatherForecastKey, out weatherForecastCollection))
        {
            return Ok(weatherForecastCollection);
        }

        // If not found, then calculate response
        weatherForecastCollection = WeatherForecasts
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToList();
        
        // Set cache options
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(3))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(20));
            
        // Set object in cache
        _memoryCache.Set(weatherForecastKey, weatherForecastCollection, cacheOptions);
        return Ok(weatherForecastCollection);
    }
}

public class Response<T>
{
    public Response()
    {
    }
    
    public Response(T data)
    {
        Succeeded = true;
        Message = string.Empty;
        Errors = null;
        Data = data;
    }
    public T Data { get; set; }
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; }
    public string Message { get; set; }
}

public class PagedResponse<T> : Response<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public Uri FirstPage { get; set; }
    public Uri LastPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public Uri NextPage { get; set; }
    public Uri PreviousPage { get; set; }
    public PagedResponse(T data, int pageNumber, int pageSize)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Data = data;
        this.Message = null;
        this.Succeeded = true;
        this.Errors = null;
    }
}

public class PaginationFilter
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public PaginationFilter()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
    public PaginationFilter(int pageNumber, int pageSize)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize > 10 ? 10 : pageSize;
    }
}