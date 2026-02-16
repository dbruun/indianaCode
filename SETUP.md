# IndianaChatBot - Setup Guide

## Quick Start

### 1. Prerequisites
- .NET 10 SDK installed
- (Optional) Microsoft Foundry account with deployed agent
- (Optional) Bing Custom Search API subscription

### 2. Run the Application

```bash
cd IndianaChatBot/IndianaChatBot
dotnet run
```

The application will start at `https://localhost:5001` (or the URL shown in console).

### 3. Using the Chatbot

1. Open your browser to the application URL
2. Click the purple chat button (💬) in the bottom-right corner
3. Type your message and press Enter or click Send
4. The bot will respond (in demo mode if no API keys configured)

## Configuring API Keys (Optional)

To enable full AI functionality with Microsoft Foundry and Bing Search:

1. Open `IndianaChatBot/appsettings.json`

2. Replace the placeholder values:

```json
{
  "FoundryAgent": {
    "Endpoint": "https://your-actual-endpoint.azure.com/openai/deployments/your-deployment/chat/completions?api-version=2024-02-15-preview",
    "ApiKey": "your-actual-foundry-api-key"
  },
  "BingSearch": {
    "ApiKey": "your-actual-bing-api-key",
    "CustomConfigId": "your-actual-custom-config-id"
  }
}
```

3. Restart the application

### Getting API Keys

**Microsoft Foundry:**
1. Go to Azure OpenAI Studio or Microsoft AI Studio
2. Deploy a model (e.g., GPT-4)
3. Copy the endpoint URL and API key

**Bing Custom Search:**
1. Go to https://www.microsoft.com/en-us/bing/apis/bing-custom-search-api
2. Create a Custom Search instance
3. Get your API key and Custom Config ID from the portal

## Project Structure

```
IndianaChatBot/
├── IndianaChatBot/                  # Server project (.NET Web API)
│   ├── Controllers/
│   │   └── ChatController.cs        # API endpoint
│   ├── Services/
│   │   ├── IAgentService.cs
│   │   └── AgentService.cs          # AI orchestration
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
- 🤖 AI-powered responses via Microsoft Foundry
- 🔍 Grounded answers using Bing Custom Search
- 📱 Responsive design
- ⌨️ Keyboard shortcuts (Enter to send)
- 🎨 Smooth animations and transitions

## Troubleshooting

### Issue: Chat button not appearing
**Solution:** Ensure both projects built successfully. Check browser console for errors.

### Issue: "Error processing request" messages
**Solution:** Verify API keys are correctly configured in appsettings.json.

### Issue: Build errors
**Solution:** Ensure .NET 10 SDK is installed with `dotnet --version`.

## Development

To modify the chatbot:

- **UI changes:** Edit `IndianaChatBot.Client/Components/ChatBot.razor`
- **Styling:** Edit `IndianaChatBot.Client/Components/ChatBot.razor.css`
- **AI logic:** Modify `IndianaChatBot/Services/AgentService.cs`
- **API:** Update `IndianaChatBot/Controllers/ChatController.cs`

## Building for Production

```bash
dotnet publish -c Release -o ./publish
```

The published files will be in the `./publish` directory.

## Security Notes

- Never commit API keys to source control
- Use environment variables or Azure Key Vault in production
- The provided appsettings.json contains only placeholder values
- Consider implementing rate limiting for public deployments

## Support

For issues or questions, refer to the main README.md or create an issue in the repository.
