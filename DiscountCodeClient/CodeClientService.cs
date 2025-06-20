using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DiscountClient
{
    using System.Net.Sockets;
    using System.Text;

    public class CodeClientService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger<CodeClientService> _logger;

        public CodeClientService(ILogger<CodeClientService> logger, string host = "127.0.0.1", int port = 5000)
        {
            _host = host;
            _port = port;
            _logger = logger;
        }

        public async Task GenerateCodesAsync(int clientId, ushort count, byte length)
        {
            try
            {
                using var client = new TcpClient(_host, _port);
                using var stream = client.GetStream();

                var buffer = new byte[3];
                BitConverter.GetBytes(count).CopyTo(buffer, 0);
                buffer[2] = length;

                await stream.WriteAsync(buffer, 0, buffer.Length);

                var response = new byte[1];
                await stream.ReadAsync(response, 0, response.Length);

                _logger.
                    
                    ($"[Client {clientId}] Generate: {(response[0] == 1 ? "Success" : "Failure")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Client {clientId}] Error generating codes");
            }
        }

        public async Task UseCodeAsync(int clientId, string code)
        {
            try
            {
                using var client = new TcpClient(_host, _port);
                using var stream = client.GetStream();

                var buffer = Encoding.UTF8.GetBytes(code);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                var response = new byte[1];
                await stream.ReadAsync(response, 0, response.Length);

                _logger.LogInformation($"[Client {clientId}] Use code '{code.Trim()}': {(response[0] == 1 ? "Success" : "Invalid")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Client {clientId}] Error using code");
            }
        }
    }

}


