using DiscountServer.Models;

namespace DiscountServer;

public class DiscountService
{
    private readonly HashSet<string> _codes = new();
    private readonly CodeStorage _storage = new();

    public DiscountService()
    {
        var loaded = _storage.LoadCodes();
        foreach (var code in loaded)
            _codes.Add(code);
    }

    public bool GenerateCodes(ushort count, byte length)
    {
        if (length < 7 || length > 8 || count > 2000) return false;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rand = new Random();
        int added = 0;

        for (int i = 0; i < count; i++)
        {
            var code = new string(Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)]).ToArray());
            if (_codes.Add(code)) added++;
        }

        return added > 0;
    }

    public bool UseCode(string code)
    {
        return _codes.Remove(code.Trim());
    }

    public async Task SaveAsync()
    {
        await _storage.SaveCodesAsync(_codes);
    }

    public List<string> GetAllCodes()
    {
        return _codes.ToList();
    }
}
