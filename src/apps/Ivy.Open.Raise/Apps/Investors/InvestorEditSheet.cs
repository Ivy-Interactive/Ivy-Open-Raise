namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investor = UseState<Investor?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            investor.Set(await context.Investors.FirstOrDefaultAsync(e => e.Id == investorId));
            loading.Set(false);
        });

        if (loading.Value) return new Loading();

        return investor
            .ToForm()
            .Builder(e => e.WebsiteUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Builder(e => e.Thesis, e => e.ToTextAreaInput())
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
            refreshToken.Refresh();
        }
    }
}