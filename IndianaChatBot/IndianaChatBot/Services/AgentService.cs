using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace IndianaChatBot.Services;

/// <summary>
/// Agent service that connects to a Microsoft Foundry hosted agent
/// using the Agentic Framework SDK
/// </summary>
public class AgentService : IAgentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentService> _logger;
    private PersistentAgentsClient? _agentClient;
    private string? _agentId;

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

            if (_agentClient == null || string.IsNullOrEmpty(_agentId))
            {
                return new ChatResponse
                {
                    Response = "Agent not configured. Please configure your Microsoft Foundry endpoint and agent ID in appsettings.json."
                };
            }

            // Get the agent
            var agentResponse = await _agentClient.Administration.GetAgentAsync(_agentId);
            var agent = agentResponse.Value;

            // Create a new thread for this conversation
            var threadResponse = await _agentClient.Threads.CreateThreadAsync();
            var thread = threadResponse.Value;

            // Add the user's message to the thread
            await _agentClient.Messages.CreateMessageAsync(
                thread.Id,
                MessageRole.User,
                message
            );

            // Run the agent on the thread
            var runResponse = await _agentClient.Runs.CreateRunAsync(thread.Id, agent.Id);
            var run = runResponse.Value;

            // Poll for completion
            while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
            {
                await Task.Delay(500);
                var updatedRunResponse = await _agentClient.Runs.GetRunAsync(thread.Id, run.Id);
                run = updatedRunResponse.Value;
            }

            // Check if the run completed successfully
            if (run.Status != RunStatus.Completed)
            {
                _logger.LogWarning("Agent run did not complete successfully. Status: {Status}", run.Status);
                return new ChatResponse
                {
                    Response = $"The agent encountered an issue. Status: {run.Status}"
                };
            }

            // Get the messages from the thread (newest first)
            var messages = _agentClient.Messages.GetMessagesAsync(
                threadId: thread.Id,
                order: ListSortOrder.Descending
            );

            // Find the first assistant message (which should be the response)
            await foreach (var msg in messages)
            {
                if (msg.Role == MessageRole.Agent)
                {
                    // Extract text content from the message
                    foreach (var content in msg.ContentItems)
                    {
                        if (content is MessageTextContent textContent)
                        {
                            return new ChatResponse
                            {
                                Response = textContent.Text
                            };
                        }
                    }
                }
            }

            return new ChatResponse
            {
                Response = "No response from agent."
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
        if (_agentClient != null)
        {
            return; // Already initialized
        }

        // Get configuration for Microsoft Foundry
        var foundryEndpoint = _configuration["FoundryAgent:Endpoint"];
        _agentId = _configuration["FoundryAgent:AgentId"];

        if (string.IsNullOrEmpty(foundryEndpoint) || string.IsNullOrEmpty(_agentId))
        {
            _logger.LogWarning("Foundry agent not configured. Please set FoundryAgent:Endpoint and FoundryAgent:AgentId in appsettings.json");
            return;
        }

        try
        {
            // Create the agent client with Azure authentication
            // DefaultAzureCredential will use various credential sources in order:
            // 1. Environment variables
            // 2. Managed Identity (when deployed to Azure)
            // 3. Visual Studio
            // 4. Azure CLI
            // 5. Azure PowerShell
            var credential = new DefaultAzureCredential();
            
            _agentClient = new PersistentAgentsClient(foundryEndpoint, credential);
            _logger.LogInformation("Successfully initialized Foundry agent client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Foundry agent client");
            throw;
        }
    }
}
