using System.Text.Json;

namespace DiscountServer;

public class CodeStorage
{
    private const string FilePath = "codes.json";

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
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load codes: {ex.Message}");
            return new HashSet<string>();
        }
    }

    public async Task SaveCodesAsync(HashSet<string> codes)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(codes);
        await File.WriteAllTextAsync(FilePath, json);
    }
}
