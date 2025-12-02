namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalDetailsBlade(int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var startupVertical = UseState<StartupVertical?>(() => null!);
        var investorCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            startupVertical.Set(await db.StartupVerticals.SingleOrDefaultAsync(e => e.Id == startupVerticalId));
            investorCount.Set(await db.Investors.CountAsync(e => e.StartupVerticals.Any(v => v.Id == startupVerticalId)));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (startupVertical.Value == null) return null;

        var startupVerticalValue = startupVertical.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this startup vertical?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Startup Vertical");
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(investorCount.Value>0).Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new StartupVerticalEditSheet(isOpen, refreshToken, startupVerticalId));

        var detailsCard = new Card(
            content: new
            {
                startupVerticalValue.Name,
                Investors = investorCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Startup Vertical Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();

        var connectedInvestorsCount = db.Investors.Count(e => e.StartupVerticals.Any(v => v.Id == startupVerticalId));

        if (connectedInvestorsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete startup vertical with {connectedInvestorsCount} connected investor(s).");
        }

        var startupVertical = db.StartupVerticals.FirstOrDefault(e => e.Id == startupVerticalId)!;
        db.StartupVerticals.Remove(startupVertical);
        db.SaveChanges();
    }
}
