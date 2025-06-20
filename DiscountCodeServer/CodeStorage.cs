using System.Text.Json;
using System.Collections.Concurrent;
using System.Threading;

namespace DiscountServer;

public interface ICodeStorage
{
    HashSet<string> LoadCodes();
    Task SaveCodesAsync(HashSet<string> codes);
}

public class CodeStorage : ICodeStorage
{
    private const string FilePath = "codes.json";
    private static readonly SemaphoreSlim _fileSemaphore = new(1, 1);

    public HashSet<string> LoadCodes()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new HashSet<string>();

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? new HashSet<string>()
                : JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    public async Task SaveCodesAsync(HashSet<string> codes)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(codes);
        await _fileSemaphore.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(FilePath, json);
        }
        finally
        {
            _fileSemaphore.Release();
        }
    }
}
