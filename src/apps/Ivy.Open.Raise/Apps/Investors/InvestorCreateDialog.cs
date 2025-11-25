using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InvestorCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int? AddressCountryId { get; init; }

        [Required]
        public int InvestorTypeId { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investor = UseState(() => new InvestorCreateRequest());

        UseEffect(() =>
        {
            var investorId = CreateInvestor(factory, investor.Value);
            refreshToken.Refresh(investorId);
        }, [investor]);

        return investor
            .ToForm()
            .Builder(e => e.AddressCountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .Builder(e => e.InvestorTypeId, e => e.ToAsyncSelectInput(QueryInvestorTypes(factory), LookupInvestorType(factory), placeholder: "Select Investor Type"))
            .ToDialog(isOpen, title: "Create Investor", submitTitle: "Create");
    }

    private Guid CreateInvestor(DataContextFactory factory, InvestorCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var investor = new Investor()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            AddressCountryId = request.AddressCountryId,
            InvestorTypeId = request.InvestorTypeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Investors.Add(investor);
        db.SaveChanges();

        return investor.Id;
    }
}