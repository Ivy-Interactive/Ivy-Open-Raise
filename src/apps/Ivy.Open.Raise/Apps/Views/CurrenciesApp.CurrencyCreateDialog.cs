namespace Ivy.Open.Raise.Apps.Views;

public class CurrencyCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record CurrencyCreateRequest
    {
        [Required]
        [StringLength(3)]
        public string Id { get; init; } = "";

        [Required]
        public string Symbol { get; init; } = "";

        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var currencyState = UseState(() => new CurrencyCreateRequest());

        UseEffect(() =>
        {
            CreateCurrency(factory, currencyState.Value);
            refreshToken.Refresh();
        }, [currencyState]);

        return currencyState
            .ToForm()
            .ToDialog(isOpen, title: "Create Currency", submitTitle: "Create");
    }

    private void CreateCurrency(DataContextFactory factory, CurrencyCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var currency = new Currency
        {
            Id = request.Id,
            Symbol = request.Symbol,
            Name = request.Name
        };

        db.Currencies.Add(currency);
        db.SaveChanges();
    }
}