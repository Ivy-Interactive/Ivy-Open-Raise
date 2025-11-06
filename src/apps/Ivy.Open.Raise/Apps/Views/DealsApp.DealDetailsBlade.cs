namespace Ivy.Open.Raise.Apps.Views;

public class DealDetailsBlade(Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var deal = UseState<Deal?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            deal.Set(await db.Deals
                .Include(e => e.Contact)
                .Include(e => e.DealState)
                .SingleOrDefaultAsync(e => e.Id == dealId && e.DeletedAt == null));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (deal.Value == null) return null;

        var dealValue = deal.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this deal?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Deal", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new DealEditSheet(isOpen, refreshToken, dealId));

        var detailsCard = new Card(
            content: new
            {
                Id = dealValue.Id,
                FullName = $"{dealValue.Contact.FirstName} {dealValue.Contact.LastName}",
                DealState = dealValue.DealState.Name,
                AmountFrom = dealValue.AmountFrom,
                AmountTo = dealValue.AmountTo,
                Notes = dealValue.Notes,
                NextAction = dealValue.NextAction,
                NextActionNotes = dealValue.NextActionNotes
            }
            .ToDetails()
            .MultiLine(e => e.Notes)
            .RemoveEmpty(),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal Details").Width(Size.Units(140));

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var deal = db.Deals.FirstOrDefault(e => e.Id == dealId)!;
        deal.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}