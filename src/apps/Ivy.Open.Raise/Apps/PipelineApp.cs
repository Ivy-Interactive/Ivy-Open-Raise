using Ivy.Open.Raise.Apps.Pipeline;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Kanban, path: ["Apps"], searchHints: ["pipeline", "kanban", "board", "deals"], title: "Pipeline")]
public class PipelineApp : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deals = UseState<ImmutableArray<DealRecord>>();
        var isLoading = UseState(true);
        var (editView, showEdit) = this.UseTrigger((IState<bool> isOpen, Guid linkId) 
            => new DealEditSheet(isOpen, refreshToken, linkId));
        var (alertView, showAlert) = this.UseAlert();
        
        UseEffect(async () =>
        {
            isLoading.Set(true);
            var fetchedDeals = await FetchDeals(factory);
            deals.Set(fetchedDeals);
            isLoading.Set(false);
        }, [EffectTrigger.AfterInit(), refreshToken]);
        
        if(isLoading.Value) return Text.Muted("Loading...");
        
        var createBtn = new Button("Create Deal").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        var kanban = deals.Value
            .ToKanban(
                groupBySelector: deal => deal.DealState,
                idSelector: deal => deal.Id,
                orderSelector: deal => deal.Order
            ) 
            .ColumnOrder(deal => deal.DealStateOrder)
            .CardBuilder(CardBuilder)
            .HandleMove(OnMove)
            ;

        var header = Layout.Horizontal() | createBtn;

        var body = new HeaderLayout(
            header, 
            kanban
        );

        return new Fragment()
               | body
               | editView
               | alertView;
        
        object CardBuilder(DealRecord deal)
        {
            var dropDown = Icons.Ellipsis
                .ToButton()
                .Ghost()
                .WithDropDown(
                    MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(() => OnDelete(deal.Id)),
                    MenuItem.Default("Edit").Icon(Icons.Pencil)
                );
            
            var details = new
            {
                Amount = new Button(deal.AmountFormatted()).Inline().HandleClick(() => showEdit(deal.Id)),
                Contact = deal.ContactName,
                Owner = new Button(deal.OwnerName).Inline().HandleClick(() => showEdit(deal.Id))
            };

            var content = details.ToDetails();

            return new Card(content)
                .Title(deal.InvestorName)
                .Icon(dropDown)
                .Key(deal.Id);
        }
        
        void OnDelete(Guid cardId)
        {
            showAlert("Are you sure you want to delete this deal?", (result) =>
            {
                if (result.IsOk())
                { 
                    deals.Set([..deals.Value.Where(d => d.Id != cardId)]);
                    DeleteDeal(factory, cardId); //todo: fire and forget
                }
            }, "Delete Deal");
        }
        
        //todo: include fromStack         
        void OnMove((object? cardId, string toState, int? targetIndex) moveData)
        {
            if (!Guid.TryParse(moveData.cardId?.ToString(), out var dealId)) return;

            // var ds = deals.Value;
            //
            // var deal = deals.Value.FirstOrDefault(d => d.Id == dealId);
            // if(deal == null) return;
            //
            // var updatedList = items
            //     .Select(i => i.Id == id ? i with { State = newState } : i)
            //     .ToList();
            //
            // string fromStack = deal.DealState; //can we get this from moveData?
            //
            // var stacks = deals.Value.GroupBy(e => e.DealState);
            //
            // stacks.Select((stack,group) =>
            // {
            //     if(e => )    
            // })
            //     
            // var dealsInTargetColumn = deals.Value.Where(d => d.DealState == moveData.toState)
            //     .OrderBy(d => d.Order)
            //     .ToList();
                
            //refreshToken.Refresh();
            // float newOrder;
            // if (moveData.TargetIndex.HasValue && moveData.TargetIndex.Value < dealsInTargetColumn.Count)
            // {
            //     // Insert between existing items
            //     if (moveData.TargetIndex.Value == 0)
            //     {
            //         // First position
            //         newOrder = dealsInTargetColumn[0].Order - 1;
            //     }
            //     else
            //     {
            //         // Between two items
            //         var prevOrder = dealsInTargetColumn[moveData.TargetIndex.Value - 1].Order;
            //         var nextOrder = dealsInTargetColumn[moveData.TargetIndex.Value].Order;
            //         newOrder = (prevOrder + nextOrder) / 2;
            //     }
            // }
            // else
            // {
            //     // Last position
            //     newOrder = dealsInTargetColumn.Count > 0
            //         ? dealsInTargetColumn[^1].Order + 1
            //         : 1;
            // }
            //
            // var updatedDeals = deals.Value.Select(deal => deal.Id.ToString() == dealId
            //         ? new DealRecord
            //         {
            //             Id = deal.Id,
            //             ContactName = deal.ContactName,
            //             DealStateName = moveData.ToColumn,
            //             Description = deal.Description,
            //             Order = newOrder,
            //             AmountFrom = deal.AmountFrom,
            //             AmountTo = deal.AmountTo,
            //             Priority = deal.Priority, // Priority should NOT change when moving
            //             OwnerName = deal.OwnerName,
            //             NextAction = deal.NextAction
            //         }
            //         : deal)
            //     .ToArray();
            //
            // deals.Set(updatedDeals);
        
            //todo: 
        }
    }

    private void MoveDeal(DataContextFactory factory, Guid cardId, string toColumn, float newOrder) 
    {
        using var db = factory.CreateDbContext();
        
        var deal = db.Deals
            .Include(d => d.DealState)
            .SingleOrDefault(d => d.Id == cardId);
        if (deal == null) return;
        
        var targetDealState = db.DealStates.SingleOrDefault(ds => ds.Name == toColumn);
        if (targetDealState == null) return;
        
        deal.DealStateId = targetDealState.Id;
        deal.Order = newOrder;
        
        db.SaveChanges();
    }
    
    private void DeleteDeal(DataContextFactory factory, Guid dealId)
    {
        using var db = factory.CreateDbContext();
        
        var deal = db.Deals.SingleOrDefault(d => d.Id == dealId);
        if (deal == null) return;
        
        deal.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
    
    private async Task<ImmutableArray<DealRecord>> FetchDeals(DataContextFactory factory)
    {
        await using var db = factory.CreateDbContext();

        return [
            ..(await db.Deals
                .Include(d => d.Contact).ThenInclude(c => c.Investor)
                .Include(d => d.DealState)
                .Include(d => d.Owner)
                .Where(d => d.DeletedAt == null)
                .Select(d => new DealRecord
                {
                    Id = d.Id,
                    InvestorId = d.Contact.InvestorId,
                    InvestorName = d.Contact.Investor.Name,
                    ContactId = d.ContactId,
                    ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
                    DealState = d.DealState.Name,
                    DealStateOrder = d.DealState.Order,
                    Order = d.Order,
                    AmountFrom = d.AmountFrom,
                    AmountTo = d.AmountTo,
                    Priority = d.Priority ?? 1,
                    OwnerName = $"{d.Owner.FirstName} {d.Owner.LastName}",
                    NextAction = d.NextAction
                })
                .OrderBy(e => e.DealStateOrder).ThenBy(e => e.Order)
                .ToArrayAsync())
        ];
    }
    
    private record DealRecord
    {
        public Guid Id { get; init; }
        public Guid ContactId { get; init; }
        public Guid InvestorId { get; init; }
        public string InvestorName { get; init; }
        public string OwnerName { get; init; }
        public string ContactName { get; init; }
        public string DealState { get; init; }
        public int DealStateOrder { get; init; }
        public float Order { get; init; }
        public int? AmountFrom { get; init; }
        public int? AmountTo { get; init; }
        public int Priority { get; init; }
        public DateTime? NextAction { get; init; }

        public string AmountFormatted()
        {
            string amount = "(not specified)";

            if (AmountFrom.HasValue)
            {
                amount = Ivy.Utils.FormatNumber(AmountFrom.Value, 1);
                
                if (AmountTo.HasValue && AmountTo.Value != AmountFrom.Value)
                {
                    amount = $"{amount} - {Ivy.Utils.FormatNumber(AmountFrom.Value, 1)}";
                }
            }
            
            return amount;
        }
    }
}
