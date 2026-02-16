using Microsoft.Extensions.AI;
using System.Net.Http.Json;

namespace IndianaChatBot.Services;

/// <summary>
/// Agent service that integrates with Microsoft Foundry hosted agent
/// and uses Bing Custom Search for grounding
/// </summary>
public class AgentService : IAgentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<AgentService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ChatResponse> GetResponseAsync(string message)
    {
        try
        {
            // Get configuration for Microsoft Foundry and Bing Search
            var foundryEndpoint = _configuration["FoundryAgent:Endpoint"] ?? string.Empty;
            var foundryApiKey = _configuration["FoundryAgent:ApiKey"] ?? string.Empty;
            var bingSearchKey = _configuration["BingSearch:ApiKey"] ?? string.Empty;
            var bingCustomConfigId = _configuration["BingSearch:CustomConfigId"] ?? string.Empty;

            // Step 1: Get grounding information from Bing Custom Search (if configured)
            string groundingContext = string.Empty;
            string? sourceUrl = null;

            if (!string.IsNullOrEmpty(bingSearchKey) && !string.IsNullOrEmpty(bingCustomConfigId))
            {
                var searchResult = await PerformBingSearchAsync(message, bingSearchKey, bingCustomConfigId);
                groundingContext = searchResult.Context;
                sourceUrl = searchResult.Url;
            }

            // Step 2: Call Microsoft Foundry Agent with grounding context
            string agentResponse;
            
            if (!string.IsNullOrEmpty(foundryEndpoint) && !string.IsNullOrEmpty(foundryApiKey))
            {
                agentResponse = await CallFoundryAgentAsync(message, groundingContext, foundryEndpoint, foundryApiKey);
            }
            else
            {
                // Fallback to a simple response if Foundry is not configured
                agentResponse = GenerateFallbackResponse(message, groundingContext);
            }

            return new ChatResponse
            {
                Response = agentResponse,
                Source = sourceUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent response");
            return new ChatResponse
            {
                Response = "I apologize, but I encountered an error processing your request. Please try again."
            };
        }
    }

    private async Task<(string Context, string? Url)> PerformBingSearchAsync(
        string query, 
        string apiKey, 
        string customConfigId)
    {
        try
        {
            // Bing Custom Search API endpoint
            var searchUrl = $"https://api.bing.microsoft.com/v7.0/custom/search?q={Uri.EscapeDataString(query)}&customconfig={customConfigId}&count=3";

            var request = new HttpRequestMessage(HttpMethod.Get, searchUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var searchResult = await response.Content.ReadFromJsonAsync<BingSearchResult>();
                
                if (searchResult?.WebPages?.Value != null && searchResult.WebPages.Value.Length > 0)
                {
                    var topResult = searchResult.WebPages.Value[0];
                    var context = $"Based on search results: {topResult.Snippet}";
                    return (context, topResult.Url);
                }
            }

            return (string.Empty, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error performing Bing search");
            return (string.Empty, null);
        }
    }

    private async Task<string> CallFoundryAgentAsync(
        string message, 
        string groundingContext, 
        string endpoint, 
        string apiKey)
    {
        try
        {
            // Construct the prompt with grounding context
            var prompt = string.IsNullOrEmpty(groundingContext) 
                ? message 
                : $"{groundingContext}\n\nUser question: {message}";

            // Create the request for Microsoft Foundry Agent
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("api-key", apiKey);
            
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant. Use the provided context to answer questions accurately." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
                temperature = 0.7
            };

            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FoundryResponse>();
                return result?.Choices?[0]?.Message?.Content ?? "I couldn't generate a response.";
            }

            _logger.LogWarning("Foundry API call failed with status: {StatusCode}", response.StatusCode);
            return GenerateFallbackResponse(message, groundingContext);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling Foundry agent");
            return GenerateFallbackResponse(message, groundingContext);
        }
    }

    private string GenerateFallbackResponse(string message, string groundingContext)
    {
        // Provide a helpful response indicating the service is in demo mode
        if (!string.IsNullOrEmpty(groundingContext))
        {
            return $"Based on available information: {groundingContext}\n\n" +
                   $"To get AI-powered responses, please configure your Microsoft Foundry agent credentials in appsettings.json.";
        }

        return "Hello! I'm your AI assistant. To enable full functionality, please configure:\n\n" +
               "1. Microsoft Foundry Agent endpoint and API key\n" +
               "2. Bing Custom Search API key and custom config ID\n\n" +
               "These settings should be added to your appsettings.json file.";
    }

    #region API Response Models

    private class BingSearchResult
    {
        public WebPages? WebPages { get; set; }
    }

    private class WebPages
    {
        public WebPage[]? Value { get; set; }
    }

    private class WebPage
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }

    private class FoundryResponse
    {
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string Content { get; set; } = string.Empty;
    }

    #endregion
}
