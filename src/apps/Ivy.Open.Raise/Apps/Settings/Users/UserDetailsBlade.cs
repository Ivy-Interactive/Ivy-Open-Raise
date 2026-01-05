using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.Users;

public class UserDetailsBlade(Guid userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(UserDetailsBlade), userId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var user = await db.Users.SingleOrDefaultAsync(e => e.Id == userId, ct);
                var dealCount = await db.Deals.CountAsync(e => e.OwnerId == userId, ct);
                var interactionCount = await db.Interactions.CountAsync(e => e.UserId == userId, ct);
                return (user, dealCount, interactionCount);
            },
            tags: [(typeof(User), userId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.user == null)
            return new Callout($"User '{userId}' not found.").Variant(CalloutVariant.Warning);

        var userValue = query.Value.user;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(User[]));
                    blades.Pop();
                })
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
               | new BladeHeader(Text.Literal($"{userValue.FirstName} {userValue.LastName}"))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var user = await db.Users.FirstOrDefaultAsync(e => e.Id == userId);
        if (user != null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}
