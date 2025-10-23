namespace Ivy.Open.Raise.Apps.Views;

public class ContactInteractionsBlade(Guid contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var interactions = this.UseState<Interaction[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            interactions.Set(await db.Interactions
                .Include(i => i.User)
                .Include(i => i.InteractionTypeNavigation)
                .Where(i => i.ContactId == contactId)
                .ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this interaction?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Interaction", AlertButtonSet.OkCancel);
            };
        }

        if (interactions.Value == null) return null;

        var table = interactions.Value.Select(i => new
        {
            User = $"{i.User.FirstName} {i.User.LastName}",
            InteractionType = i.InteractionTypeNavigation.Name,
            Subject = i.Subject,
            Notes = i.Notes,
            OccurredAt = i.OccurredAt,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(i.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new ContactInteractionsEditSheet(isOpen, refreshToken, i.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Interaction").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new ContactInteractionsCreateDialog(isOpen, refreshToken, contactId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid interactionId)
    {
        using var db = factory.CreateDbContext();
        db.Interactions.Remove(db.Interactions.Single(i => i.Id == interactionId));
        db.SaveChanges();
    }
}