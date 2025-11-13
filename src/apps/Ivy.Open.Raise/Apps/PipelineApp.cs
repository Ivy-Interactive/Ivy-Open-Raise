using Ivy.Open.Raise.Apps.Pipeline;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Kanban, path: ["Apps"], searchHints: ["pipeline", "kanban", "board", "deals"], title: "Pipeline")]
public class PipelineApp : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deals = UseState<DealRecord[]>([]);
        var selectedDealId = UseState((Guid?)null);
        var isLoading = UseState(true);
        var isEditSheetOpen = UseState(false); //todo: is why is this needed?
        
        UseEffect(async () =>
        {
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
            isLoading.Set(false);
        }, []);
        
        UseEffect(async () =>
        {
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
        }, [refreshToken]);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue != null)
            {
                isEditSheetOpen.Set(false);
                selectedDealId.Set((Guid?)null);
            }
        }, [refreshToken]);
        
        if(isLoading.Value) return Text.Muted("Loading...");
        
        var createBtn = new Button("Create Deal").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        var kanban = deals.Value
            .ToKanban(
                groupBySelector: deal => deal.DealStateName,
                idSelector: deal => deal.Id.ToString(), //todo ivy: does this need to be a string?
                titleSelector: deal => deal.InvestorName, //this is optional
                descriptionSelector: deal => FormatDealDescription(deal), //todo: how is this even working - this is not a selector
                orderSelector: deal => deal.Order) //this is required
            .ColumnOrder(deal => GetDealStateOrder(deal.DealStateName))
            // .CardBuilder(deal =>
            // {
            //     return deal.ContactName;
            //     return new Card(); //here I want to implement my own card 100%
            // })
            .HandleAdd(columnKey =>
            {
                //todo ivy: not implemented?
                
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
            })
            .HandleMove(OnMove)
            .HandleDelete(OnDelete)
            .HandleClick(OnClick);
        
        var editSheet = selectedDealId.Value.HasValue
            ? new DealEditSheet(
                isEditSheetOpen,
                refreshToken,
                selectedDealId.Value!.Value
            )
            : null;

        var header = Layout.Horizontal() | createBtn;

        var body = new HeaderLayout(
            header, 
            kanban
        );

        return new Fragment()
               | body
               | editSheet;
        
        void OnClick(object? cardId)
        {
            if (Guid.TryParse(cardId?.ToString(), out var dealId))
            {
                selectedDealId.Set(dealId);
                isEditSheetOpen.Set(true);
            }
        }

        void OnDelete(object? cardId)
        {
            var dealId = cardId?.ToString();
            if (string.IsNullOrEmpty(dealId)) return;

            var updatedDeals = deals.Value.Where(deal => deal.Id.ToString() != dealId).ToArray();
            deals.Set(updatedDeals);

            //todo: 
        }

        void OnMove((object? CardId, string FromColumn, string ToColumn, int? TargetIndex) moveData)
        {
            var dealId = moveData.CardId?.ToString();
            if (string.IsNullOrEmpty(dealId)) return;

            var dealsInTargetColumn = deals.Value.Where(d => d.DealStateName == moveData.ToColumn)
                .OrderBy(d => d.Order)
                .ToList();
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

            var updatedDeals = deals.Value.Select(deal => deal.Id.ToString() == dealId
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
                    : deal)
                .ToArray();

            deals.Set(updatedDeals);

            //todo: 
        }
    }

    private async Task<DealRecord[]> FetchDeals(DataContextFactory factory)
    {
        await using var db = factory.CreateDbContext();

        return await db.Deals
            .Include(d => d.Contact).ThenInclude(c => c.Investor)
            .Include(d => d.DealState)
            .Include(d => d.Owner)
            .Where(d => d.DeletedAt == null)
            .OrderBy(d => d.Order)
            .Select(d => new DealRecord
            {
                Id = d.Id,
                InvestorName = d.Contact.Investor.Name,
                ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
                DealStateName = d.DealState.Name,
                Description = d.Notes ?? "",
                Order = d.Order,
                AmountFrom = d.AmountFrom,
                AmountTo = d.AmountTo,
                Priority = d.Priority ?? 1,
                OwnerName = $"{d.Owner.FirstName} {d.Owner.LastName}",
                NextAction = d.NextAction
            })
            .ToArrayAsync();
    }

    //todo: get this from db
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

    private static float GetNextOrder(string columnKey, DealRecord[] deals)
    {
        var dealsInColumn = deals.Where(d => d.DealStateName == columnKey).ToList();
        return dealsInColumn.Count + 1;
    }

    // Will not be used -> Remove
    private static string FormatDealDescription(DealRecord deal)
    {
        var parts = new List<string>();
    
        // Format amount range
        if (deal.AmountFrom.HasValue || deal.AmountTo.HasValue)
        {
            var amountStr = deal is { AmountFrom: not null, AmountTo: not null }
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

    private record DealRecord
    {
        public Guid Id { get; init; }
        public string InvestorName { get; init; } = "";
        public string ContactName { get; init; } = "";
        public string DealStateName { get; init; } = "";
        public string Description { get; init; } = "";
        public float Order { get; init; }
        public int? AmountFrom { get; init; }
        public int? AmountTo { get; init; }
        public int Priority { get; init; }
        public string OwnerName { get; init; } = "";
        public DateTime? NextAction { get; init; }
    }
}
