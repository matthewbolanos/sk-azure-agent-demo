import os
import yaml
from dotenv import load_dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.ai.projects.models import OpenApiTool, OpenApiAnonymousAuthDetails

load_dotenv()

project_client = AIProjectClient.from_connection_string(
    credential=DefaultAzureCredential(),
    conn_str=os.environ["PROJECT_CONNECTION_STRING"],
)

# Update the agent with the latest OpenAPI tool
agent = project_client.agents.create_agent(
    model="gpt-4o-mini",
    name="Writer agent",
    instructions="""Your job is to write the report. Call the "need more research" tool if you need more data.

Required topics:
- User sentiment
- Market size

Do not make parallel tool calls"""
)