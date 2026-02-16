# IndianaChatBot - Setup Guide

## Quick Start

### 1. Prerequisites
- .NET 10 SDK installed
- Azure AI Foundry project with a deployed agent
- Azure CLI (for authentication)

### 2. Configure Your Agent

Before running the application, you need to:

1. **Set up your agent in Azure AI Foundry**:
   - Go to https://ai.azure.com
   - Create a new project or use an existing one
   - Create or configure an agent
   - Add any tools or connections (like Bing Custom Search) to your agent in Foundry
   - Note your project endpoint URL and agent ID

2. **Configure appsettings.json**:
   
   Open `IndianaChatBot/appsettings.json` and update:

   ```json
   {
     "FoundryAgent": {
       "Endpoint": "https://your-foundry-resource.services.ai.azure.com/api/projects/your-project",
       "AgentId": "your-agent-id"
     }
   }
   ```

   Replace:
   - `your-foundry-resource`: Your Azure AI Foundry resource name
   - `your-project`: Your project name
   - `your-agent-id`: The ID of your agent (found in Foundry portal)

3. **Authenticate with Azure**:
   
   ```bash
   az login
   ```

   The application uses `DefaultAzureCredential` which will automatically use your Azure CLI credentials for local development.

### 3. Run the Application

```bash
cd IndianaChatBot/IndianaChatBot
dotnet run
```

The application will start at `https://localhost:5001` (or the URL shown in console).

### 4. Using the Chatbot

1. Open your browser to the application URL
2. Click the purple chat button (💬) in the bottom-right corner
3. Type your message and press Enter or click Send
4. The bot will respond using your configured Foundry agent

## Azure AI Foundry Setup

### Creating an Agent

1. Navigate to [Azure AI Foundry](https://ai.azure.com)
2. Create a new project or select an existing one
3. Go to the "Agents" section
4. Click "Create Agent"
5. Configure your agent:
   - Choose a model (e.g., GPT-4)
   - Add instructions for the agent's behavior
   - Add tools or connections (e.g., Bing Custom Search)
6. Save and deploy your agent
7. Copy the agent ID from the agent details page

### Getting Your Endpoint URL

The endpoint URL follows this format:
```
https://<resource-name>.services.ai.azure.com/api/projects/<project-name>
```

You can find this in:
1. Azure AI Foundry portal
2. Azure portal under your AI Foundry resource
3. The "Settings" or "Keys and Endpoint" section of your project

## Authentication

The application uses Azure `DefaultAzureCredential` which supports multiple authentication methods:

### Local Development
- Sign in with Azure CLI: `az login`
- Or use Visual Studio credentials
- Or use Azure PowerShell credentials

### Production Deployment
- Use Managed Identity when deployed to Azure App Service or Azure Container Apps
- Or configure environment variables with service principal credentials

## Project Structure

```
IndianaChatBot/
├── IndianaChatBot/                  # Server project (.NET Web API)
│   ├── Controllers/
│   │   └── ChatController.cs        # API endpoint
│   ├── Services/
│   │   ├── IAgentService.cs
│   │   └── AgentService.cs          # Foundry SDK integration
│   ├── Components/                  # Server Blazor components
│   └── appsettings.json             # Configuration
│
└── IndianaChatBot.Client/           # Client WebAssembly project
    ├── Components/
    │   ├── ChatBot.razor            # Chatbot UI
    │   └── ChatBot.razor.css        # Chatbot styles
    └── Program.cs
```

## Features

- ✨ Beautiful gradient-styled chat interface
- 💬 Real-time messaging with typing indicators
- 🤖 AI-powered responses via Microsoft Foundry agents
- 🔒 Secure Azure authentication
- 📱 Responsive design
- ⌨️ Keyboard shortcuts (Enter to send)
- 🎨 Smooth animations and transitions

## Troubleshooting

### Issue: Authentication errors
**Solution:** Ensure you're logged in with Azure CLI (`az login`) and have access to the Foundry resource.

### Issue: "Agent not configured" message
**Solution:** Verify that `Endpoint` and `AgentId` are correctly set in appsettings.json.

### Issue: Chat button not appearing
**Solution:** Ensure both projects built successfully. Check browser console for errors.

### Issue: Build errors
**Solution:** Ensure .NET 10 SDK is installed with `dotnet --version`.

### Issue: Agent responses are slow
**Solution:** This is normal - the agent needs time to process. The application polls for completion every 500ms.

## Development

To modify the chatbot:

- **UI changes:** Edit `IndianaChatBot.Client/Components/ChatBot.razor`
- **Styling:** Edit `IndianaChatBot.Client/Components/ChatBot.razor.css`
- **Agent integration:** Modify `IndianaChatBot/Services/AgentService.cs`
- **API:** Update `IndianaChatBot/Controllers/ChatController.cs`

## Building for Production

```bash
dotnet publish -c Release -o ./publish
```

The published files will be in the `./publish` directory.

## Security Notes

- Uses Azure DefaultAzureCredential (no API keys in code)
- Use Managed Identity in production deployments
- Never commit credentials to source control
- Consider implementing rate limiting for public deployments

## Support

For issues or questions, refer to the main README.md or create an issue in the repository.
