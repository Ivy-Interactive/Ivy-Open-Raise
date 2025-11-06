namespace Ivy.Open.Raise.Apps.Settings.Views;

public class UserDetailsBlade(Guid userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var user = UseState<User?>();
        var dealCount = UseState<int>();
        var interactionCount = UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            user.Set(await db.Users.SingleOrDefaultAsync(e => e.Id == userId));
            dealCount.Set(await db.Deals.CountAsync(e => e.OwnerId == userId));
            interactionCount.Set(await db.Interactions.CountAsync(e => e.UserId == userId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (user.Value == null) return null;

        var userValue = user.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this user?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete User");
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
            .ToTrigger((isOpen) => new UserEditSheet(isOpen, refreshToken, userId));

        var detailsCard = new Card(
            content: new
            {
                FullName = $"{userValue.FirstName} {userValue.LastName}",
                userValue.Email,
                userValue.Title,
                userValue.LinkedinUrl,
                userValue.XUrl
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Email, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("User Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var user = db.Users.FirstOrDefault(e => e.Id == userId)!;
        db.Users.Remove(user);
        db.SaveChanges();
    }
}