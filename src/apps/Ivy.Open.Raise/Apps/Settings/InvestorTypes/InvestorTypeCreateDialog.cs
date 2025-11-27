namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

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
        var details = UseState(() => new InvestorTypeCreateRequest());

        return details
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Investor Type", submitTitle: "Create");

        async Task OnSubmit(InvestorTypeCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var investorType = new InvestorType
            {
                Name = request.Name
            };

            db.InvestorTypes.Add(investorType);
            await db.SaveChangesAsync();
            refreshToken.Refresh(investorType.Id);
        }
    }
}
