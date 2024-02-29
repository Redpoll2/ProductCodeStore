using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProductCodeStore
{
    public sealed class TelegramUpdateListenerService(ILogger<TelegramUpdateListenerService> logger, ITelegramUpdateQueue queue, ITelegramBotClient client) : BackgroundService
    {
        private readonly ILogger<TelegramUpdateListenerService> _logger = logger;
        private readonly ITelegramUpdateQueue _queue = queue;
        private readonly ITelegramBotClient _client = client;

        private int _updateId;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                Update[] updates = await _client.GetUpdatesAsync(
                    offset: _updateId + 1,
                    timeout: 30,
                    allowedUpdates: [ UpdateType.Message, UpdateType.CallbackQuery ],
                    cancellationToken: stoppingToken);

                foreach (Update update in updates)
                {
                    if (update.Type == UpdateType.Message)
                    {
                        Message message = update.Message!;

                        _logger.LogInformation("Получено сообщение от {User}: {Text}", message.From, message.Text);
                    }
                    else
                    {
                        _logger.LogInformation("Произошло новое событие с типом '{Type}'", update.Type);
                    }

                    await _queue.EnqueueAsync(update, stoppingToken);

                    _updateId = update.Id;
                }
            }
        }
    }
}
