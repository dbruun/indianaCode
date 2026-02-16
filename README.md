# indianaCode

## Indiana ChatBot - .NET 10 SPA with AI Agent

A Single Page Application (SPA) built with .NET 10 Blazor WebAssembly featuring an AI-powered chatbot that integrates with Microsoft Foundry hosted agents and Bing Custom Search for grounding.

### Features

- 🎨 **Modern SPA**: Built with Blazor WebAssembly for a responsive single-page application experience
- 🤖 **AI-Powered Chat**: Integrated with Microsoft Foundry Agent Framework
- 🔍 **Grounded Responses**: Uses Bing Custom Search for accurate, source-backed answers
- 💬 **Interactive UI**: Beautiful chatbot interface with typing indicators and smooth animations
- ⚡ **Real-time**: WebAssembly-based client with API backend for fast responses

### Technology Stack

- **.NET 10**: Latest .NET framework
- **Blazor WebAssembly**: Client-side SPA framework
- **Microsoft.Extensions.AI**: Agent Framework integration
- **ASP.NET Core Web API**: Backend API for agent communication
- **Bing Custom Search API**: Grounding and information retrieval

### Getting Started

#### Prerequisites

- .NET 10 SDK
- Microsoft Foundry account with agent deployment
- Bing Custom Search API subscription (optional but recommended)

> **💡 Quick Deploy**: Use our [Azure Bicep templates](infrastructure/README.md) to automatically deploy the Bing Custom Search resource!

#### Configuration

1. **Clone the repository**
   ```bash
   git clone https://github.com/dbruun/indianaCode.git
   cd indianaCode/IndianaChatBot
   ```

2. **Deploy Azure Resources (Optional but Recommended)**
   
   Deploy Bing Custom Search using our automated Bicep templates:
   
   ```bash
   # Deploy infrastructure
   az deployment sub create \
     --location eastus \
     --template-file infrastructure/main.bicep
   ```
   
   See [infrastructure/README.md](infrastructure/README.md) for detailed deployment instructions.

3. **Configure API Keys**
   
   Edit `IndianaChatBot/appsettings.json` and add your credentials:
   
   ```json
   {
     "FoundryAgent": {
       "Endpoint": "https://your-foundry-endpoint.azure.com/openai/deployments/your-deployment/chat/completions?api-version=2024-02-15-preview",
       "ApiKey": "YOUR_FOUNDRY_API_KEY"
     },
     "BingSearch": {
       "ApiKey": "YOUR_BING_SEARCH_API_KEY",
       "CustomConfigId": "YOUR_CUSTOM_CONFIG_ID"
     }
   }
   ```

   **Note**: The application will work in demo mode without these credentials, showing helpful messages to guide configuration.

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
4. The AI assistant will respond with information grounded by Bing Custom Search

### Project Structure

```
IndianaChatBot/
├── IndianaChatBot/                  # Server project
│   ├── Controllers/
│   │   └── ChatController.cs        # API endpoint for chat
│   ├── Services/
│   │   ├── IAgentService.cs         # Service interface
│   │   └── AgentService.cs          # Agent implementation with Foundry & Bing integration
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
   - AgentService: Orchestrates Microsoft Foundry and Bing Search
   - Implements grounding with search results
   - Handles fallback for unconfigured services

### Security Considerations

- Never commit API keys to source control
- Use environment variables or Azure Key Vault in production
- The provided appsettings.json contains placeholder values only
- Consider implementing rate limiting for production deployments

### Development

To modify the chatbot behavior:

1. **UI Changes**: Edit `IndianaChatBot.Client/Components/ChatBot.razor`
2. **Agent Logic**: Modify `IndianaChatBot/Services/AgentService.cs`
3. **API Endpoints**: Update `IndianaChatBot/Controllers/ChatController.cs`

### Troubleshooting

- **Chat button not appearing**: Ensure the client project is built and the server is running
- **No responses**: Check that API keys are configured in appsettings.json
- **Build errors**: Verify .NET 10 SDK is installed with `dotnet --version`

### License

This project is part of the indianaCode repository.

### Contributing

Contributions are welcome! Please feel free to submit a Pull Request.