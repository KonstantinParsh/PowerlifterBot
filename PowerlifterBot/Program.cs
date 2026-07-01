using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using PowerlifterBot.Handlers;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
    .Build();
    
string botToken = configuration["BotSettings:Token"]
    ?? throw new ArgumentException("Bot token not found");

var botClient = new TelegramBotClient(botToken);
using var cts = new CancellationTokenSource();

var updateRouter = new UpdateRouter();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};


botClient.StartReceiving(
    updateHandler: updateRouter.HandleUpdateAsync,
    errorHandler: (bot, exception, cancellationToken) =>
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Polling Error]: {exception.Message}");
        Console.ResetColor();
        return Task.CompletedTask;
    },
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var botInfo = await botClient.GetMe(cancellationToken: cts.Token);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"=== Бот @{botInfo.Username} успешно запущен! ===");
Console.ResetColor();
Console.WriteLine("Нажми [Enter] в консоли, чтобы остановить его работу.");

Console.ReadLine();
cts.Cancel();