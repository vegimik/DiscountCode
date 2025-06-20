using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DiscountServer;

public class DiscountTcpServer
{
    private readonly TcpListener _listener = new(IPAddress.Any, 5000);
    private readonly DiscountService _service;
    private readonly ILogger<DiscountTcpServer> _logger;

    public DiscountTcpServer(DiscountService service, ILogger<DiscountTcpServer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Server started...");
        _listener.Start();
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[2048];
            var bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead == 3) // Minimum for generate request
            {
                ushort count = BitConverter.ToUInt16(buffer, 0);
                byte length = buffer[2];

                _logger.LogInformation($"Request for code generation (NumberOfCodes={count}, Length={length})");

                var result = _service.GenerateCodes(count, length);
                await _service.SaveAsync();

                _logger.LogInformation($"Generated codes until now: {string.Join(", ", _service.GetAllCodes())}");

                await stream.WriteAsync(BitConverter.GetBytes(result));
            }
            else if (bytesRead == 8) // Use code request
            {
                var code = Encoding.UTF8.GetString(buffer, 0, 8);

                _logger.LogInformation($"Request for use code (Code={code})");

                var success = _service.UseCode(code);
                await _service.SaveAsync();

                _logger.LogInformation($"Remaining codes until now: {string.Join(", ", _service.GetAllCodes())}");

                await stream.WriteAsync(new byte[] { success ? (byte)1 : (byte)0 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client request");
        }
        finally
        {
            client.Close();
        }
    }
}
