using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ProductCodeStore
{
    public interface ITelegramUpdateQueue
    {
        ValueTask<Update> DequeueAsync(CancellationToken cancellationToken = default);
        ValueTask EnqueueAsync(Update update, CancellationToken cancellationToken = default);
    }

    public sealed class TelegramUpdateQueue : ITelegramUpdateQueue
    {
        private readonly Channel<Update> _queue;

        public TelegramUpdateQueue(int capacity)
        {
            _queue = Channel.CreateBounded<Update>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
            });
        }

        public ValueTask<Update> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return _queue.Reader.ReadAsync(cancellationToken);
        }

        public ValueTask EnqueueAsync(Update update, CancellationToken cancellationToken = default)
        {
            return _queue.Writer.WriteAsync(update, cancellationToken);
        }
    }
}
