using Ivy.Open.Raise.Apps.Views;
using Ivy.Open.Raise.Connections.Data;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Kanban, path: ["Apps"], searchHints: ["pipeline", "kanban", "board"])]
public class DealsPipelineApp : ViewBase
{
    public override object? Build()
    {
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

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            // Handle create deal logic here
        }).Outline().Tooltip("Create Deal").ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        return deals.Value
                .ToKanban(
                    groupBySelector: deal => deal.DealStateName,
                    idSelector: deal => deal.Id.ToString(),
                    titleSelector: deal => deal.ContactName,
                    descriptionSelector: deal => deal.Description,
                    orderSelector: deal => deal.Order)
                .Height(Size.Units(400))
                .ColumnOrder(deal => GetDealStateOrder(deal.DealStateName))
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
                    var dealId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(dealId)) return;

                    var updatedDeals = deals.Value.Select(deal =>
                        deal.Id.ToString() == dealId
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
                    var dealId = cardId?.ToString();
                    if (string.IsNullOrEmpty(dealId)) return;

                    var updatedDeals = deals.Value.Where(deal => deal.Id.ToString() != dealId).ToArray();
                    deals.Set(updatedDeals);

                    // Here you would typically delete from database
                })
                .Empty(
                    new Card()
                        .Title("No Deals")
                        .Description("Create your first deal to get started")
                )
                .Height(Size.Full())
                .Width(Size.Fit());
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
            .Select(d => new DealRecord
            {
                Id = d.Id,
                ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
                DealStateName = d.DealState.Name,
                Description = d.Notes ?? "No description",
                Order = d.Order,
                AmountFrom = d.AmountFrom,
                AmountTo = d.AmountTo,
                Priority = d.Priority ?? 1,
                OwnerName = $"{d.Owner.FirstName} {d.Owner.LastName}"
            })
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
        _ => stateName
    };

    private static float GetNextOrder(string columnKey, DealRecord[] deals)
    {
        var dealsInColumn = deals.Where(d => d.DealStateName == columnKey).ToList();
        return dealsInColumn.Count + 1;
    }

    private class DealRecord
    {
        public Guid Id { get; set; }
        public string ContactName { get; set; } = "";
        public string DealStateName { get; set; } = "";
        public string Description { get; set; } = "";
        public float Order { get; set; }
        public int? AmountFrom { get; set; }
        public int? AmountTo { get; set; }
        public int Priority { get; set; }
        public string OwnerName { get; set; } = "";
    }
}
