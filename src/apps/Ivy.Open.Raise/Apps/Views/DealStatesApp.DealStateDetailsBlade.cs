namespace Ivy.Open.Raise.Apps.Views;

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
            }, "Delete Deal State", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new DealStateEditSheet(isOpen, refreshToken, dealStateId));

        var detailsCard = new Card(
            content: new
            {
                dealStateValue.Id,
                dealStateValue.Name
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal State Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Deals", onClick: _ =>
                {
                    blades.Push(this, new DealStateDealsBlade(dealStateId), "Deals");
                }, badge: dealCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var dealState = db.DealStates.FirstOrDefault(e => e.Id == dealStateId)!;
        db.DealStates.Remove(dealState);
        db.SaveChanges();
    }
}