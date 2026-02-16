using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using OpenAI.Responses;

// Suppress OPENAI001 warning for using preview OpenAI API features
// This is required because we're using the new Responses API which is still in preview
#pragma warning disable OPENAI001

namespace IndianaChatBot.Services;

/// <summary>
/// Agent service that connects to a Microsoft Foundry hosted agent
/// using the Azure AI Projects SDK
/// </summary>
public class AgentService : IAgentService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private AIProjectClient? _projectClient;
    private ProjectResponsesClient? _responsesClient;
    private string? _agentName;
    private bool _disposed;

    public AgentService(
        IConfiguration configuration,
        ILogger<AgentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatResponse> GetResponseAsync(string message)
    {
        try
        {
            // Initialize the agent client if not already done
            await EnsureAgentClientInitializedAsync();

            if (_responsesClient == null)
            {
                return new ChatResponse
                {
                    Response = "Agent not configured. Please configure your Microsoft Foundry endpoint and agent name in appsettings.json."
                };
            }

            // Use the agent to generate a response
            ResponseResult response = await _responsesClient.CreateResponseAsync(message);
            
            string agentResponse = response.GetOutputText();

            return new ChatResponse
            {
                Response = agentResponse ?? "No response from agent."
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

    private async Task EnsureAgentClientInitializedAsync()
    {
        if (_responsesClient != null)
        {
            return; // Already initialized
        }

        // Use semaphore to ensure thread-safe initialization
        await _initializationLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_responsesClient != null)
            {
                return;
            }

            // Get configuration for Microsoft Foundry
            var foundryEndpoint = _configuration["FoundryAgent:Endpoint"];
            _agentName = _configuration["FoundryAgent:AgentName"];

            if (string.IsNullOrEmpty(foundryEndpoint) || string.IsNullOrEmpty(_agentName))
            {
                _logger.LogWarning("Foundry agent not configured. Please set FoundryAgent:Endpoint and FoundryAgent:AgentName in appsettings.json");
                return;
            }

            // Create the project client with Azure authentication
            // DefaultAzureCredential will use various credential sources in order:
            // 1. Environment variables
            // 2. Managed Identity (when deployed to Azure)
            // 3. Visual Studio
            // 4. Azure CLI
            // 5. Azure PowerShell
            var credential = new DefaultAzureCredential();
            
            _projectClient = new AIProjectClient(
                endpoint: new Uri(foundryEndpoint), 
                tokenProvider: credential
            );

            // Get the agent by name
            var agentResult = await _projectClient.Agents.GetAgentAsync(_agentName);
            var agentRecord = agentResult.Value;
            _logger.LogInformation("Successfully retrieved agent: {AgentName} (id: {AgentId})", agentRecord.Name, agentRecord.Id);

            // Get the responses client for this agent
            _responsesClient = _projectClient.OpenAI.GetProjectResponsesClientForAgent(agentRecord);
            
            _logger.LogInformation("Successfully initialized Foundry agent client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Foundry agent client");
            throw;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _initializationLock?.Dispose();
        _disposed = true;
    }
}
