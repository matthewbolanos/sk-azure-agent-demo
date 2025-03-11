// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Azure.AI.Projects;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace Steps;

public sealed class Init(AzureAIClientProvider clientProvider) : KernelProcessStep
{
    readonly AzureAIClientProvider clientProvider = clientProvider;

    [KernelFunction]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, string reportTopic)
    {
        // Create the Azure provider. (Implement GetAzureProvider with your settings.)
        AgentsClient client = clientProvider.Client.GetAgentsClient();

        // Create a conversation thread.
        AgentThread thread = await client.CreateThreadAsync();

        await client.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            "Report topic: " + reportTopic
        );

        return thread.Id;
    }
}