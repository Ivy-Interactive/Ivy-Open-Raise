namespace Ivy.Open.Raise.Apps.Views;

public class CountryCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record CountryCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        [StringLength(2)]
        public string Iso { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var countryState = UseState(() => new CountryCreateRequest());

        UseEffect(() =>
        {
            var countryId = CreateCountry(factory, countryState.Value);
            refreshToken.Refresh(countryId);
        }, [countryState]);

        return countryState
            .ToForm()
            .ToDialog(isOpen, title: "Create Country", submitTitle: "Create");
    }

    private int CreateCountry(DataContextFactory factory, CountryCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var country = new Country
        {
            Name = request.Name,
            Iso = request.Iso
        };

        db.Countries.Add(country);
        db.SaveChanges();

        return country.Id;
    }
}