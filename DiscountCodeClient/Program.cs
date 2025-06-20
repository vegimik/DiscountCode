using DiscountClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
services.AddSingleton<CodeClientService>();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();
var service = provider.GetRequiredService<CodeClientService>();

while (true)
{
    Console.Write("Enter number of parallel clients: ");
    if (!int.TryParse(Console.ReadLine(), out int numClients) || numClients <= 0)
    {
        logger.LogWarning("Invalid number of clients.");
        continue;
    }

    Console.WriteLine("\n1. Generate codes");
    Console.WriteLine("2. Use code");
    Console.WriteLine("0. Exit");
    Console.Write("Choose: ");
    var option = Console.ReadLine();

    if (option == "0")
        break;

    ushort count = 0;
    byte length = 0;
    HashSet<string> _codes = new();

    if (option == "1")
    {
        Console.Write("Count (1-2000): ");
        if (!ushort.TryParse(Console.ReadLine(), out count) || count < 1 || count > 2000)
        {
            logger.LogWarning("Invalid count.");
            continue;
        }

        Console.Write("Length (7 or 8): ");
        if (!byte.TryParse(Console.ReadLine(), out length) || (length != 7 && length != 8))
        {
            logger.LogWarning("Invalid length.");
            continue;
        }
    }
    else if (option == "2")
    {
        string doesItExist = "";
        while (_codes.Count != numClients)
        {
            Console.Write($"Code (client {_codes.Count}{doesItExist}): ");
            string codeRaw = Console.ReadLine()!.PadRight(8);
            if (_codes.Contains(codeRaw))
            {
                doesItExist = ", It assigned, please try another code.";
            }
            else
            {
                doesItExist = "";
                _codes.Add(codeRaw);
            }
        }
    }
    else
    {
        logger.LogWarning("Invalid option.");
        continue;
    }

    List<Task> tasks = new();

    for (int i = 0; i < numClients; i++)
    {
        int clientId = i;
        if (option == "1")
            tasks.Add(service.GenerateCodesAsync(clientId, count, length));
        else if (option == "2")
            tasks.Add(service.UseCodeAsync(clientId, _codes.ElementAt(i)));
    }

    await Task.WhenAll(tasks);
}
