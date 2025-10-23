namespace Ivy.Open.Raise.Apps.Views;

public class InteractionTypeDetailsBlade(int interactionTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var interactionType = this.UseState<InteractionType?>();
        var interactionCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            interactionType.Set(await db.InteractionTypes.SingleOrDefaultAsync(e => e.Id == interactionTypeId));
            interactionCount.Set(await db.Interactions.CountAsync(e => e.InteractionType == interactionTypeId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (interactionType.Value == null) return null;

        var interactionTypeValue = interactionType.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this interaction type?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Interaction Type", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new InteractionTypeEditSheet(isOpen, refreshToken, interactionTypeId));

        var detailsCard = new Card(
            content: new
                {
                    interactionTypeValue.Id,
                    interactionTypeValue.Name
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Interaction Type Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Interactions", onClick: _ =>
                {
                    blades.Push(this, new InteractionTypeInteractionsBlade(interactionTypeId), "Interactions");
                }, badge: interactionCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var interactionType = db.InteractionTypes.FirstOrDefault(e => e.Id == interactionTypeId)!;
        db.InteractionTypes.Remove(interactionType);
        db.SaveChanges();
    }
}