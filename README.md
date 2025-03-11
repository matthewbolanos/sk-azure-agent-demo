# Semantic Kernel + Azure AI Agent demo

In this demo, we will show how to use the Semantic Kernel process framework to orchestrate
multiple Azure AI Agent Service agents.

## Prerequisites
You'll need an Azure AI Foundry project with the following agents. You can find the code to create these agents in the `agents` folder.

- [Researcher agent](./agents/researcher_agent.py)
- [Writer agent](./agents/writer_agent.py)
- [Editor agent](./agents/editor_agent.py)
- [Publisher agent](./agents/publisher_agent.py)

## Running the demo
To run the demo, first copy the `sample.env` file to `.env` and fill in the required values.

Then update the `./agents/tools/send_email.yaml` file with the endpoint of your send email API.

Then, run the following command:

```bash
dotnet build
dotnet run
```