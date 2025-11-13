namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InvestorCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        public string? WebsiteUrl { get; init; }

        public string? LinkedinUrl { get; init; }

        public string? XUrl { get; init; }

        public string? AddressStreet { get; init; }

        public string? AddressZip { get; init; }

        public string? AddressCity { get; init; }

        public int? AddressCountryId { get; init; }

        public int InvestorTypeId { get; init; }

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
            .Builder(e => e.AddressCountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .Builder(e => e.InvestorTypeId, e => e.ToAsyncSelectInput(QueryInvestorTypes(factory), LookupInvestorType(factory), placeholder: "Select Investor Type"))
            .ToDialog(isOpen, title: "Create Investor", submitTitle: "Create");
    }

    private Guid CreateInvestor(DataContextFactory factory, InvestorCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var investor = new Investor()
        {
            Name = request.Name,
            WebsiteUrl = request.WebsiteUrl,
            LinkedinUrl = request.LinkedinUrl,
            XUrl = request.XUrl,
            AddressStreet = request.AddressStreet,
            AddressZip = request.AddressZip,
            AddressCity = request.AddressCity,
            AddressCountryId = request.AddressCountryId,
            InvestorTypeId = request.InvestorTypeId,
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

    private static AsyncSelectQueryDelegate<int?> QueryCountries(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Countries
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupCountry(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var country = await db.Countries.FirstOrDefaultAsync(e => e.Id == id);
            if (country == null) return null;
            return new Option<int?>(country.Name, country.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryInvestorTypes(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.InvestorTypes
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupInvestorType(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var investorType = await db.InvestorTypes.FirstOrDefaultAsync(e => e.Id == id);
            if (investorType == null) return null;
            return new Option<int?>(investorType.Name, investorType.Id);
        };
    }
}