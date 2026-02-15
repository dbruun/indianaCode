namespace IndianaChatBot.Services;

public interface IAgentService
{
    Task<ChatResponse> GetResponseAsync(string message);
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public string? Source { get; set; }
}
