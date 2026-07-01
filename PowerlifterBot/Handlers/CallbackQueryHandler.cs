using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PowerlifterBot.Enums;
using PowerlifterBot.Record;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PowerlifterBot.Handlers;

public class CallbackQueryHandler
{
    public async Task ProcessCallbackAsync(ITelegramBotClient bot, 
        CallbackQuery callbackQuery,
        ConcurrentDictionary<long, SurveyStep> userSteps,
        ConcurrentDictionary<long, TempProfile> userProfiles,
        CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        switch (callbackQuery.Data)
        {
            case "start_btn":
                userSteps[chatId] = SurveyStep.AwaitingName;

                if (!userProfiles.ContainsKey(chatId))
                {
                    userProfiles[chatId] = new TempProfile();
                }
                userProfiles[chatId].WelcomeMessageId = messageId;
                
                await bot.EditMessageMedia(
                    chatId: chatId,
                    messageId: messageId,
                    media: new InputMediaPhoto(InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjz-3GPvY2JcmIIkGfUIuq916nKOvui1UIUH61Gu_wL1GR-5C-Mt5br2s&s=10"))
                    {
                        Caption = "Шаг 1 из 5\n\nНапиши свое имя или псевдоним.\n\n(Будет отображаться в таблице рекордов)"
                    },
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
                break;
            
            case "records_btn":
                break;
                
            case "agree_btn":
            case "disagree_btn":
                if (userProfiles.TryGetValue(chatId, out var currentProfile))
                {
                    currentProfile.WeightUnit = callbackQuery.Data == "agree_btn"
                        ? WeightUnit.Lbs : WeightUnit.Kg;

                    var dbProfile = new UserProfile
                    {
                        TelegramId = chatId,
                        Name = currentProfile.Name,
                        Age = currentProfile.Age,
                        BodyWeight = currentProfile.BodyWeight,
                        Height = currentProfile.Height,
                        WeightUnit = currentProfile.WeightUnit
                    };

                    using (var db = new PowerlifterBot.Database.BotDbContext())
                    {
                        db.Users.Add(dbProfile);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    
                    userProfiles.TryRemove(chatId, out _);
                    userSteps.TryRemove(chatId, out _);

                    await bot.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Анкета успешно сохранена!\n\n" +
                              $"- ИМЯ: {dbProfile.Name}\n" +
                              $"- ВОЗРАСТ: {dbProfile.Age}\n" +
                              $"- РОСТ: {dbProfile.Height}\n" +
                              $"- ВЕС: {dbProfile.BodyWeight}\n",
                        replyMarkup: null,
                        cancellationToken: cancellationToken);
                }
                break;
        }
        
        await bot.AnswerCallbackQuery(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken);
    }
}