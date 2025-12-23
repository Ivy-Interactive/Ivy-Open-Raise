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
            .PlaceHorizontal(e => e.FirstName, e => e.LastName)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.InvestorId)
            .Label(e => e.LinkedinUrl, "LinkedIn Profile")
            .Label(e => e.XUrl, "X Profile")
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Contact");

        async Task OnSubmit(Contact? modifiedContact)
        {
            if (modifiedContact == null) return;
            await using var db = factory.CreateDbContext();
            modifiedContact.UpdatedAt = DateTime.UtcNow;
            db.Contacts.Update(modifiedContact);
            await db.SaveChangesAsync();
            //refreshToken.Refresh(); //this isn't working
        }
    }
}