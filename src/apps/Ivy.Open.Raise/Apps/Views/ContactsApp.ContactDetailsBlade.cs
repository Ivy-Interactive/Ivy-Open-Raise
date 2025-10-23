namespace Ivy.Open.Raise.Apps.Views;

public class ContactDetailsBlade(Guid contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var contact = UseState<Contact?>();
        var dealCount = UseState<int>();
        var deckLinkCount = UseState<int>();
        var interactionCount = UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            contact.Set(await db.Contacts.SingleOrDefaultAsync(e => e.Id == contactId));
            dealCount.Set(await db.Deals.CountAsync(e => e.ContactId == contactId));
            deckLinkCount.Set(await db.DeckLinks.CountAsync(e => e.ContactId == contactId));
            interactionCount.Set(await db.Interactions.CountAsync(e => e.ContactId == contactId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (contact.Value == null) return null;

        var contactValue = contact.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this contact?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Contact", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new ContactEditSheet(isOpen, refreshToken, contactId));

        var detailsCard = new Card(
            content: new
                {
                    FullName = $"{contactValue.FirstName} {contactValue.LastName}",
                    contactValue.Email,
                    contactValue.Title,
                    contactValue.LinkedinUrl,
                    contactValue.XUrl
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Email, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Contact Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Deals", onClick: _ =>
                {
                    blades.Push(this, new ContactDealsBlade(contactId), "Deals");
                }, badge: dealCount.Value.ToString("N0")),
                new ListItem("Deck Links", onClick: _ =>
                {
                    blades.Push(this, new ContactDeckLinksBlade(contactId), "Deck Links");
                }, badge: deckLinkCount.Value.ToString("N0")),
                new ListItem("Interactions", onClick: _ =>
                {
                    blades.Push(this, new ContactInteractionsBlade(contactId), "Interactions");
                }, badge: interactionCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var contact = db.Contacts.FirstOrDefault(e => e.Id == contactId)!;
        db.Contacts.Remove(contact);
        db.SaveChanges();
    }
}