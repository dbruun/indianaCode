# indianaCode

## Indiana ChatBot - .NET 10 SPA with AI Agent

A Single Page Application (SPA) built with .NET 10 Blazor WebAssembly featuring an AI-powered chatbot that connects to Microsoft Foundry hosted agents using the Azure AI Agents SDK.

### Features

- 🎨 **Modern SPA**: Built with Blazor WebAssembly for a responsive single-page application experience
- 🤖 **AI-Powered Chat**: Integrated with Microsoft Foundry Agent using Azure AI Agents SDK
- 🔒 **Secure Authentication**: Uses Azure DefaultAzureCredential for seamless authentication
- 💬 **Interactive UI**: Beautiful chatbot interface with typing indicators and smooth animations
- ⚡ **Real-time**: WebAssembly-based client with API backend for fast responses

### Technology Stack

- **.NET 10**: Latest .NET framework
- **Blazor WebAssembly**: Client-side SPA framework
- **Azure.AI.Projects**: Azure AI Projects SDK for connecting to Foundry agents
- **Azure.AI.Projects.OpenAI**: OpenAI integration for Azure AI Projects
- **Azure.Identity**: Azure authentication
- **ASP.NET Core Web API**: Backend API for agent communication

### Getting Started

#### Prerequisites

- .NET 10 SDK
- Azure AI Foundry project with a deployed agent
- Azure CLI (for authentication) or other Azure credential method
- Agent ID from your Foundry project

#### Configuration

1. **Clone the repository**
   ```bash
   git clone https://github.com/dbruun/indianaCode.git
   cd indianaCode/IndianaChatBot
   ```

2. **Set up your Foundry Agent**
   
   In Azure AI Foundry:
   - Create a new agent or use an existing one
   - Configure any tools or connections (like Bing Custom Search) within Foundry
   - Note your project endpoint and agent name

3. **Configure the Application**
   
   Edit `IndianaChatBot/appsettings.json`:
   
   ```json
   {
     "FoundryAgent": {
       "Endpoint": "https://your-foundry-resource.services.ai.azure.com/api/projects/your-project",
       "AgentName": "your-agent-name"
     }
   }
   ```

4. **Set up Azure Authentication**
   
   The application uses `DefaultAzureCredential` which will try these methods in order:
   - Environment variables
   - Managed Identity (when deployed to Azure)
   - Visual Studio credentials
   - Azure CLI credentials
   - Azure PowerShell credentials
   
   For local development, sign in with Azure CLI:
   ```bash
   az login
   ```

#### Running the Application

1. **Restore dependencies**
   ```bash
   dotnet restore
   ```

2. **Build the application**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   cd IndianaChatBot
   dotnet run
   ```

4. **Open in browser**
   
   Navigate to `https://localhost:5001` or the URL shown in the console.

### Usage

1. Click the chat button (💬) in the bottom-right corner of the page
2. Type your question in the chat input
3. Press Enter or click "Send"
4. The AI assistant will respond using your configured Foundry agent

### Project Structure

```
IndianaChatBot/
├── IndianaChatBot/                  # Server project
│   ├── Controllers/
│   │   └── ChatController.cs        # API endpoint for chat
│   ├── Services/
│   │   ├── IAgentService.cs         # Service interface
│   │   └── AgentService.cs          # Agent implementation with Foundry SDK
│   ├── Components/                  # Server-side Blazor components
│   ├── appsettings.json             # Configuration file
│   └── Program.cs                   # Server startup
│
└── IndianaChatBot.Client/           # Client WebAssembly project
    ├── Components/
    │   ├── ChatBot.razor            # Main chatbot component
    │   └── ChatBot.razor.css        # Chatbot styles
    ├── Pages/                       # Client pages
    └── Program.cs                   # Client startup
```

### Architecture

The application follows a clean architecture pattern:

1. **Client Layer** (Blazor WebAssembly):
   - ChatBot.razor: Interactive UI component
   - Communicates with backend via HTTP

2. **API Layer** (ASP.NET Core):
   - ChatController: Handles chat requests
   - RESTful endpoint: POST /api/chat

3. **Service Layer**:
   - AgentService: Connects to Foundry agent using Azure AI Projects SDK
   - Uses new Responses API for simplified agent interaction
   - Uses Azure authentication for secure access

### How It Works

1. When a user sends a message, the service connects to the configured agent
2. The service uses the Azure AI Projects SDK to get an agent reference by name
3. The OpenAI Responses API is invoked with the user's message
4. The agent processes the request and returns a response
5. The response is extracted and returned to the user

### Security Considerations

- Uses Azure DefaultAzureCredential for authentication (no API keys in code)
- Never commit credentials to source control
- Use Managed Identity when deploying to Azure
- Consider implementing rate limiting for production deployments

### Development

To modify the chatbot behavior:

1. **UI Changes**: Edit `IndianaChatBot.Client/Components/ChatBot.razor`
2. **Agent Logic**: Modify `IndianaChatBot/Services/AgentService.cs`
3. **API Endpoints**: Update `IndianaChatBot/Controllers/ChatController.cs`

### Troubleshooting

- **Chat button not appearing**: Ensure the client project is built and the server is running
- **Authentication errors**: Verify you're logged in with `az login` or have proper credentials configured
- **No responses**: Check that Endpoint and AgentName are configured correctly in appsettings.json
- **Build errors**: Verify .NET 10 SDK is installed with `dotnet --version`

### License

This project is part of the indianaCode repository.

### Contributing

Contributions are welcome! Please feel free to submit a Pull Request.