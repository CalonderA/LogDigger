using CacheApi.Models;
using CacheApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CacheApi.Controllers;

[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly CacheService _cacheService;

    public CacheController(CacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpPost("set")]
    public IActionResult Set([FromBody] SetRequest request)
    {
        try
        {
            var response = _cacheService.Set(request.Key, request.Value, request.TtlSeconds);
            return StatusCode(201, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("get/{key}")]
    public IActionResult Get(string key)
    {
        var entry = _cacheService.Get(key);
        if (entry == null)
        {
            return NotFound(new { error = "Key not found or expired." });
        }
        return Ok(entry);
    }

    [HttpDelete("{key}")]
    public IActionResult Delete(string key)
    {
        var deleted = _cacheService.Delete(key);
        if (!deleted)
        {
            return NotFound(new { error = "Key not found." });
        }
        return NoContent();
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var stats = _cacheService.GetStats();
        return Ok(stats);
    }
}
