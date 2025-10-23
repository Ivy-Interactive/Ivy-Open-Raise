namespace Ivy.Open.Raise.Apps.Views;

public class InteractionDetailsBlade(Guid interactionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var interaction = UseState<Interaction?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            interaction.Set(await db.Interactions
                .Include(e => e.Contact)
                .Include(e => e.User)
                .Include(e => e.InteractionTypeNavigation)
                .SingleOrDefaultAsync(e => e.Id == interactionId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (interaction.Value == null) return null;

        var interactionValue = interaction.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this interaction?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Interaction", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new InteractionEditSheet(isOpen, refreshToken, interactionId));

        var detailsCard = new Card(
            content: new
            {
                Id = interactionValue.Id,
                ContactName = $"{interactionValue.Contact.FirstName} {interactionValue.Contact.LastName}",
                UserName = $"{interactionValue.User.FirstName} {interactionValue.User.LastName}",
                InteractionType = interactionValue.InteractionTypeNavigation.Name,
                Subject = interactionValue.Subject,
                Notes = interactionValue.Notes,
                OccurredAt = interactionValue.OccurredAt
            }
            .ToDetails()
            .MultiLine(e => e.Notes)
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                | dropDown
                | editBtn
        ).Title("Interaction Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var interaction = db.Interactions.FirstOrDefault(e => e.Id == interactionId)!;
        db.Interactions.Remove(interaction);
        db.SaveChanges();
    }
}