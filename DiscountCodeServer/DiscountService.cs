using DiscountServer.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DiscountServer;

public class DiscountService
{
    private readonly ConcurrentDictionary<string, byte> _codes = new();
    private readonly ICodeStorage _storage;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(ICodeStorage storage, ILogger<DiscountService> logger)
    {
        _storage = storage;
        _logger = logger;
        var loaded = _storage.LoadCodes();
        foreach (var code in loaded)
            _codes.TryAdd(code, 0);
    }

    public bool GenerateCodes(ushort count, byte length)
    {
        if (length < 7 || length > 8 || count > 2000) return false;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rand = new Random();
        int added = 0;
        int attempts = 0;
        int maxAttempts = count * 10;
        while (added < count && attempts < maxAttempts)
        {
            var code = new string(Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)]).ToArray());
            if (_codes.TryAdd(code, 0)) added++;
            attempts++;
        }
        if (added < count)
        {
            _logger.LogWarning($"Could not generate the requested number of unique codes. Requested: {count}, Generated: {added}");
        }
        return added > 0;
    }

    public bool UseCode(string code)
    {
        return _codes.TryRemove(code.Trim(), out _);
    }

    public async Task SaveAsync()
    {
        await _storage.SaveCodesAsync(_codes.Keys.ToHashSet());
    }

    public List<string> GetAllCodes()
    {
        return _codes.Keys.ToList();
    }
}
