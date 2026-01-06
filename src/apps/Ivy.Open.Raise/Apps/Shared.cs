using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps;

public static class Shared
{
    public static QueryResult<OrganizationSetting?> UseOrganizationSettings(this IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: typeof(OrganizationSetting),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.OrganizationSettings.FirstOrDefaultAsync(ct);
            }
        );
    }

    public static IAnyInput FileUploadBuilder(IAnyState s, IViewContext v)
    {
        var blobService = v.UseService<IBlobService>();
        var upload = v.UseUpload(BlobUploadHandler.Create(s, blobService, Constants.DeckBlobContainerName, CalculateBlobName))
            .MaxFileSize(Constants.MaxUploadFileSize)
            .Accept(FileTypes.Pdf);
        return s.ToFileInput(upload);
        string CalculateBlobName(FileUpload f) => f.Id + System.IO.Path.GetExtension(f.FileName);
    }

    public static readonly Func<IAnyState, IAnyInput> SelectUserBuilder =
        state => state.ToAsyncSelectInput(UseUserSearch, UseUserLookup, "Select User");

    public static QueryResult<Option<Guid?>[]> UseUserSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseUserSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Users
                        .Where(e => (e.FirstName + " " + e.LastName).Contains(query) || e.Email.Contains(query))
                        .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<Guid?>?> UseUserLookup(IViewContext context, Guid? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseUserLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (user == null) return null;
                return new Option<Guid?>(user.FirstName + " " + user.LastName, user.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectCountryBuilder =
        state => state.ToAsyncSelectInput(UseCountrySearch, UseCountryLookup, "Select Country");

    public static QueryResult<Option<int?>[]> UseCountrySearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCountrySearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Countries
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<int?>?> UseCountryLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCountryLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var country = await db.Countries.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (country == null) return null;
                return new Option<int?>(country.Name, country.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectInvestorTypeBuilder =
        state => state.ToAsyncSelectInput(UseInvestorTypeSearch, UseInvestorTypeLookup, "Select Investor Type");

    public static QueryResult<Option<int?>[]> UseInvestorTypeSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseInvestorTypeSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.InvestorTypes
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<int?>?> UseInvestorTypeLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseInvestorTypeLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var investorType = await db.InvestorTypes.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (investorType == null) return null;
                return new Option<int?>(investorType.Name, investorType.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectContactBuilder =
        state => state.ToAsyncSelectInput(UseContactSearch, UseContactLookup, "Select Contact");

    public static QueryResult<Option<Guid?>[]> UseContactSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseContactSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Contacts.Include(c => c.Investor)

                        .Where(e => (e.FirstName.Contains(query) || e.LastName.Contains(query) || e.Investor.Name.Contains(query)) && e.DeletedAt == null)
                        .Select(e => new
                        {
                            e.Id,
                            FullName = e.FirstName + " " + e.LastName,
                            Investor = e.Investor.Name
                        })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.FullName, e.Id, description:e.Investor))
                    .ToArray();
            });
    }

    public static QueryResult<Option<Guid?>?> UseContactLookup(IViewContext context, Guid? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseContactLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var contact = await db
                    .Contacts
                    .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
                if (contact == null) return null;
                return new Option<Guid?>(contact.FirstName + " " + contact.LastName, contact.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectDealStateBuilder =
        state => state.ToAsyncSelectInput(UseDealStateSearch, UseDealStateLookup, "Select Deal State");

    public static QueryResult<Option<int?>[]> UseDealStateSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealStateSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.DealStates
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<int?>?> UseDealStateLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealStateLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var dealState = await db.DealStates.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (dealState == null) return null;
                return new Option<int?>(dealState.Name, dealState.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectDealApproachBuilder =
        state => state.ToAsyncSelectInput(UseDealApproachSearch, UseDealApproachLookup, "Select Deal Approach");

    public static QueryResult<Option<int?>[]> UseDealApproachSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealApproachSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.DealApproaches
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<int?>?> UseDealApproachLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealApproachLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var dealApproach = await db.DealApproaches.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (dealApproach == null) return null;
                return new Option<int?>(dealApproach.Name, dealApproach.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectCurrencyBuilder =
        state => state.ToAsyncSelectInput(UseCurrencySearch, UseCurrencyLookup, "Select Currency");

    public static QueryResult<Option<string?>[]> UseCurrencySearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCurrencySearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Currencies
                        .Where(e => e.Name.Contains(query) || e.Id.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        //.Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<string?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<string?>?> UseCurrencyLookup(IViewContext context, string? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCurrencyLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var currency = await db.Currencies.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (currency == null) return null;
                return new Option<string?>(currency.Name, currency.Id);
            });
    }

    public static readonly Func<IAnyState, IAnyInput> SelectStartupStageBuilder =
        state => state.ToAsyncSelectInput(UseStartupStageSearch, UseStartupStageLookup, "Select Startup Stage");

    public static QueryResult<Option<int?>[]> UseStartupStageSearch(IViewContext context, string query)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseStartupStageSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.StartupStages
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        //.Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    public static QueryResult<Option<int?>?> UseStartupStageLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseStartupStageLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var startupStage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (startupStage == null) return null;
                return new Option<int?>(startupStage.Name, startupStage.Id);
            });
    }

    public static async Task<Guid> CreateDeckAsync(DataContextFactory factory, string title, FileUpload<BlobInfo> file)
    {
        await using var db = factory.CreateDbContext();

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            Title = title,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        db.Decks.Add(deck);

        var deckVersion = new DeckVersion
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            Name = "Version 1",
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true,
            BlobName = file.Content.BlobName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileName = file.FileName,
        };
        db.DeckVersions.Add(deckVersion);

        var deckLink = new DeckLink
        {
            Id = Guid.NewGuid(),
            Secret = Utils.RandomKey(12),
            Reference = null!,
            ContactId = null,
            DeckId = deck.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.DeckLinks.Add(deckLink);

        await db.SaveChangesAsync();

        return deck.Id;
    }
}
