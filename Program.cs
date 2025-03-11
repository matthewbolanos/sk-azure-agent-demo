using Microsoft.SemanticKernel;
using Azure.Identity;
using Microsoft.SemanticKernel.Agents.AzureAI;

using Events;
using Steps;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;


namespace GettingStarted.AzureAgents
{
    public static class Program
    {   
        public static async Task Main(string[] args)
        {
            Env.Load();

            var connectionString = Environment.GetEnvironmentVariable("PROJECT_CONNECTION_STRING");
            var clientProvider = AzureAIClientProvider.FromConnectionString(connectionString, new DefaultAzureCredential());
            
             // Create a simple kernel 
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(clientProvider);
            Kernel kernel = kernelBuilder.Build();

            // Create a process that will interact with the chat completion service
            ProcessBuilder process = new("ChatBot");
            var init = process.AddStepFromType<Init>();
            var researcher = process.AddStepFromType<ResearchAgent>();
            var writer = process.AddStepFromType<ReportWriterAgent>();
            var editor = process.AddStepFromType<ReportEditorAgent>();
            var sender = process.AddStepFromType<ReportSenderAgent>();

            // Define the process flow
            process
                .OnInputEvent(ProcessEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(init));

            init
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(researcher));

            researcher
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(writer));

            writer
                .OnEvent(ProcessEvents.NeedMoreData)
                .SendEventTo(new ProcessFunctionTargetBuilder(researcher));

            writer
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(editor));

            editor
                .OnEvent(ProcessEvents.NeedsEdit)
                .SendEventTo(new ProcessFunctionTargetBuilder(writer));

            editor
                .OnEvent(ProcessEvents.Approved)
                .SendEventTo(new ProcessFunctionTargetBuilder(sender));

            sender
                .OnFunctionResult()
                .StopProcess();

            // Build the process to get a handle that can be started
            KernelProcess kernelProcess = process.Build();

            Console.Clear();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("What would you like a report on?");
            Console.ResetColor();
            Console.Write (": ");
            string input = Console.ReadLine()!;

            // Start the process with an initial external event
            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                    new KernelProcessEvent()
                    {
                        Id = ProcessEvents.StartProcess,
                        Data = input
                    });
            /*
            // Create the Azure provider. (Implement GetAzureProvider with your settings.)
            var connectionString = "eastus.api.azureml.ms;921496dc-987f-410f-bd57-426eb2611356;rg-amanda25;ai-project-demo-xih6";
            var clientProvider = AzureAIClientProvider.FromConnectionString(connectionString, new DefaultAzureCredential());
            AgentsClient client = clientProvider.Client.GetAgentsClient();


            // Create an agent definition.
            Agent definition = await client.GetAgentAsync("asst_TmyNKaCjuQOqDporKHYrkvmR");
            AzureAIAgent agent = new(definition, clientProvider){Kernel = new Kernel()};

            // Create a plugin from the MenuPlugin type and add it to the agent’s Kernel.
            KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
            agent.Kernel.Plugins.Add(plugin);

            // Create a conversation thread.
            AgentThread thread = await client.CreateThreadAsync();

            while(true)
            {
                Console.Write("You: ");
                string input = Console.ReadLine()!;
                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                ChatMessageContent message = new(AuthorRole.User, input);
                await agent.AddChatMessageAsync(thread.Id, message);

                Console.Write("Agent: ");
                await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(thread.Id))
                {
                    Console.Write(response.Content);
                }
                Console.WriteLine();
            }*/ 
        }

        private class ThreadState
        {
            internal string ThreadId = string.Empty;
        }
    }
}
