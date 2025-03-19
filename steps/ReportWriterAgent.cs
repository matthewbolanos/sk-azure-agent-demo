// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using Azure;
using Azure.AI.Projects;
using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Events;

namespace Steps;

public sealed class ReportWriterAgent(AzureAIClientProvider clientProvider) : KernelProcessStep
{
    readonly AzureAIClientProvider clientProvider = clientProvider;
    readonly AgentsClient agentsClient = clientProvider.Client.GetAgentsClient();

    [KernelFunction]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, string threadId)
    {
        Env.Load();

        Agent definition = await agentsClient.GetAgentAsync(Environment.GetEnvironmentVariable("WRITER_AGENT_ID"));
        AzureAIAgent agent = new(definition, clientProvider) { Kernel = new Kernel() };
        KernelPlugin plugin = KernelPluginFactory.CreateFromObject(new Help(agentsClient, context, threadId));
        agent.Kernel.Plugins.Add(plugin);

        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[Writer Agent]");
        Console.ResetColor();
        Console.WriteLine();

        try
        {
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(threadId))
            {
                Console.Write(response.Content);
            }
            Console.WriteLine();

        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        
        return threadId;
    }
    private sealed class Help(AgentsClient agentsClient, KernelProcessStepContext context, string threadId)
    {
        private readonly KernelProcessStepContext context = context;
        private readonly string threadId = threadId;

        [KernelFunction("NeedMoreResearch"), Description("Call if you need more research to be performed")]
        async public Task NeedMoreResearchAsync(
            [Description("A description of the research required")] string researchNeeded
        )
        {
            ThreadRun run = (await agentsClient.GetRunsAsync(threadId, 1, ListSortOrder.Descending)).Value.FirstOrDefault()!;
            await agentsClient.CancelRunAsync(threadId, run.Id);

            try
            {
                // poll the thread until the run is cancelled
                while (run.Status != RunStatus.Cancelled)
                {
                    run = (await agentsClient.GetRunAsync(threadId, run.Id)).Value;
                    await Task.Delay(1000);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Need more research: ");
            Console.ResetColor();
            Console.WriteLine(researchNeeded);

            await agentsClient.CreateMessageAsync(
                threadId,
                MessageRole.User,
                "Additional research required: " + researchNeeded
            );
            await context.EmitEventAsync(new KernelProcessEvent { Id = ProcessEvents.NeedMoreData, Data = threadId });
        }
    }

}