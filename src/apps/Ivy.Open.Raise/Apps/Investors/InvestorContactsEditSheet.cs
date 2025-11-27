namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorContactsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<Contact?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.Contacts.FirstOrDefaultAsync(e => e.Id == contactId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Place(e => e.FirstName, e => e.LastName)
            .Group("Contact Details", e => e.Email, e => e.Title, e => e.LinkedinUrl, e => e.XUrl)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.InvestorId)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Contact");

        async Task OnSubmit(Contact? contact)
        {
            if (contact == null) return;
            await using var db = factory.CreateDbContext();
            contact.UpdatedAt = DateTime.UtcNow;
            db.Contacts.Update(contact);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}