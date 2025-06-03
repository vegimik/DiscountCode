using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountClient
{
    using System.Net.Sockets;
    using System.Text;

    public class CodeClientService
    {
        private readonly string _host;
        private readonly int _port;

        public CodeClientService(string host = "127.0.0.1", int port = 5000)
        {
            _host = host;
            _port = port;
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

                Console.WriteLine($"[Client {clientId}] Generate: {(response[0] == 1 ? "Success" : "Failure")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {clientId}] Error: {ex.Message}");
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

                Console.WriteLine($"[Client {clientId}] Use code '{code.Trim()}': {(response[0] == 1 ? "Success" : "Invalid")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {clientId}] Error: {ex.Message}");
            }
        }
    }

}


