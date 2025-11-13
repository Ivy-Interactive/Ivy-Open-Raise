using Ivy.Open.Raise.Apps.Investors;
using Ivy.Open.Raise.Connections.Data;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.DollarSign, path: ["Apps"], order: 1)]
public class InvestorsApp : ViewBase
{
    private record InvestorRecord(
        Guid Id,
        string Name,
        string InvestorType,
        string? Country,
        string? WebsiteUrl,
        string CheckSizeRange,
        int ContactsCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public override object? Build()
    {
        //todo ivy: auto format column widths based on content?
        
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        var createBtn = new Button("Create Investor")   
                            .Outline()
                            .Icon(Icons.Plus)
                            .ToTrigger((isOpen) => new InvestorCreateDialog(isOpen, refreshToken));

        var dataTable = 
            BuildInvestorQuery(factory)
                .ToDataTable()
                
                .Renderer(e => e.WebsiteUrl, new LinkDisplayRenderer())  
                
                .Width(i => i.ContactsCount, Size.Px(100))
                .Align(i => i.ContactsCount, Align.Center)
                
                .Hidden(i => i.Id, i => i.CreatedAt, i => i.UpdatedAt)
                
                .RowActions2(
                    MenuItem.Default(Icons.Delete).Tooltip("Delete"),
                    MenuItem.Default(Icons.Pencil).Tooltip("Edit"),
                    MenuItem.Default(Icons.Menu) | MenuItem.Default("Bar") | MenuItem.Default("Foo")
                )
                .HandleRowAction((record, menuItem) =>
                {
                    
                })
                
                .Renderer(e => e.ContactsCount, new ButtonDisplayRenderer())
                .HandleCellAction(e => e.ContactsCount, record =>
                {
                            
                })
                
                .Config(config =>
                {
                });

        var header = Layout.Horizontal() | createBtn;

        return new HeaderLayout(header, dataTable);
    }

    private static IQueryable<InvestorRecord> BuildInvestorQuery(DataContextFactory factory)
    {
        var db = factory.CreateDbContext();

        return db.Investors
            .Include(i => i.InvestorType)
            .Include(i => i.AddressCountry)
            .Include(i => i.Contacts)
            .Where(i => i.DeletedAt == null)
            .OrderByDescending(i => i.CreatedAt) //todo: will this work? 
            .Select(i => new InvestorRecord(
                i.Id,
                i.Name,
                i.InvestorType.Name,
                i.AddressCountry != null ? i.AddressCountry.Name : null,
                i.WebsiteUrl,
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