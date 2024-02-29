using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProductCodeStore
{
    public sealed class TelegramUpdateHandlerService(ILogger<TelegramUpdateHandlerService> logger, ITelegramUpdateQueue queue, ITelegramBotClient client) : BackgroundService
    {
        private readonly ILogger<TelegramUpdateHandlerService> _logger = logger;
        private readonly ITelegramUpdateQueue _queue = queue;
        private readonly ITelegramBotClient _client = client;

        private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            // Выводим главное меню на каждое сообщение

            await _client.SendTextMessageAsync(
                chatId: message.Chat,
                text: "Приветствуем в нашем магазине - HavenStore! Оплата сервисов, покупка ключей и аккаунтов в пару кнопок и без комиссии! Творение красной лужи и выча.",
                replyMarkup: new InlineKeyboardMarkup([
                    InlineKeyboardButton.WithCallbackData(ActionButtons.Buy),
                    InlineKeyboardButton.WithCallbackData(ActionButtons.Support),
                ]),
                cancellationToken: cancellationToken);
        }

        private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken cancellationToken = default)
        {
            var buttons = new List<InlineKeyboardButton>();

            switch (callback.Data)
            {
                case ActionButtons.Buy:
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Нет товаров"));
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Назад"));
                    break;

                case ActionButtons.Support:
                    buttons.Add(InlineKeyboardButton.WithUrl("Наш Discord", "https://discord.gg/XNTqjjk"));
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Назад"));
                    break;
            }

            await _client.EditMessageReplyMarkupAsync(
                chatId: callback.Message!.Chat,
                messageId: callback.Message!.MessageId,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                cancellationToken: cancellationToken);
            
            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: callback.Id,
                cancellationToken: cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                Update update = await _queue.DequeueAsync(stoppingToken);

                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(update.Message!, stoppingToken);
                        break;

                    case UpdateType.CallbackQuery:
                        await HandleCallbackAsync(update.CallbackQuery!, stoppingToken);
                        break;
                }
            }
        }
    }
}
