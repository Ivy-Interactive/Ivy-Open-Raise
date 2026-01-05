using Ivy.Hooks;
using Ivy.Open.Raise.Apps.Pipeline;

namespace Ivy.Open.Raise.Apps.Investors;

public record DealDto(Guid Id, string Owner, string Contact, string State, int? AmountFrom, int? AmountTo);

public class InvestorDetailsBlade(Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();
        var (editView, showEdit) = UseTrigger((isOpen) => new InvestorEditSheet(isOpen, refreshToken, investorId));

        var investorQuery = UseQuery(
            key: (nameof(InvestorDetailsBlade), investorId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var investor = await db.Investors
                    .Include(e => e.AddressCountry)
                    .Include(e => e.InvestorType)
                    .SingleOrDefaultAsync(e => e.Id == investorId && e.DeletedAt == null, ct);
                var contacts = await db.Contacts
                    .Where(e => e.InvestorId == investorId && e.DeletedAt == null)
                    .ToArrayAsync(ct);
                var currency = await db.OrganizationSettings
                    .Select(e => e.CurrencyId)
                    .FirstOrDefaultAsync(ct);
                var deals = await db.Deals
                    .Include(e => e.Contact)
                    .Include(e => e.Owner)
                    .Include(e => e.DealState)
                    .Where(e => e.Contact.InvestorId == investorId && e.DeletedAt == null)
                    .Select(e => new DealDto(
                        e.Id,
                        (e.Owner.FirstName + " " + e.Owner.LastName).Trim(),
                        (e.Contact.FirstName + " " + e.Contact.LastName).Trim(),
                        e.DealState.Name,
                        e.AmountFrom,
                        e.AmountTo
                    ))
                    .ToArrayAsync(ct);
                return (investor, contacts, currency, deals);
            },
            tags: [(typeof(Investor), investorId)]
        );

        if (investorQuery.Loading) return Skeleton.Card();
        if (investorQuery.Value.investor == null)
            return new Callout($"Investor '{investorId}' not found.").Variant(CalloutVariant.Warning);

        var investorValue = investorQuery.Value.investor;
        var contacts = investorQuery.Value.contacts;
        var currency = investorQuery.Value.currency ?? "USD";
        var deals = investorQuery.Value.deals;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(Investor[]));
                    blades.Pop();
                }),
                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(showEdit)
            );

        var addDealBtn = new Button("Add Deal")
            .Icon(Icons.Plus)
            .Outline()
            .ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        var addContactBtn = new Button("Add Contact")
            .Icon(Icons.UserPlus)
            .Outline()
            .ToTrigger((isOpen) => new InvestorContactsCreateDialog(isOpen, refreshToken, investorId: investorId));

        List<Button> socials = [];

        if (!string.IsNullOrEmpty(investorValue.WebsiteUrl))
            socials.Add(new Button().Icon(Icons.Globe).Ghost().Url(investorValue.WebsiteUrl!));

        if (!string.IsNullOrEmpty(investorValue.LinkedinUrl))
            socials.Add(new Button().Icon(Icons.Linkedin).Ghost().Url(investorValue.LinkedinUrl!));

        if (!string.IsNullOrEmpty(investorValue.XUrl))
            socials.Add(new Button().Icon(Icons.XTwitter).Ghost().Url(investorValue.XUrl!));

        var details = new
            {
                investorValue.Name,
                Type = investorValue.InvestorType.Name,
                Address = (Layout.Vertical().Align(Align.Right).Gap(0)
                           | investorValue.AddressStreet
                           | (investorValue.AddressZip + " " + investorValue.AddressCity).Trim().NullIfEmpty()
                           | investorValue.AddressCountry?.Name),
                investorValue.Thesis,
                CheckSize = Utils.FormatMoneyRange(currency, investorValue.CheckSizeMin, investorValue.CheckSizeMax),
            }
            .ToDetails()
            .MultiLine(e => e.Thesis)
            .RemoveEmpty();

        var detailsCard = new Card(
            content: details,
            footer: Layout.Horizontal().Gap(1).Align(Align.Right)
                    | addDealBtn
                    | addContactBtn
        )
            .Title("Investor Details")
            .Icon(Layout.Horizontal().Gap(0).Align(Align.Right) | socials | dropDown).Width(Size.Units(100));

        var dealCards = deals.Select(e => new DealCardView(e, refreshToken, currency)).ToArray();
        var contactCards = contacts.Select(e => new ContactCardView(e, refreshToken)).ToArray();

        return new Fragment()
               | new BladeHeader(Text.Literal(investorValue.Name))
               | (Layout.Vertical() | detailsCard | dealCards | contactCards)
               | editView
            ;
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var investor = await db.Investors.FirstOrDefaultAsync(e => e.Id == investorId);
        if (investor != null)
        {
            investor.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}

public class DealCardView(DealDto deal, RefreshToken refreshToken, string currency) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();
        var (editView, showEdit) = UseTrigger((isOpen) => new DealEditSheet(isOpen, refreshToken, deal.Id));

        var deleteBtn = MenuItem.Default("Delete")
            .Icon(Icons.Trash)
            .HandleSelect(async () =>
            {
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(Deal[]));
                refreshToken.Refresh();
            });

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                deleteBtn,
                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(showEdit)
            );

        var badge = new Badge(deal.State).Outline();

        var content = new
        {
            Amount = Utils.FormatMoneyRange(currency, deal.AmountFrom, deal.AmountTo),
            deal.Owner,
            deal.Contact
        }.ToDetails();

        var body = new Card(content)
            .Title("Deal")
            .Icon(Layout.Horizontal().Gap(1) | badge | dropDown);

        return new Fragment()
               | body
               | editView
            ;
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var dealEntity = await db.Deals.FirstOrDefaultAsync(e => e.Id == deal.Id);
        if (dealEntity != null)
        {
            dealEntity.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}

public class ContactCardView(Contact contact, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();
        var (editView, showEdit) = UseTrigger((isOpen) => new InvestorContactsEditSheet(isOpen, refreshToken, contact.Id));

        List<object> details = [];

        var deleteBtn = MenuItem.Default("Delete")
            .Icon(Icons.Trash)
            .HandleSelect(async () =>
            {
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(Contact[]));
                refreshToken.Refresh();
            });

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                deleteBtn,
                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(showEdit)
            );

        if (!string.IsNullOrEmpty(contact.Email))
            details.Add(new Button().Icon(Icons.Mail).Ghost().Url("mailto:" + contact.Email));

        if (!string.IsNullOrEmpty(contact.LinkedinUrl))
            details.Add(new Button().Icon(Icons.Linkedin).Ghost().Url(contact.LinkedinUrl!));

        if (!string.IsNullOrEmpty(contact.XUrl))
            details.Add(new Button().Icon(Icons.XTwitter).Ghost().Url(contact.XUrl!));

        var content = Layout.Vertical().Gap(2)
                      | (!string.IsNullOrEmpty(contact.Title) ? Text.Muted(contact.Title) : null)
            ;

        var body = new Card(content)
            .Title(contact.FirstName + " " + contact.LastName)
            .Icon(Layout.Horizontal().Gap(0).Align(Align.Right) | details | dropDown);

        return new Fragment()
               | body
               | editView
            ;
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var contactEntity = await db.Contacts.FirstOrDefaultAsync(e => e.Id == contact.Id);
        if (contactEntity != null)
        {
            contactEntity.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
