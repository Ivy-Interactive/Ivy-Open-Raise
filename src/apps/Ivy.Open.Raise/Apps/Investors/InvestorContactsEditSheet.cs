using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorContactsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var contactQuery = UseQuery(
            key: (nameof(InvestorContactsEditSheet), contactId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Contacts.FirstOrDefaultAsync(e => e.Id == contactId, ct);
            },
            tags: [(typeof(Contact), contactId)]
        );

        if (contactQuery.Loading || contactQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Contact");

        return contactQuery.Value
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
            queryService.RevalidateByTag((typeof(Contact), contactId));
            refreshToken.Refresh();
        }
    }
}
