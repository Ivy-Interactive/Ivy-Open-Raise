namespace Ivy.Open.Raise.Apps.Settings.Views;

public class UserEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var user = UseState(() => factory.CreateDbContext().Users.FirstOrDefault(e => e.Id == userId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            user.Value.UpdatedAt = DateTime.UtcNow;
            db.Users.Update(user.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [user]);

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
            .ToSheet(isOpen, "Edit User");
    }
}