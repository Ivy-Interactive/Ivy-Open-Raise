namespace Ivy.Open.Raise.Apps.Settings.Views;

public class UserListBlade : ViewBase
{
    private record UserListRecord(Guid Id, string FullName, string Email);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid userId)
            {
                blades.Pop(this, true);
                blades.Push(this, new UserDetailsBlade(userId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var user = (UserListRecord)e.Sender.Tag!;
            blades.Push(this, new UserDetailsBlade(user.Id), user.FullName);
        });

        ListItem CreateItem(UserListRecord record) =>
            new(title: record.FullName, subtitle: record.Email, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create User").ToTrigger((isOpen) => new UserCreateDialog(isOpen, refreshToken));

        return new FilteredListView<UserListRecord>(
            fetchRecords: (filter) => FetchUsers(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<UserListRecord[]> FetchUsers(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.FirstName.Contains(filter) || e.LastName.Contains(filter) || e.Email.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new UserListRecord(e.Id, $"{e.FirstName} {e.LastName}", e.Email))
            .ToArrayAsync();
    }
}