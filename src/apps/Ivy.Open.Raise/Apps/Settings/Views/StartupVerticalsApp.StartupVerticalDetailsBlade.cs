namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupVerticalDetailsBlade(int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var startupVertical = UseState<StartupVertical?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            startupVertical.Set(await db.StartupVerticals.SingleOrDefaultAsync(e => e.Id == startupVerticalId));
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
            }, "Delete Startup Vertical", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new StartupVerticalEditSheet(isOpen, refreshToken, startupVerticalId));

        var detailsCard = new Card(
            content: new
            {
                Id = startupVerticalValue.Id,
                Name = startupVerticalValue.Name
            }
            .ToDetails()
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
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
        var startupVertical = db.StartupVerticals.FirstOrDefault(e => e.Id == startupVerticalId)!;
        db.StartupVerticals.Remove(startupVertical);
        db.SaveChanges();
    }
}