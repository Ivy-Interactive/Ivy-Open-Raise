namespace Ivy.Open.Raise;

public static class Utils
{
    public static string RandomKey(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static string? FormatMoneyRange(string currency, double? from, double? to)
    {
        if(from == null && to == null) return null;
        if(to == null) return FormatMoney(from!.Value);
        if(from == null) return FormatMoney(to.Value);
        return FormatMoney(from.Value) + " - " + FormatMoney(to.Value);
        string FormatMoney(double value)
        {
            return currency + " " + value.ToString("N0");
        }
    }
}