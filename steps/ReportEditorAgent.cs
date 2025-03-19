// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using Azure;
using Azure.AI.Projects;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Events;
using DotNetEnv;

namespace Steps;

public sealed class ReportEditorAgent(AzureAIClientProvider clientProvider) : KernelProcessStep
{
    readonly AzureAIClientProvider clientProvider = clientProvider;
    readonly AgentsClient agentsClient = clientProvider.Client.GetAgentsClient();

    [KernelFunction]
    public async ValueTask<string> ExecuteAsync(KernelProcessStepContext context, string threadId)
    {
        Env.Load();

        Agent definition = await agentsClient.GetAgentAsync(Environment.GetEnvironmentVariable("EDITOR_AGENT_ID"));
        AzureAIAgent agent = new(definition, clientProvider) { Kernel = new Kernel() };
        KernelPlugin approvalPlugin = KernelPluginFactory.CreateFromObject(new ApprovalTool(agentsClient, context, threadId));
        agent.Kernel.Plugins.Add(approvalPlugin);

        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[Editor Agent]");
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

    private sealed class ApprovalTool(AgentsClient agentsClient, KernelProcessStepContext context, string threadId)
    {
        private readonly KernelProcessStepContext context = context;
        private readonly string threadId = threadId;

        [KernelFunction("Rejected")]
        async public Task RejectedAsync(
            [Description("The reason why the report was rejected")] string reason
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

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Rejected: ");
            Console.ResetColor();
            Console.WriteLine(reason);

            await agentsClient.CreateMessageAsync(
                threadId,
                MessageRole.User,
                "Rejected: " + reason
            );
            await agentsClient.CreateMessageAsync(
                threadId,
                MessageRole.User,
                "Please make the requested edits and resubmit."
            );
            await context.EmitEventAsync(new KernelProcessEvent { Id = ProcessEvents.NeedsEdit, Data = threadId });
        }

        [KernelFunction("Approved")]
        async public Task ApprovedAsync(
            [Description("The reason why the report was approved")] string reason
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Approved: ");
            Console.ResetColor();
            Console.WriteLine(reason);

            await context.EmitEventAsync(new KernelProcessEvent { Id = ProcessEvents.Approved, Data = threadId });
        }
    }

}