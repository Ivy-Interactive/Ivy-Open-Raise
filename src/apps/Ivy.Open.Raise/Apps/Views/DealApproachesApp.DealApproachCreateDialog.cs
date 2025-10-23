namespace Ivy.Open.Raise.Apps.Views;

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
        var dealApproach = UseState(() => new DealApproachCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var dealApproachId = CreateDealApproach(factory, dealApproach.Value);
                refreshToken.Refresh(dealApproachId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [dealApproach]);

        return dealApproach
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "Create Deal Approach", submitTitle: "Create");
    }

    private int CreateDealApproach(DataContextFactory factory, DealApproachCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var dealApproach = new DealApproach()
        {
            Name = request.Name
        };

        db.DealApproaches.Add(dealApproach);
        db.SaveChanges();

        return dealApproach.Id;
    }
}