# Semantic Kernel + Azure AI Agent demo

In this demo, we will show how to use the Semantic Kernel process framework to orchestrate
multiple Azure AI Agent Service agents.

## Prerequisites
### Azure Services
Ensure the following Azure services are deployed and configured:

- **Azure AI Foundry Project**: To manage and deploy your AI agents and model endpoints.
- **Azure OpenAI Service**: To provide the underlying AI models used by your agents.
- **Azure Storage Account**: To store data, logs, and model artifacts.
- **Azure Key Vault**: To securely store secrets, keys, and credentials.
- **Azure App Service**: To host APIs or web applications interacting with your deployed models.
- **Grounding with Bing Search**: To enable agents to retrieve and ground information from the web ([setup instructions](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/bing-grounding)).

### Model Endpoints
You will need to deploy model endpoints corresponding to the models required by the agents in AI Foundry. For the list of models required and location of agents, see below.

### Agents
You'll need an Azure AI Foundry project with the following agents. You can find the code to create these agents in the `agents` folder.

- [Researcher agent](./agents/researcher_agent.py)
- [Writer agent](./agents/writer_agent.py)
- [Editor agent](./agents/editor_agent.py)
- [Publisher agent](./agents/publisher_agent.py)

To run the Python files, first copy the `sample.env` file to `.env` and fill in the required values.

## Running the demo
To run the demo, you will re-use the `.env` file that we previously created with filled in required values.

Then update the `./agents/tools/send_email.yaml` file with the endpoint of your send email API.

Then, run the following command:

```bash
dotnet build
dotnet run
```