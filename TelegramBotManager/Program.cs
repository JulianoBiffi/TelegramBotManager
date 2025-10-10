using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBotManager.Configurations;
using TelegramBotManager.Middleware;

var builder =
    FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
       .AddEnvironmentVariables();

builder.Services
       .AddApplicationInsightsTelemetryWorkerService()
       .ConfigureFunctionsApplicationInsights();

builder.UseMiddleware<ValidationExceptionMiddleware>();

IConfiguration configuration = builder.Configuration;

builder.Services.AddAzureStorageAndQueue(configuration);

builder.Services.AddAutoMapper(configAction => { }, typeof(Program));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Build().Run();


/*
 
 public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            TelegramBotClient Bot = new TelegramBotClient("8396528579:AAFgI9fQ1rMz9vk8zls6qqP5hNba-FGsxvM");
            var json = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(json))
                return new OkObjectResult("Vazio");

            var update = JsonSerializer.Deserialize<Update>(json);

            var fullchat = await Bot.GetChat("7658500418");

            await Bot.SendMessage(fullchat.PersonalChat, $"Mensagem recebida!");
            
             {"update_id":342538516,
"message":{"message_id":7,"from":{"id":7658500418,"is_bot":false,"first_name":"Juliano","last_name":"Biffi","language_code":"pt-br"},"chat":{"id":7658500418,"first_name":"Juliano","last_name":"Biffi","type":"private"},"date":1759431417,"text":"oi"}}
         

if (update?.Message != null)
{
    var chatId = update.Message.Chat.Id;
    var messageText = update.Message.Text;

    await Bot.SendMessage(update.Message.Chat, $"Você disse: {messageText}");
}
return new OkResult();
        }


using ScottPlot;
using ScottPlot.Palettes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


ScottPlot.Plot myPlot = new();

// Define the values for each slice
double[] values = { 15, 25, 30, 10, 20 };

// Add the pie chart to the plot
var pie = myPlot.Add.Pie(values);

// Customize the pie chart (optional)
pie.ExplodeFraction = 0.05; // Add a small gap between slices
pie.SliceLabelDistance = 0.5; // Position the labels

// Hide unnecessary plot components for a cleaner look
myPlot.Axes.Frameless();
myPlot.HideGrid();

// Save the plot to an image file
myPlot.SavePng("pie_chart_example.png", 600, 400);

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("8396528579:AAFgI9fQ1rMz9vk8zls6qqP5hNba-FGsxvM", cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnMessage += OnMessage;

await bot.SendMessage("7658500418", $"Teste: {DateTime.Now.ToString("dd/MM/yyyy")}");

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

// method that handle messages received by the bot:
async Task OnMessage(Message msg, UpdateType type)
{

    return;
    if (msg.Text is null) return;	// we only handle Text messages here
    Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");
    // let's echo back received text in the chat
    await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");


    await bot.SendPhoto(
    chatId: msg.Chat,
    photo: new InputFileStream(File.OpenRead("pie_chart_example.png")),
    caption: "Aqui está seu gráfico."
);
}

*/