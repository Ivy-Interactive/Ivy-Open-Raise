namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

public class DealApproachCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DealApproachCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new DealApproachCreateRequest());

        return details
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Deal Approach", submitTitle: "Create");

        async Task OnSubmit(DealApproachCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var dealApproach = new DealApproach
            {
                Name = request.Name
            };

            db.DealApproaches.Add(dealApproach);
            await db.SaveChangesAsync();
            refreshToken.Refresh(dealApproach.Id);
        }
    }
}
