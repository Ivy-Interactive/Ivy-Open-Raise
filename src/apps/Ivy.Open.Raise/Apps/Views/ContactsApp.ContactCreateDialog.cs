namespace Ivy.Open.Raise.Apps.Views;

public class ContactCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record ContactCreateRequest
    {
        [Required]
        public string Email { get; init; } = "";

        [Required]
        public Guid InvestorId { get; init; }

        [Required]
        public string FirstName { get; init; } = "";

        [Required]
        public string LastName { get; init; } = "";

        public string? Title { get; init; }

        public string? LinkedinUrl { get; init; }

        public string? XUrl { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var contact = UseState(() => new ContactCreateRequest());

        UseEffect(() =>
        {
            var contactId = CreateContact(factory, contact.Value);
            refreshToken.Refresh(contactId);
        }, [contact]);

        return contact
            .ToForm()
            .Builder(e => e.InvestorId, e => e.ToAsyncSelectInput(QueryInvestors(factory), LookupInvestor(factory), placeholder: "Select Investor"))
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .ToDialog(isOpen, title: "Create Contact", submitTitle: "Create");
    }

    private Guid CreateContact(DataContextFactory factory, ContactCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            InvestorId = request.InvestorId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Title = request.Title,
            LinkedinUrl = request.LinkedinUrl,
            XUrl = request.XUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Contacts.Add(contact);
        db.SaveChanges();

        return contact.Id;
    }

    private static AsyncSelectQueryDelegate<Guid> QueryInvestors(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Investors
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid> LookupInvestor(DataContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var investor = await db.Investors.FirstOrDefaultAsync(e => e.Id == id);
            if (investor == null) return null;
            return new Option<Guid>(investor.Name, investor.Id);
        };
    }
}