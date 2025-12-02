namespace Ivy.Open.Raise.Apps;

public static class Shared
{
    public static IAnyInput FileUploadBuilder(IAnyState s, IViewContext v)
    {
        var blobService = v.UseService<IBlobService>();
        var upload = v.UseUpload(BlobUploadHandler.Create(s, blobService, Constants.DeckBlobContainerName, CalculateBlobName))
            .MaxFileSize(Constants.MaxUploadFileSize)
            .Accept(FileTypes.Pdf);
        return s.ToFileInput(upload);
        string CalculateBlobName(FileUpload f) => f.Id + System.IO.Path.GetExtension(f.FileName);
    }
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectUserBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), "Select User");
    
    public static AsyncSelectQueryDelegate<Guid?> QueryUsers(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => (e.FirstName + " " + e.LastName).Contains(query) || e.Email.Contains(query))
                    .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<Guid?> LookupUser(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<Guid?>(user.FirstName + " " + user.LastName, user.Id);
        };
    }
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectCountryBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), "Select Country");
    
    public static AsyncSelectQueryDelegate<int?> QueryCountries(DataContextFactory factory)
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

    public static AsyncSelectLookupDelegate<int?> LookupCountry(DataContextFactory factory)
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
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectInvestorTypeBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryInvestorTypes(factory), LookupInvestorType(factory), "Select Investor Type");

    public static AsyncSelectQueryDelegate<int?> QueryInvestorTypes(DataContextFactory factory)
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

    public static AsyncSelectLookupDelegate<int?> LookupInvestorType(DataContextFactory factory)
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
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectContactBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), "Select Contact");
    
    public static AsyncSelectQueryDelegate<Guid?> QueryContacts(DataContextFactory factory)
    {
        return async query =>
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
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.FullName, e.Id, description:e.Investor))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<Guid?> LookupContact(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var contact = await db
                .Contacts
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
            if (contact == null) return null;
            return new Option<Guid?>(contact.FirstName + " " + contact.LastName, contact.Id);
        };
    }

    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectDealStateBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryDealStates(factory), LookupDealState(factory), "Select Deal State");
    
    public static AsyncSelectQueryDelegate<int?> QueryDealStates(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.DealStates
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<int?> LookupDealState(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var dealState = await db.DealStates.FirstOrDefaultAsync(e => e.Id == id);
            if (dealState == null) return null;
            return new Option<int?>(dealState.Name, dealState.Id);
        };
    }
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectDealApproachBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryDealApproaches(factory), LookupDealApproach(factory), "Select Deal Approach");
    
    public static AsyncSelectQueryDelegate<int?> QueryDealApproaches(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.DealApproaches
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<int?> LookupDealApproach(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var dealApproach = await db.DealApproaches.FirstOrDefaultAsync(e => e.Id == id);
            if (dealApproach == null) return null;
            return new Option<int?>(dealApproach.Name, dealApproach.Id);
        };
    }
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectCurrencyBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryCurrencies(factory), LookupCurrency(factory), "Select Currency");
    
    public static AsyncSelectQueryDelegate<string?> QueryCurrencies(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Currencies
                    .Where(e => e.Name.Contains(query) || e.Id.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    //.Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<string?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<string?> LookupCurrency(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var currency = await db.Currencies.FirstOrDefaultAsync(e => e.Id == id);
            if (currency == null) return null;
            return new Option<string?>(currency.Name, currency.Id);
        };
    }
    
    public static readonly Func<DataContextFactory, Func<IAnyState,IAnyInput>> SelectStartupStageBuilder = factory => 
        state => state.ToAsyncSelectInput(QueryStartupStages(factory), LookupStartupStage(factory), "Select Startup Stage");
    
    public static AsyncSelectQueryDelegate<int?> QueryStartupStages(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.StartupStages
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    //.Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<int?> LookupStartupStage(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var startupStage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == id);
            if (startupStage == null) return null;
            return new Option<int?>(startupStage.Name, startupStage.Id);
        };
    }
}