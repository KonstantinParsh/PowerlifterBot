using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PowerlifterBot.Enums;
using PowerlifterBot.Record;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PowerlifterBot.Handlers;

public class UpdateRouter
{
    private readonly TextCommandHandler _textHandler = new();
    private readonly CallbackQueryHandler _callbackHandler = new();
    
    private readonly ConcurrentDictionary<long, SurveyStep> _userSteps = new();
    private readonly ConcurrentDictionary<long, TempProfile> _userProfiles = new();

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message != null)
        {
            await _textHandler.ProcessMessageAsync(botClient, 
                update.Message,
                _userSteps,
                _userProfiles,
                cancellationToken);
            return;
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await _callbackHandler.ProcessCallbackAsync(botClient, 
                update.CallbackQuery,
                _userSteps,
                _userProfiles,
                cancellationToken);
            return;
        }
    }
}