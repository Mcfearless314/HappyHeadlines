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
    public void Get()
    {
        _database.GetProfanityWords();
    }

    [HttpGet("check")]
    public bool CheckProfanity([FromQuery] string text)
    {
        return _database.ContainsProfanity(text);
    }
}