using Microsoft.AspNetCore.Mvc;

namespace ProfanityService.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfanityController : ControllerBase
{
    private readonly Database _database;
    public ProfanityController(Database database)
    {
        _database = database;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var profanityWords = _database.GetProfanityWords();
        return Ok(profanityWords);
    }

    [HttpGet("check")]
    public async Task<bool> CheckProfanity([FromQuery] string text)
    {
        return await _database.ContainsProfanityAsync(text);
    }
}