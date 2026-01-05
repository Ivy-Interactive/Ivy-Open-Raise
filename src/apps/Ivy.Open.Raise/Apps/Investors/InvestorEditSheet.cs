using Ivy.Hooks;
using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var settingsQuery = Context.UseOrganizationSettings();
        var currency = settingsQuery.Value?.CurrencyId ?? "USD";

        var investorQuery = UseQuery(
            key: (nameof(InvestorEditSheet), investorId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Investors.FirstOrDefaultAsync(e => e.Id == investorId, ct);
            },
            tags: [(typeof(Investor), investorId)]
        );

        if (investorQuery.Loading || investorQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Investor");

        return investorQuery.Value
            .ToForm()
            .Builder(e => e.WebsiteUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Builder(e => e.CheckSizeMin, e => e.ToMoneyInput(currency: currency))
            .Builder(e => e.CheckSizeMax, e => e.ToMoneyInput(currency: currency))
            .Label(e => e.WebsiteUrl, "Website")
            .Label(e => e.LinkedinUrl, "Linkedin Profile")
            .Label(e => e.XUrl, "X Profile")
            .Place(e => e.Name, e => e.InvestorTypeId, e => e.Thesis)
            .PlaceHorizontal(e => e.CheckSizeMin, e => e.CheckSizeMax)
            .Group("Address", e => e.AddressStreet, e => e.AddressZip, e => e.AddressCity, e => e.AddressCountryId)
            .Builder(e => e.Thesis, e => e.ToTextAreaInput())
            .Builder(e => e.AddressCountryId, SelectCountryBuilder)
            .Builder(e => e.InvestorTypeId, SelectInvestorTypeBuilder)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Investor");

        async Task OnSubmit(Investor? modifiedInvestor)
        {
            if (modifiedInvestor == null) return;
            await using var db = factory.CreateDbContext();
            modifiedInvestor.UpdatedAt = DateTime.UtcNow;
            db.Investors.Update(modifiedInvestor);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Investor), investorId));
            refreshToken.Refresh();
        }
    }
}
