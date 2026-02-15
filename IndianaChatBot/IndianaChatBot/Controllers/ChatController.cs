using Microsoft.AspNetCore.Mvc;
using IndianaChatBot.Services;

namespace IndianaChatBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAgentService agentService, ILogger<ChatController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            var response = await _agentService.GetResponseAsync(request.Message);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }
}

public record ChatRequest(string Message);
