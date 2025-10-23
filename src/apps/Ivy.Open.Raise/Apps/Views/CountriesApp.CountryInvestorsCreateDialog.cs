namespace Ivy.Open.Raise.Apps.Views;

public class CountryInvestorsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int? addressCountryId) : ViewBase
{
    private record InvestorCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int InvestorTypeId { get; init; }

        public string? WebsiteUrl { get; init; }
        public string? LinkedinUrl { get; init; }
        public string? XUrl { get; init; }
        public string? AddressStreet { get; init; }
        public string? AddressZip { get; init; }
        public string? AddressCity { get; init; }
        public string? Thesis { get; init; }
        public int? CheckSizeMin { get; init; }
        public int? CheckSizeMax { get; init; }
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
            .Builder(e => e.InvestorTypeId, e => e.ToAsyncSelectInput(QueryInvestorTypes(factory), LookupInvestorType(factory), placeholder: "Select Investor Type"))
            .Builder(e => e.WebsiteUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Builder(e => e.CheckSizeMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.CheckSizeMax, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Investor", submitTitle: "Create");
    }

    private Guid CreateInvestor(DataContextFactory factory, InvestorCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var investor = new Investor
        {
            Name = request.Name,
            InvestorTypeId = request.InvestorTypeId,
            WebsiteUrl = request.WebsiteUrl,
            LinkedinUrl = request.LinkedinUrl,
            XUrl = request.XUrl,
            AddressStreet = request.AddressStreet,
            AddressZip = request.AddressZip,
            AddressCity = request.AddressCity,
            AddressCountryId = addressCountryId,
            Thesis = request.Thesis,
            CheckSizeMin = request.CheckSizeMin,
            CheckSizeMax = request.CheckSizeMax,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Investors.Add(investor);
        db.SaveChanges();

        return investor.Id;
    }

    private static AsyncSelectQueryDelegate<int> QueryInvestorTypes(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.InvestorTypes
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupInvestorType(DataContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var investorType = await db.InvestorTypes.FirstOrDefaultAsync(e => e.Id == id);
            if (investorType == null) return null;
            return new Option<int>(investorType.Name, investorType.Id);
        };
    }
}