using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.Users;

public class UserListBlade : ViewBase
{
    public record UserListRecord(Guid Id, string FullName, string Email);

    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var refreshToken = UseRefreshToken();

        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
            blades.Pop(this);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid userId)
            {
                blades.Pop(this);
                blades.Push(this, new UserDetailsBlade(userId));
            }
        }, [refreshToken]);

        var usersQuery = UseUserList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var user = (UserListRecord)e.Sender.Tag!;
            blades.Push(this, new UserDetailsBlade(user.Id), user.FullName);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New User").ToTrigger((isOpen) => new UserCreateDialog(isOpen, refreshToken));

        var items = (usersQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.FullName,
                subtitle: record.Email,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (usersQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<UserListRecord[]> UseUserList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseUserList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.FirstName.Contains(trimmed) || e.LastName.Contains(trimmed) || e.Email.Contains(trimmed));
                }

                return await linq
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(50)
                    .Select(e => new UserListRecord(e.Id, $"{e.FirstName} {e.LastName}", e.Email))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(User[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
