namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateDetailsBlade(int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var dealState = this.UseState<DealState?>();
        var dealCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            dealState.Set(await db.DealStates.SingleOrDefaultAsync(e => e.Id == dealStateId));
            dealCount.Set(await db.Deals.CountAsync(e => e.DealStateId == dealStateId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (dealState.Value == null) return null;

        var dealStateValue = dealState.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this deal state?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Deal State");
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(dealCount.Value>0).Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new DealStateEditSheet(isOpen, refreshToken, dealStateId));

        var detailsCard = new Card(
            content: new
            {
                dealStateValue.Name,
                dealStateValue.Order,
                Deals = dealCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal State Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();

        var connectedDealsCount = db.Deals.Count(e => e.DealStateId == dealStateId);

        if (connectedDealsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete deal state with {connectedDealsCount} connected deal(s).");
        }

        var dealState = db.DealStates.FirstOrDefault(e => e.Id == dealStateId)!;
        db.DealStates.Remove(dealState);
        db.SaveChanges();
    }
}
