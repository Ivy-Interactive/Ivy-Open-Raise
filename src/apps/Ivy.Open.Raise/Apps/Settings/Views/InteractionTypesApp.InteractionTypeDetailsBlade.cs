namespace Ivy.Open.Raise.Apps.Settings.Views;

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
            }, "Delete Interaction Type");
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(interactionCount.Value>0).Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new InteractionTypeEditSheet(isOpen, refreshToken, interactionTypeId));

        var detailsCard = new Card(
            content: new
            {
                interactionTypeValue.Name,
                Interactions = interactionCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Interaction Type Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();

        var connectedInteractionsCount = db.Interactions.Count(e => e.InteractionType == interactionTypeId);

        if (connectedInteractionsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete interaction type with {connectedInteractionsCount} connected interaction(s).");
        }

        var interactionType = db.InteractionTypes.FirstOrDefault(e => e.Id == interactionTypeId)!;
        db.InteractionTypes.Remove(interactionType);
        db.SaveChanges();
    }
}