using DiscountServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
services.AddSingleton<ICodeStorage, CodeStorage>();
services.AddSingleton<DiscountService>();
services.AddSingleton<DiscountTcpServer>();

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

var server = provider.GetRequiredService<DiscountTcpServer>();
await server.StartAsync();
