namespace Ivy.Open.Raise.Apps.Views;

public class InvestorTypeCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InvestorTypeCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investorTypeState = UseState(() => new InvestorTypeCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var investorTypeId = CreateInvestorType(factory, investorTypeState.Value);
                refreshToken.Refresh(investorTypeId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [investorTypeState]);

        return investorTypeState
            .ToForm()
            .ToDialog(isOpen, title: "Create Investor Type", submitTitle: "Create");
    }

    private int CreateInvestorType(DataContextFactory factory, InvestorTypeCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var investorType = new InvestorType
        {
            Name = request.Name
        };

        db.InvestorTypes.Add(investorType);
        db.SaveChanges();

        return investorType.Id;
    }
}