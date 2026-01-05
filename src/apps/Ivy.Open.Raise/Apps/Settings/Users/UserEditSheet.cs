using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.Users;

public class UserEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var userQuery = UseQuery(
            key: (nameof(UserEditSheet), userId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Users.FirstOrDefaultAsync(e => e.Id == userId, ct);
            },
            tags: [(typeof(User), userId)]
        );

        if (userQuery.Loading || userQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit User");

        return userQuery.Value
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.CalendarUrl, e => e.ToUrlInput())
            .Builder(e => e.ProfilePictureUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit User");

        async Task OnSubmit(User? modifiedUser)
        {
            if (modifiedUser == null) return;
            await using var db = factory.CreateDbContext();
            modifiedUser.UpdatedAt = DateTime.UtcNow;
            db.Users.Update(modifiedUser);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(User), userId));
            refreshToken.Refresh();
        }
    }
}
