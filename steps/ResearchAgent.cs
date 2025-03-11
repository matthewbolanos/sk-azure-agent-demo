// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.Projects;
using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace Steps;


public sealed class ResearchAgent(AzureAIClientProvider clientProvider) : KernelProcessStep
{
    readonly AzureAIClientProvider clientProvider = clientProvider;
    readonly AgentsClient agentsClient = clientProvider.Client.GetAgentsClient();

    [KernelFunction]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, string threadId)
    {
        Env.Load();

        Agent definition = await agentsClient.GetAgentAsync(Environment.GetEnvironmentVariable("RESEARCH_AGENT_ID"));
        AzureAIAgent agent = new(definition, clientProvider){Kernel = new Kernel()};

        // Print agent details to the screen
        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[Researcher Agent]");
        Console.ResetColor();
        Console.WriteLine();

        // Run agent to perform research
        try 
        {
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(threadId))
            {
                foreach(var item in response.Items)
                {
                    switch(item)
                    {
                        case StreamingTextContent textContent:
                            Console.Write(textContent);
                            break;
                    }
                }
            }
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        

        return threadId;
    }
}