namespace Ivy.Open.Raise.Apps.Views;

public class DealApproachDetailsBlade(int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var dealApproach = this.UseState<DealApproach?>();
        var dealCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            dealApproach.Set(await db.DealApproaches.SingleOrDefaultAsync(e => e.Id == dealApproachId));
            dealCount.Set(await db.Deals.CountAsync(e => e.DealApproachId == dealApproachId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (dealApproach.Value == null) return null;

        var dealApproachValue = dealApproach.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this deal approach?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Deal Approach", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new DealApproachEditSheet(isOpen, refreshToken, dealApproachId));

        var detailsCard = new Card(
            content: new
                {
                    dealApproachValue.Id,
                    dealApproachValue.Name
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal Approach Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Deals", onClick: _ =>
                {
                    blades.Push(this, new DealApproachDealsBlade(dealApproachId), "Deals");
                }, badge: dealCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var dealApproach = db.DealApproaches.FirstOrDefault(e => e.Id == dealApproachId)!;
        db.DealApproaches.Remove(dealApproach);
        db.SaveChanges();
    }
}