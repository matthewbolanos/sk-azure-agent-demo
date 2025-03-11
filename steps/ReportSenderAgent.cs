// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.AI.Projects;
using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Events;

namespace Steps;

public sealed class ReportSenderAgent(AzureAIClientProvider clientProvider) : KernelProcessStep
{
    readonly AzureAIClientProvider clientProvider = clientProvider;
    readonly AgentsClient agentsClient = clientProvider.Client.GetAgentsClient();

    [KernelFunction]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, string threadId)
    {
        Env.Load();

        Agent definition = await agentsClient.GetAgentAsync(Environment.GetEnvironmentVariable("SENDER_AGENT_ID"));
        AzureAIAgent agent = new(definition, clientProvider){Kernel = new Kernel()};

        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[Sender Agent]");
        Console.ResetColor();
        Console.WriteLine();

        await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(threadId))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine();

        Console.WriteLine("Email sent.");

        return threadId;
    }
}