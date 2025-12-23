namespace Ivy.Open.Raise.Apps.Settings.Users;

public class UserEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var user = UseState<User?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            user.Set(await context.Users.FirstOrDefaultAsync(e => e.Id == userId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return user
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.FirstName, e => e.ToTextAreaInput())
            .Builder(e => e.LastName, e => e.ToTextAreaInput())
            .Builder(e => e.Title, e => e.ToTextAreaInput())
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
            refreshToken.Refresh();
        }
    }
}
