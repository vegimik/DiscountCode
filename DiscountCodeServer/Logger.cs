using System.Threading.Channels;

namespace DiscountServer;

public interface ILogger
{
    void Log(string message);
}

public class AsyncFileLogger : ILogger, IDisposable
{
    private readonly Channel<string> _logChannel = Channel.CreateUnbounded<string>();
    private readonly StreamWriter _writer;
    private readonly Task _processor;

    public AsyncFileLogger(string filePath)
    {
        _writer = new StreamWriter(File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };

        _processor = Task.Run(async () =>
        {
            await foreach (var msg in _logChannel.Reader.ReadAllAsync())
            {
                try
                {
                    await _writer.WriteLineAsync($"{DateTime.Now:O} - {msg}");
                }
                catch { }
            }
        });
    }

    public void Log(string message)
    {
        _logChannel.Writer.TryWrite(message);
    }

    public void Dispose()
    {
        _logChannel.Writer.Complete();
        _processor.Wait();
        _writer.Dispose();
    }
}
