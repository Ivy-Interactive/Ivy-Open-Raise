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
        var dealState = UseState(() => new DealStateCreateRequest());

        UseEffect(() =>
        {
            var dealStateId = CreateDealState(factory, dealState.Value);
            refreshToken.Refresh(dealStateId);
        }, [dealState]);

        return dealState
            .ToForm()
            .ToDialog(isOpen, title: "New Deal State", submitTitle: "Create");
    }

    private int CreateDealState(DataContextFactory factory, DealStateCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var dealState = new DealState
        {
            Name = request.Name
        };

        db.DealStates.Add(dealState);
        db.SaveChanges();

        return dealState.Id;
    }
}
