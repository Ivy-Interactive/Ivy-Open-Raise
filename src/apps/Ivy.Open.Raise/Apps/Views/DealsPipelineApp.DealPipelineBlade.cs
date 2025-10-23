using Ivy.Open.Raise.Connections.Data;

namespace Ivy.Open.Raise.Apps.Views;

public class DealPipelineBlade : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deals = UseState<DealRecord[]>([]);

        // Fetch deals on component mount and when refresh token changes
        UseEffect(async () =>
        {
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
        }, []);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid dealId)
            {
                // Refresh the deals list when a new deal is created
                UseEffect(async () =>
                {
                    var fetchedDeals = await FetchDeals(factory);
                    deals.Set(fetchedDeals);
                }, [refreshToken]);
            }
        }, [refreshToken]);

        var onCardClicked = new Action<Event<object>>(e =>
        {
            if (e.Sender.Tag is DealRecord deal)
            {
                blades.Push(this, new DealDetailsBlade(deal.Id), deal.ContactName);
            }
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Deal").ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        return Layout.Vertical(
            Layout.Horizontal(
                Text.H3("Deal Pipeline"),
                Spacer.Flex(),
                createBtn
            ),
            
            deals.Value
                .ToKanban(
                    groupBySelector: deal => deal.DealStateName,
                    idSelector: deal => deal.Id.ToString(),
                    titleSelector: deal => deal.ContactName,
                    descriptionSelector: deal => deal.Description,
                    orderSelector: deal => deal.Order)
                .Height(Size.Units(400))
                .ColumnOrder(stateName => GetDealStateOrder(stateName))
                .ColumnTitle(stateName => GetCustomColumnTitle(stateName))
                .HandleAdd(columnKey =>
                {
                    // Create a new deal in the specified state
                    var newDeal = new DealRecord
                    {
                        Id = Guid.NewGuid(),
                        ContactName = $"New Deal in {columnKey}",
                        DealStateName = columnKey,
                        Description = $"Auto-generated deal for {columnKey} state",
                        Order = GetNextOrder(columnKey, deals.Value),
                        AmountFrom = null,
                        AmountTo = null,
                        Priority = 1,
                        OwnerName = "Current User"
                    };
                    
                    deals.Set(deals.Value.Append(newDeal).ToArray());
                    
                    // Here you would typically save to database
                    // For now, we'll just update the local state
                })
                .HandleMove(moveData =>
                {
                    var dealId = Guid.Parse(moveData.CardId?.ToString() ?? "");
                    
                    var updatedDeals = deals.Value.Select(deal =>
                        deal.Id == dealId
                            ? new DealRecord
                            {
                                Id = deal.Id,
                                ContactName = deal.ContactName,
                                DealStateName = moveData.ToColumn,
                                Description = deal.Description,
                                Order = moveData.TargetIndex.HasValue ? moveData.TargetIndex.Value + 1 : deal.Order,
                                AmountFrom = deal.AmountFrom,
                                AmountTo = deal.AmountTo,
                                Priority = deal.Priority,
                                OwnerName = deal.OwnerName
                            }
                            : deal
                    ).ToArray();

                    deals.Set(updatedDeals);
                    
                    // Here you would typically save the updated deal to database
                })
                .HandleDelete(cardId =>
                {
                    var dealId = Guid.Parse(cardId?.ToString() ?? "");
                    var updatedDeals = deals.Value.Where(deal => deal.Id != dealId).ToArray();
                    deals.Set(updatedDeals);
                    
                    // Here you would typically delete from database
                })
                .Empty(
                    new Card()
                        .Title("No Deals")
                        .Description("Create your first deal to get started")
                )
        );
    }

    private async Task<DealRecord[]> FetchDeals(DataContextFactory factory)
    {
        await using var db = factory.CreateDbContext();

        return await db.Deals
            .Include(d => d.Contact)
            .Include(d => d.DealState)
            .Include(d => d.Owner)
            .Where(d => d.DeletedAt == null)
            .OrderBy(d => d.Order)
            .Select(d => new DealRecord(
                d.Id,
                $"{d.Contact.FirstName} {d.Contact.LastName}",
                d.DealState.Name,
                d.Notes ?? "No description",
                d.Order,
                d.AmountFrom,
                d.AmountTo,
                d.Priority ?? 1,
                $"{d.Owner.FirstName} {d.Owner.LastName}"
            ))
            .ToArrayAsync();
    }

    private static int GetDealStateOrder(string stateName) => stateName switch
    {
        "Lead" => 1,
        "Qualified" => 2,
        "Proposal" => 3,
        "Negotiation" => 4,
        "Closed Won" => 5,
        "Closed Lost" => 6,
        _ => 0
    };

    private static string GetCustomColumnTitle(string stateName) => stateName switch
    {
        "Lead" => "🔍 Leads",
        "Qualified" => "✅ Qualified",
        "Proposal" => "📋 Proposals",
        "Negotiation" => "🤝 Negotiation",
        "Closed Won" => "🎉 Won",
        "Closed Lost" => "❌ Lost",
        _ => stateName
    };

    private static float GetNextOrder(string columnKey, DealRecord[] deals)
    {
        var dealsInColumn = deals.Where(d => d.DealStateName == columnKey).ToList();
        return dealsInColumn.Count + 1;
    }

    private record DealRecord(
        Guid Id,
        string ContactName,
        string DealStateName,
        string Description,
        float Order,
        int? AmountFrom,
        int? AmountTo,
        int Priority,
        string OwnerName
    );
}
