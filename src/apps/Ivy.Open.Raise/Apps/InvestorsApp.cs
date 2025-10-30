using Ivy.Open.Raise.Connections.Data;
using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.User, path: ["Apps"], order:1)]
public class InvestorsApp : ViewBase
{
    private record InvestorRecord(
        Guid Id,
        string Name,
        string InvestorType,
        string? Country,
        string? WebsiteUrl,
        string? LinkedinUrl,
        string? XUrl,
        string CheckSizeRange,
        int ContactsCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var investors = UseState(Enumerable.Empty<InvestorRecord>().AsQueryable());

        // Fetch investors on component mount and when refresh token changes
        UseEffect(() =>
        {
            var fetchedInvestors = FetchInvestors(factory);
            investors.Set(fetchedInvestors);
        }, [EffectTrigger.AfterInit(), refreshToken]);
        
        var createBtn = Icons.Plus.ToButton(_ =>
        {
            // Handle create investor logic here
        }).Outline().Tooltip("Create Investor").ToTrigger((isOpen) => new InvestorCreateDialog(isOpen, refreshToken));

        return Layout.Vertical(
            Layout.Horizontal(
                Text.H3("Investors"),
                createBtn
            ),
            
            investors.Value.ToDataTable()
                .Header(i => i.Name, "Name")
                .Header(i => i.InvestorType, "Type")
                .Header(i => i.Country, "Country")
                .Header(i => i.WebsiteUrl, "Website")
                .Header(i => i.LinkedinUrl, "LinkedIn")
                .Header(i => i.XUrl, "X (Twitter)")
                .Header(i => i.CheckSizeRange, "Check Size")
                .Header(i => i.ContactsCount, "Contacts")
                .Header(i => i.CreatedAt, "Created")
                .Header(i => i.UpdatedAt, "Updated")
                .Header(i => i.Id, "ID")
                
                // Set column widths
                .Width(i => i.Name, Size.Px(200))
                .Width(i => i.InvestorType, Size.Px(120))
                .Width(i => i.Country, Size.Px(120))
                .Width(i => i.WebsiteUrl, Size.Px(150))
                .Width(i => i.LinkedinUrl, Size.Px(150))
                .Width(i => i.XUrl, Size.Px(150))
                .Width(i => i.CheckSizeRange, Size.Px(120))
                .Width(i => i.ContactsCount, Size.Px(100))
                .Width(i => i.CreatedAt, Size.Px(150))
                .Width(i => i.UpdatedAt, Size.Px(150))
                .Width(i => i.Id, Size.Px(100))
                
                // Set alignment
                .Align(i => i.Name, Align.Left)
                .Align(i => i.InvestorType, Align.Left)
                .Align(i => i.Country, Align.Left)
                .Align(i => i.WebsiteUrl, Align.Left)
                .Align(i => i.LinkedinUrl, Align.Left)
                .Align(i => i.XUrl, Align.Left)
                .Align(i => i.CheckSizeRange, Align.Left)
                .Align(i => i.ContactsCount, Align.Center)
                .Align(i => i.CreatedAt, Align.Left)
                .Align(i => i.UpdatedAt, Align.Left)
                .Align(i => i.Id, Align.Left)
                
                // Hide ID column
                .Hidden([i => i.Id])
                
                // Group columns
                .Group(i => i.Name, "Basic Info")
                .Group(i => i.InvestorType, "Basic Info")
                .Group(i => i.Country, "Basic Info")
                .Group(i => i.WebsiteUrl, "Social Links")
                .Group(i => i.LinkedinUrl, "Social Links")
                .Group(i => i.XUrl, "Social Links")
                .Group(i => i.CheckSizeRange, "Investment Info")
                .Group(i => i.ContactsCount, "Investment Info")
                .Group(i => i.CreatedAt, "Timestamps")
                .Group(i => i.UpdatedAt, "Timestamps")
                
                // Configure DataTable
                .Config(config =>
                {
                    config.AllowSorting = true;
                    config.AllowFiltering = true;
                    config.AllowLlmFiltering = true;
                    config.AllowColumnReordering = true;
                    config.AllowColumnResizing = true;
                    config.AllowCopySelection = true;
                    config.SelectionMode = SelectionModes.Cells;
                    config.ShowIndexColumn = true;
                    config.ShowGroups = true;
                    config.ShowColumnTypeIcons = false;
                })
        );
    }

    private IQueryable<InvestorRecord> FetchInvestors(DataContextFactory factory)
    {
        var db = factory.CreateDbContext();

        return db.Investors
            .Include(i => i.InvestorType)
            .Include(i => i.AddressCountry)
            .Include(i => i.Contacts)
            .Where(i => i.DeletedAt == null)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvestorRecord(
                i.Id,
                i.Name,
                i.InvestorType.Name,
                i.AddressCountry != null ? i.AddressCountry.Name : null,
                i.WebsiteUrl,
                i.LinkedinUrl,
                i.XUrl,
                i.CheckSizeMin.HasValue && i.CheckSizeMax.HasValue 
                    ? $"${i.CheckSizeMin:N0} - ${i.CheckSizeMax:N0}"
                    : i.CheckSizeMin.HasValue 
                        ? $"${i.CheckSizeMin:N0}+"
                        : i.CheckSizeMax.HasValue 
                            ? $"Up to ${i.CheckSizeMax:N0}"
                            : "Not specified",
                i.Contacts.Count,
                i.CreatedAt,
                i.UpdatedAt
            ));
    }
}