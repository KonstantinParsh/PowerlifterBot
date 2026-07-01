using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using PowerlifterBot.Enums;
using PowerlifterBot.Record;

namespace PowerlifterBot.Handlers;

public class TextCommandHandler
{
    public async Task ProcessMessageAsync(ITelegramBotClient bot, 
        Message message,
        ConcurrentDictionary<long, SurveyStep> userSteps,
        ConcurrentDictionary<long, TempProfile> userProfiles,
        CancellationToken cancellationToken)
    {
        if (message.Text is not { } messageText) return;
        var chatId = message.Chat.Id;

        if (messageText == "/start")
        {
            var welcomeButtons = new InlineKeyboardMarkup(new[] {
                new[] { InlineKeyboardButton.WithCallbackData("📝 Заполнить анкету", "start_btn") },
                new[] { InlineKeyboardButton.WithCallbackData("🏆 Таблица рекордов", "records_btn") }
            });
            
            var sentPhotoMessage = await bot.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromUri("https://cdn.shopify.com/s/files/1/1103/4864/files/Artboard_4-min.jpg?v=1726766796"),
                caption: "Добро пожаловать в дневник пауэрлифтера!\n\nЗдесь ты можешь фиксировать свои рекорды в силовом троеборье, а также соревноваться с другими пользователями!\n\nНу что, готов начать?",
                replyMarkup: welcomeButtons,
                cancellationToken: cancellationToken);
            
            userProfiles[chatId] = new TempProfile { WelcomeMessageId = sentPhotoMessage.MessageId };
            return;
        }

        if (userSteps.TryGetValue(chatId, out var currentStep))
        {
            if (!userProfiles.TryGetValue(chatId, out var currentProfile)) return;

            try
            {
                await bot.DeleteMessage(
                    chatId:  chatId,
                    messageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }
            catch {  }

            switch (currentStep)
            {
                case SurveyStep.AwaitingName:
                    currentProfile.Name = messageText;

                    userSteps[chatId] = SurveyStep.AwaitingAge;
                    await bot.EditMessageCaption(
                        chatId: chatId,
                        messageId: currentProfile.WelcomeMessageId,
                        caption: "Шаг 2 из 5\n\nСколько тебе лет? (Цифрами)",
                        cancellationToken: cancellationToken);
                    break;
                
                case SurveyStep.AwaitingAge:
                    if (int.TryParse(messageText, out int age))
                    {
                        try
                        {
                            currentProfile.Age = age;

                            userSteps[chatId] = SurveyStep.AwaitingWeight;
                            await bot.EditMessageCaption(
                                chatId: chatId,
                                messageId: currentProfile.WelcomeMessageId,
                                caption: "Шаг 3 из 5\n\nСколько ты весишь (в килограммах)?\n\n(пример ввода: 65.5)",
                                cancellationToken: cancellationToken);
                        }
                        catch (ArgumentException ex)
                        {
                            await bot.EditMessageCaption(
                                chatId: chatId,
                                messageId: currentProfile.WelcomeMessageId,
                                caption: $"❌ Ошибка: {ex.Message}",
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await bot.EditMessageCaption(
                            chatId: chatId,
                            messageId: currentProfile.WelcomeMessageId,
                            caption: "❌ Пожалуйста, введите возраст корректным числом!",
                            cancellationToken: cancellationToken);
                    }
                    break;
                
                case SurveyStep.AwaitingWeight:
                    string normalizedWeight = messageText.Replace('.', ',');
                    if (double.TryParse(normalizedWeight, out double weight))
                    {
                        try
                        {
                            currentProfile.BodyWeight = weight;

                            userSteps[chatId] = SurveyStep.AwaitingHeight;
                            await bot.EditMessageCaption(
                                chatId: chatId,
                                messageId: currentProfile.WelcomeMessageId,
                                caption: "Шаг 4 из 5\n\nКакой у тебя рост (в сантиметрах)?\n\n(пример ввода: 175)",
                                cancellationToken: cancellationToken);
                        }
                        catch (ArgumentException ex)
                        {
                            await bot.EditMessageCaption(
                                chatId: chatId,
                                messageId: currentProfile.WelcomeMessageId,
                                caption: $"❌ Ошибка: {ex.Message}",
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await bot.EditMessageCaption(
                            chatId: chatId,
                            messageId: currentProfile.WelcomeMessageId,
                            caption: "❌ Пожалуйста, введите вес корректным числом!",
                            cancellationToken: cancellationToken);
                    }
                    break;
                
                case SurveyStep.AwaitingHeight:
                    if (int.TryParse(messageText, out int height))
                    {
                        try
                        {
                            currentProfile.Height = height;
                            userSteps[chatId] = SurveyStep.AwaitingUnits;

                            var userChoice = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("✅ Да", "agree_btn"),
                                    InlineKeyboardButton.WithCallbackData("❌ Нет", "disagree_btn")
                                }
                            });
                            
                            try 
                            { 
                                await bot.DeleteMessage(chatId, currentProfile.WelcomeMessageId, cancellationToken); 
                            } 
                            catch {  }

                            var newTextMessage = await bot.SendMessage(
                                chatId: chatId,
                                text: "Шаг 5 из 5\n\nЖелаете ли вы изменить единицы измерения на фунты?\n\n(Все веса в таблицах будут отображаться в фунтах и рекорды будут регистрироваться в фунтах)",
                                replyMarkup: userChoice,
                                cancellationToken: cancellationToken);
                            
                            currentProfile.WelcomeMessageId = newTextMessage.MessageId;
                        }
                        catch (ArgumentException ex)
                        {
                            await bot.EditMessageCaption(
                                chatId: chatId,
                                messageId: currentProfile.WelcomeMessageId,
                                caption: $"❌ Ошибка: {ex.Message}",
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await bot.EditMessageCaption(
                            chatId: chatId,
                            messageId: currentProfile.WelcomeMessageId,
                            caption: "❌ Пожалуйста, введите рост корректным числом!",
                            cancellationToken: cancellationToken);
                    }
                    break;
            }
        }
    }
}