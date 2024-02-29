using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ProductCodeStore;

internal static class Program
{
    public static async Task Main()
    {
        await CreateEmptyBuilder().ConfigureChatbot().RunConsoleAsync();
    }

    public static IHostBuilder CreateEmptyBuilder()
    {
        return new HostBuilder();
    }

    public static IHostBuilder ConfigureChatbot(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            // получать токены для сервисов из...
            // во время разработки: из User Secrets
            // при развёртке: из Environment Variables

            IHostEnvironment environment = context.HostingEnvironment;

            if (environment.IsDevelopment())
            {
                builder.AddUserSecrets(typeof(Program).Assembly, optional: false, reloadOnChange: false);
            }

            builder.AddEnvironmentVariables();
        });

        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(services =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                return new TelegramBotClient(token: configuration.GetRequiredSection("Telegram:Token").Value!);
            });

            services.AddSingleton<ITelegramUpdateQueue, TelegramUpdateQueue>(services =>
            {
                return new TelegramUpdateQueue(capacity: 16);
            });
        });

        hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddHostedService<TelegramUpdateListenerService>();
            services.AddHostedService<TelegramUpdateHandlerService>();
        });

#if DEBUG
        hostBuilder.UseEnvironment(Environments.Development);
#endif

        return hostBuilder;
    }
}
