namespace Ivy.Open.Raise.Apps.Views;

public class ContactListBlade : ViewBase
{
    private record ContactListRecord(Guid Id, string FullName, string Email);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid contactId)
            {
                blades.Pop(this, true);
                blades.Push(this, new ContactDetailsBlade(contactId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var contact = (ContactListRecord)e.Sender.Tag!;
            blades.Push(this, new ContactDetailsBlade(contact.Id), contact.FullName);
        });

        ListItem CreateItem(ContactListRecord record) =>
            new(title: record.FullName, subtitle: record.Email, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Contact").ToTrigger((isOpen) => new ContactCreateDialog(isOpen, refreshToken));

        return new FilteredListView<ContactListRecord>(
            fetchRecords: (filter) => FetchContacts(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<ContactListRecord[]> FetchContacts(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.FirstName.Contains(filter) || e.LastName.Contains(filter) || e.Email.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new ContactListRecord(e.Id, $"{e.FirstName} {e.LastName}", e.Email))
            .ToArrayAsync();
    }
}