namespace DiscountServer.Models;

public record GenerateRequest(ushort Count, byte Length);
public record UseCodeRequest(string Code);
