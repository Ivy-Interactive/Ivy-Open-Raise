using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InvestorCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int? AddressCountryId { get; init; }

        [Required]
        public int InvestorTypeId { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new InvestorCreateRequest());

        return details
            .ToForm()
            .Label(e => e.AddressCountryId, "Country")
            .Label(e => e.InvestorTypeId, "Type")
            .Builder(e => e.AddressCountryId, e => e.ToAsyncSelectInput(UseCountrySearch, UseCountryLookup, placeholder: "Select Country"))
            .Builder(e => e.InvestorTypeId, e => e.ToAsyncSelectInput(UseInvestorTypeSearch, UseInvestorTypeLookup, placeholder: "Select Type"))
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Investor", submitTitle: "Create");

        async Task OnSubmit(InvestorCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var investor = new Investor
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                AddressCountryId = request.AddressCountryId,
                InvestorTypeId = request.InvestorTypeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Investors.Add(investor);
            await db.SaveChangesAsync();
            refreshToken.Refresh(investor.Id);
        }
    }
}