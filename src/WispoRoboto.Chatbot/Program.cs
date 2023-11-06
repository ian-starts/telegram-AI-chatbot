using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI_API;
using Pinecone;
using Telegram.Bot;
using WispoRoboto.Chatbot.AzureTables.Repositories;
using WispoRoboto.Chatbot.CommandHandlers;
using WispoRoboto.Chatbot.CommandHandlers.Contracts;
using WispoRoboto.Chatbot.LLM;
using WispoRoboto.Chatbot.LLM.Contracts;
using WispoRoboto.Chatbot.Telegram.Contracts;
using WispoRoboto.Chatbot.Vectors;
using WispoRoboto.Chatbot.Vectors.Contracts;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, s) =>
    {
        s.AddScoped<IOpenAIAPI>(_ => new OpenAIAPI(hostContext.Configuration.GetValue<string>("OpenAi_ApiKey")));
        s.AddSingleton<IVectorRepository>(_ =>
        {
            var pineconeClient =
                new PineconeClient(hostContext.Configuration.GetValue<string>("Pinecone_ApiKey"), "gcp-starter");
            var index = pineconeClient.GetIndex("chat-messages").Result;
            return new PineConeVectorRepository(index);
        });
        s.AddScoped<IVectorFactory>(sp => new OpenAiAdaVectorFactory(sp.GetRequiredService<IOpenAIAPI>()));
        s.AddTransient<ITelegramBotClient>(_ =>
            new TelegramBotClient(hostContext.Configuration.GetValue<string>("Telegram_ApiKey")));
        s.AddScoped<ILLMQueryable, ChatGpt4Handler>();
        s.AddTransient<ICommandHandler, QueryChatCommandHandler>();
        s.AddSingleton<TableClient>(_ =>
            new TableClient(new Uri(hostContext.Configuration.GetValue<string>("StorageAccount_URL")),
                "TelegramMessages",
                new DefaultAzureCredential()));
        s.AddTransient<IMessageRepository, AzureTablesPineconeMessageRepository>();
    })
    .Build();

host.Run();