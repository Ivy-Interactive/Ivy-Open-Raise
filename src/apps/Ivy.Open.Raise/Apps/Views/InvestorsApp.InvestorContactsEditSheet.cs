namespace Ivy.Open.Raise.Apps.Views;

public class InvestorContactsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var contact = UseState(() => factory.CreateDbContext().Contacts.FirstOrDefault(e => e.Id == contactId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            contact.Value.UpdatedAt = DateTime.UtcNow;
            db.Contacts.Update(contact.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [contact]);

        return contact
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Place(e => e.FirstName, e => e.LastName)
            .Group("Contact Details", e => e.Email, e => e.Title, e => e.LinkedinUrl, e => e.XUrl)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.InvestorId)
            .ToSheet(isOpen, "Edit Contact");
    }
}