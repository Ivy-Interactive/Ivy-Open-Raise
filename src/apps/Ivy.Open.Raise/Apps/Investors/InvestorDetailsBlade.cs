using Ivy.Open.Raise.Apps.Pipeline;

namespace Ivy.Open.Raise.Apps.Investors;

public record DealDto(Guid Id, string Owner, string Contact, string State, int? AmountFrom, int? AmountTo);

public class InvestorDetailsBlade(Guid investorId) : ViewBase
{
    private Task<DealDto[]> FetchDeals(DataContextFactory factory)
    {
        var db = factory.CreateDbContext();
        
        return db.Deals
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
            .ToArrayAsync();
    }
    
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var investor = UseState<Investor?>();
        var deals = UseState<DealDto[]?>();
        var contacts = UseState<Contact[]?>();
        var currency = UseState<string>();
        var (alertView, showAlert) = this.UseAlert();
        var (editView, showEdit) = this.UseTrigger((isOpen) => new InvestorEditSheet(isOpen, refreshToken, investorId));
        var loading = UseState(true);

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            investor.Set(await db.Investors.Include(e => e.AddressCountry).Include(e => e.InvestorType).SingleOrDefaultAsync(e => e.Id == investorId && e.DeletedAt == null));
            contacts.Set(await db.Contacts.Where(e => e.InvestorId == investorId && e.DeletedAt == null).ToArrayAsync());
            currency.Set(await db.OrganizationSettings.Select(e => e.CurrencyId).FirstOrDefaultAsync());
            deals.Set(await FetchDeals(factory));
            loading.Set(false);
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (loading.Value) return null;

        var investorValue = investor.Value;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete),
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
                CheckSize = Utils.FormatMoneyRange(currency.Value, investorValue.CheckSizeMin, investorValue.CheckSizeMax),
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
        
        var dealCards = deals.Value.Select(e => new DealCardView(e, refreshToken, currency.Value)).ToArray();
        var contactCards = contacts.Value.Select(e => new ContactCardView(e, refreshToken)).ToArray();

        return new Fragment()
               | (Layout.Vertical() | detailsCard | dealCards | contactCards)
               | alertView
               | editView
            ;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this investor?", result =>
            {
                if (!result.IsOk()) return;
                Delete(factory);
                blades.Pop(refresh: true);
            }, "Delete Investor");
        }
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var investor = db.Investors.FirstOrDefault(e => e.Id == investorId)!;
        investor.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}

public class DealCardView(DealDto deal, RefreshToken refreshToken, string currency) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var (alertView, showAlert) = this.UseAlert();
        var (editView, showEdit) = this.UseTrigger((isOpen) => new DealEditSheet(isOpen, refreshToken, deal.Id));

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete),
                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(showEdit)
            );

        var badge = new Badge(deal.State).Outline(); //todo ivy: badges look weird

        var content = new
        {
            Amount = Utils.FormatMoneyRange(currency, deal.AmountFrom, deal.AmountTo),
            deal.Owner,
            deal.Contact
        }.ToDetails();
        
        var body = new Card(content)
            .Title("Deal")
            .Icon(Layout.Horizontal().Gap(1) | badge | dropDown); //todo ivy: icons should take as little space as possible needs to be fixed

        return new Fragment()
               | body
               | alertView
               | editView
            ;
        
        void OnDelete()
        {
            showAlert("Are you sure you want to delete this deal?", result =>
            {
                if (!result.IsOk()) return;
                Delete(factory);
                refreshToken.Refresh();
            }, "Delete Deal");
        }
    }
    
    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var investor = db.Deals.FirstOrDefault(e => e.Id == deal.Id)!;
        investor.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}

public class ContactCardView(Contact contact, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var (alertView, showAlert) = this.UseAlert();
        var (editView, showEdit) = this.UseTrigger((isOpen) => new InvestorContactsEditSheet(isOpen, refreshToken, contact.Id));
        
        List<object> details = [];
            
        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete),
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
               | alertView
               | editView
            ;
        
        void OnDelete()
        {
            showAlert("Are you sure you want to delete this contact?", result =>
            {
                if (!result.IsOk()) return;
                Delete(factory);
                refreshToken.Refresh();
            }, "Delete Contact");
        }
    }
    
    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var investor = db.Contacts.FirstOrDefault(e => e.Id == contact.Id)!;
        investor.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}