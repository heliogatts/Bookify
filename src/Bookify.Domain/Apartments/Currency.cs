namespace Bookify.Domain.Apartments;

public record Currency
{
    internal static readonly Currency None = new Currency(string.Empty);
    private static readonly Currency Usd = new Currency("USD");
    private static readonly Currency Eur = new Currency("EUR");
    
    private Currency(string code) => Code = code;
    
    public string Code { get; init; }
    
    public static Currency FromCode(string code) => All.FirstOrDefault(c => c.Code == code) 
                                                    ?? throw new ApplicationException($"The currency code is invalid");

    public static readonly IReadOnlyCollection<Currency> All = new[]
    {
        Usd, 
        Eur
    };
}