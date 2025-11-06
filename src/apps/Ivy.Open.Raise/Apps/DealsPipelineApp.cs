using Ivy.Open.Raise.Apps.Deals;
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
        var selectedDealId = UseState((Guid?)null);

        // Fetch deals on component mount and when refresh token changes
        UseEffect(async () =>
        {
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
        }, []);

        // Refresh deals when refresh token changes (for both create and update operations)
        UseEffect(async () =>
        {
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
        }, [refreshToken]);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            // Handle create deal logic here
        }).Ghost().Tooltip("Create Deal").ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        var isEditSheetOpen = UseState(false);

        var kanban = deals.Value
                .ToKanban(
                    groupBySelector: deal => deal.DealStateName,
                    idSelector: deal => deal.Id.ToString(),
                    titleSelector: deal => deal.ContactName,
                    descriptionSelector: deal => FormatDealDescription(deal),
                    orderSelector: deal => deal.Order)
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
                        OwnerName = "Current User",
                        NextAction = null
                    };

                    deals.Set(deals.Value.Append(newDeal).ToArray());

                    // Here you would typically save to database
                    // For now, we'll just update the local state
                })
                .HandleMove(moveData =>
                {
                    var dealId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(dealId)) return;

                    // Get deals in the target column to calculate proper order
                    var dealsInTargetColumn = deals.Value
                        .Where(d => d.DealStateName == moveData.ToColumn)
                        .OrderBy(d => d.Order)
                        .ToList();

                    // Calculate new order based on target index
                    float newOrder;
                    if (moveData.TargetIndex.HasValue && moveData.TargetIndex.Value < dealsInTargetColumn.Count)
                    {
                        // Insert between existing items
                        if (moveData.TargetIndex.Value == 0)
                        {
                            // First position
                            newOrder = dealsInTargetColumn[0].Order - 1;
                        }
                        else
                        {
                            // Between two items
                            var prevOrder = dealsInTargetColumn[moveData.TargetIndex.Value - 1].Order;
                            var nextOrder = dealsInTargetColumn[moveData.TargetIndex.Value].Order;
                            newOrder = (prevOrder + nextOrder) / 2;
                        }
                    }
                    else
                    {
                        // Last position
                        newOrder = dealsInTargetColumn.Count > 0 
                            ? dealsInTargetColumn[^1].Order + 1 
                            : 1;
                    }

                    var updatedDeals = deals.Value.Select(deal =>
                        deal.Id.ToString() == dealId
                            ? new DealRecord
                            {
                                Id = deal.Id,
                                ContactName = deal.ContactName,
                                DealStateName = moveData.ToColumn,
                                Description = deal.Description,
                                Order = newOrder,
                                AmountFrom = deal.AmountFrom,
                                AmountTo = deal.AmountTo,
                                Priority = deal.Priority, // Priority should NOT change when moving
                                OwnerName = deal.OwnerName,
                                NextAction = deal.NextAction
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
                .HandleClick(cardId =>
                {
                    if (Guid.TryParse(cardId?.ToString(), out var dealId))
                    {
                        selectedDealId.Set(dealId);
                        isEditSheetOpen.Set(true);
                    }
                })
                .Empty(
                    new Card()
                        .Title("No Deals")
                        .Description("Create your first deal to get started")
                )
                .Height(Size.Full())
                .Width(Size.Fit());

        // Close the edit sheet when refresh token changes (deal was saved)
        UseEffect(() =>
        {
            if (refreshToken.ReturnValue != null)
            {
                isEditSheetOpen.Set(false);
                selectedDealId.Set((Guid?)null);
            }
        }, [refreshToken]);

        // Sheet is an overlay component - it manages its own visibility via isOpen state
        var editSheet = selectedDealId.Value.HasValue
            ? new DealEditSheet(
                isEditSheetOpen,
                refreshToken,
                selectedDealId.Value!.Value
            )
            : null;

        return Layout.Vertical(
            kanban,
            editSheet
        ).Height(Size.Full());
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
                OwnerName = $"{d.Owner.FirstName} {d.Owner.LastName}",
                NextAction = d.NextAction
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

    private static string FormatDealDescription(DealRecord deal)
    {
        var parts = new List<string>();

        // Format amount range
        if (deal.AmountFrom.HasValue || deal.AmountTo.HasValue)
        {
            var amountStr = deal.AmountFrom.HasValue && deal.AmountTo.HasValue
                ? $"${deal.AmountFrom.Value:N0} - ${deal.AmountTo.Value:N0}"
                : deal.AmountFrom.HasValue
                    ? $"${deal.AmountFrom.Value:N0}+"
                    : $"Up to ${deal.AmountTo.Value:N0}";
            parts.Add(amountStr);
        }

        // Format next action date
        if (deal.NextAction.HasValue)
        {
            var nextActionDate = deal.NextAction.Value;
            var now = DateTime.UtcNow;
            var daysUntil = (nextActionDate.Date - now.Date).Days;

            string dateStr;
            if (daysUntil < 0)
            {
                dateStr = $"Overdue: {nextActionDate:MMM d, yyyy}";
            }
            else if (daysUntil == 0)
            {
                dateStr = "Next action: Today";
            }
            else if (daysUntil == 1)
            {
                dateStr = "Next action: Tomorrow";
            }
            else if (daysUntil <= 7)
            {
                dateStr = $"Next action: {nextActionDate:MMM d} ({daysUntil}d)";
            }
            else
            {
                dateStr = $"Next action: {nextActionDate:MMM d, yyyy}";
            }
            parts.Add(dateStr);
        }

        // Join with HTML line break - amount on first line, next action on second line
        if (parts.Count > 0)
        {
            return string.Join("\n", parts);
        }
        
        return deal.Description;
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
        public DateTime? NextAction { get; set; }
    }
}
