namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DealStateCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new DealStateCreateRequest());

        return details
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Deal State", submitTitle: "Create");

        async Task OnSubmit(DealStateCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var dealState = new DealState
            {
                Name = request.Name
            };

            db.DealStates.Add(dealState);
            await db.SaveChangesAsync();
            refreshToken.Refresh(dealState.Id);
        }
    }
}
