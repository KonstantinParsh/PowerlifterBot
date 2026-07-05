using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PowerlifterBot.Database;
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
        
        // ГЛАВНОЕ МЕНЮ НАСТРОЕК
        var mainSettingsBtns = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("👤 Настройки профиля", "menu_user_settings_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("🔍 Настройки отображения", "menu_vizualize_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", "back_to_main_menu_btn") },
        });
        
        // НАСТРОЙКИ ПРОФИЛЯ
        var userSettingsBtns = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("👤 Изменить ник", "profile_change_nickname_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("⚖ Изменить вес", "profile_change_bodyweight_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("📏 Изменить рост", "profile_change_height_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("👴 Изменить возраст", "profile_change_age_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("❌ Удалить профиль", "profile_delete_btn") },
            new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", "back_to_main_settings_menu_btn") }
        });

        switch (callbackQuery.Data)
        {
            // --- СЕКЦИЯ РЕГИСТРАЦИИ ---
            
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
                        Caption = "📝 Шаг 1 из 5\n\nНапиши свое имя или псевдоним.\n\n(Будет отображаться в таблице рекордов)"
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
            
            // --- КОНЕЦ СЕКЦИИ РЕГИСТРАЦИИ ---
            
            
            // --- СЕКЦИЯ НАСТРОЕК ---
            
            // -- ГЛАВНОЕ МЕНЮ НАСТРОЕК --
            case "menu_settings":
                await bot.EditMessageMedia(
                    chatId: chatId,
                    messageId: messageId,
                    media: new InputMediaPhoto(InputFile.FromUri("https://elements-resized.envatousercontent.com/elements-video-cover-images/6e739fc0-502b-469a-aaa3-07aa218d2179/video_preview/video_preview_0000.jpg?w=500&cf_fit=cover&q=85&format=auto&s=54c04f12ccaa2febe8f3c7791726b11549bf1c43f165cc5217d285cc580391ce"))
                    {
                        Caption = $"Сейчас Вы находитесь в меню настроек\n\nВыберите что хотите изменить:"
                    },
                    replyMarkup: mainSettingsBtns,
                    cancellationToken: cancellationToken);
                break;
            
            // - КНОПКА "НАЗАД" -
            case "back_to_main_menu_btn":
                var mainMenuBtns = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("📝 Внести рекорд", "menu_add_record"),
                        InlineKeyboardButton.WithCallbackData("📈 Таблица прогресса", "menu_user_records_table")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🏆 Таблица рекордов пользователей", "menu_users_records")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("⚙ Настройки", "menu_settings")
                    }
                });

                await bot.EditMessageMedia(
                    chatId: chatId,
                    messageId: messageId,
                    media: new InputMediaPhoto(InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSF6GNJE-j0zldl5Tg4JV9jQ5giNVmQZLCowlDdaLn5yphR6u34JJ7B6xY&s=10"))
                    {
                        Caption = $"Вы вернулись в главное меню."
                    },
                    replyMarkup: mainMenuBtns,
                    cancellationToken: cancellationToken);
                break;
            // - КОНЕЦ КНОПКИ "НАЗАД" -
            
            // -- КОНЕЦ ГЛАВНОГО МЕНЮ НАСТРОЕК --
            
            
            // -- НАСТРОЙКИ ПРОФИЛЯ --
            
            case "menu_user_settings_btn":
                await bot.EditMessageMedia(
                    chatId: chatId,
                    messageId: messageId,
                    media: new InputMediaPhoto(InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjz-3GPvY2JcmIIkGfUIuq916nKOvui1UIUH61Gu_wL1GR-5C-Mt5br2s&s=10"))
                    {
                        Caption = $"Теперь Вы находитесь в настройках профиля\n\nВыберите что хотите изменить:"
                    },
                    replyMarkup: userSettingsBtns,
                    cancellationToken: cancellationToken);
                break;
            
            // - КНОПКА "ИЗМЕНИТЬ НИК" -
            case "profile_change_nickname_btn":
                try
                {
                    await bot.DeleteMessage(
                        chatId: chatId,
                        messageId: messageId,
                        cancellationToken: cancellationToken);
                }
                catch {  }
                
                var changeNicknameMessage = await bot.SendMessage(
                    chatId: chatId,
                    text: "📝 Введите новое имя или псевдоним:",
                    cancellationToken: cancellationToken);
                // ДОПИСАТЬ
                break;
            // - КОНЕЦ КНОПКИ "ИЗМЕНИТЬ НИК" -
            
            // - КНОПКА "НАЗАД" -
            case "back_to_main_settings_menu_btn":
                await bot.EditMessageMedia(
                    chatId: chatId,
                    messageId: messageId,
                    media: new InputMediaPhoto(InputFile.FromUri("https://elements-resized.envatousercontent.com/elements-video-cover-images/6e739fc0-502b-469a-aaa3-07aa218d2179/video_preview/video_preview_0000.jpg?w=500&cf_fit=cover&q=85&format=auto&s=54c04f12ccaa2febe8f3c7791726b11549bf1c43f165cc5217d285cc580391ce"))
                    {
                        Caption = $"Сейчас Вы находитесь в меню настроек\n\nВыберите что хотите изменить:"
                    },
                    replyMarkup: mainSettingsBtns,
                    cancellationToken: cancellationToken);
                break;
            // - КОНЕЦ КНОПКИ "НАЗАД" - 
            
            // -- КОНЕЦ НАСТРОЕК ПРОФИЛЯ --
            
            
        }
        
        await bot.AnswerCallbackQuery(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken);
    }
}