import os
import yaml
from dotenv import load_dotenv
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from azure.ai.projects.models import BingGroundingTool, OpenApiAnonymousAuthDetails

load_dotenv()

project_client = AIProjectClient.from_connection_string(
    credential=DefaultAzureCredential(),
    conn_str=os.environ["PROJECT_CONNECTION_STRING"],
)

bing_connection = project_client.connections.get(
    connection_name=os.environ["BING_CONNECTION_NAME"]
)
conn_id = bing_connection.id

# Initialize agent OpenAPI tool using the read in OpenAPI spec
bing = BingGroundingTool(connection_id=conn_id)

# Update the agent with the latest OpenAPI tool
agent = project_client.agents.create_agent(
    model="gpt-4o",
    name="Research agent",
    instructions="Use the Bing search tool to perform research on the topic. You are only the researcher, not the report writer, so simply provide all the information you find.",
    tools=bing.definitions
)