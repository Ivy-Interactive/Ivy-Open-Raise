namespace Ivy.Open.Raise.Apps.Views;

public class InvestorContactsBlade(Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var contacts = this.UseState<Contact[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            contacts.Set(await db.Contacts.Where(c => c.InvestorId == investorId && c.DeletedAt == null).ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this contact?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Contact", AlertButtonSet.OkCancel);
            };
        }

        if (contacts.Value == null) return null;

        var table = contacts.Value.Select(c => new
        {
            c.FirstName,
            c.LastName,
            c.Email,
            Title = c.Title ?? string.Empty,
            LinkedinUrl = c.LinkedinUrl ?? string.Empty,
            XUrl = c.XUrl ?? string.Empty,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(c.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new InvestorContactsEditSheet(isOpen, refreshToken, c.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Contact").Icon(Icons.Plus).Ghost()
            .ToTrigger((isOpen) => new InvestorContactsCreateDialog(isOpen, refreshToken, investorId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid contactId)
    {
        using var db = factory.CreateDbContext();
        var contact = db.Contacts.Single(c => c.Id == contactId);
        contact.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}