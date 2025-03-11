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
    name="Editor agent",
    instructions="""If the report is hilarious (it should have at least 3 puns or jokes), then you should approve the report.

Otherwise, approve it and list out the puns and jokes that were in the report."""
)