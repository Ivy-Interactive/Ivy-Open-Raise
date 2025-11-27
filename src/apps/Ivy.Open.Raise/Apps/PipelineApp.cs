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
        
        var createBtn = new Button("New Deal").Icon(Icons.Plus).Outline()
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
        ).Scroll(Scroll.None);

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
                    MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(() => OnDelete(deal.Id))
                );
            
            var details = new
            {
                Amount = deal.AmountFormatted(),
                Contact = deal.ContactName,
                Owner = deal.OwnerName
            };

            var content = details.ToDetails();

            return new Card(content)
                .Title(deal.InvestorName)
                .Icon(dropDown)
                .HandleClick(() => showEdit(deal.Id))
                .Key(deal.Id);
        }
        
        void OnDelete(Guid cardId)
        {
            showAlert("Are you sure you want to delete this deal?", (result) =>
            {
                if (result.IsOk())
                { 
                    deals.Set([..deals.Value.Where(d => d.Id != cardId)]);
                    _ = DeleteDeal(factory, cardId); //fire and forget
                }
            }, "Delete Deal");
        }
        
        void OnMove((object? cardId, string toState, int? targetIndex) moveData)
        {
            if (!Guid.TryParse(moveData.cardId?.ToString(), out var dealId)) return;
            deals.Set(MoveDealLocally(deals.Value, dealId, moveData.toState, moveData.targetIndex ?? 0));
            _ = MoveDeal(factory, dealId, moveData.toState, moveData.targetIndex ?? 0); // fire and forget
        }
    }

    private async Task MoveDeal(DataContextFactory factory, Guid dealId, string toState, int targetIndex)
    {
        await using var db = factory.CreateDbContext();

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            var moved = await db.Deals.SingleOrDefaultAsync(d => d.Id == dealId);
            if (moved is null)
                return;

            var toStateId = await db.DealStates
                .Where(ds => ds.Name == toState)
                .Select(ds => (int?)ds.Id)
                .FirstOrDefaultAsync();

            if (toStateId == null)
                return;

            moved.DealStateId = toStateId.Value;

            var destGroup = await db.Deals
                .Where(d => d.DealStateId == toStateId && d.Id != dealId)
                .OrderBy(d => d.Order)
                .ToListAsync();

            targetIndex = Math.Clamp(targetIndex, 0, destGroup.Count);
            destGroup.Insert(targetIndex, moved);

            for (int k = 0; k < destGroup.Count; k++)
            {
                destGroup[k].Order = k;
                destGroup[k].UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        });
    }
    
    ImmutableArray<DealRecord> MoveDealLocally(
        ImmutableArray<DealRecord> items,
        Guid dealId,
        string toState,
        int targetIndex)
    {
        var updatedList = items
            .Select(i => i.Id == dealId ? i with { DealState = toState } : i)
            .ToList();
            
        var movedItem = updatedList.FirstOrDefault(i => i.Id == dealId);
        if (movedItem is null)
            return items;
            
        var group = updatedList
            .Where(i => i.DealState == movedItem.DealState)
            .OrderBy(i => i.Order)
            .ToList();
            
        group.RemoveAll(i => i.Id == dealId);
        targetIndex = Math.Clamp(targetIndex, 0, group.Count);
        group.Insert(targetIndex, movedItem);
            
        for (int k = 0; k < group.Count; k++)
            group[k] = group[k] with { Order = k };
            
        var byId = group.ToDictionary(g => g.Id);
        for (int i = 0; i < updatedList.Count; i++)
        {
            if (byId.TryGetValue(updatedList[i].Id, out var gitem))
                updatedList[i] = gitem;
        }
            
        return [
            ..updatedList
                .OrderBy(i => i.DealState)
                .ThenBy(i => i.Order)
        ];
    }
    
    private async Task DeleteDeal(DataContextFactory factory, Guid dealId)
    {
        await using var db = factory.CreateDbContext();
        
        var deal = await db.Deals.SingleOrDefaultAsync(d => d.Id == dealId);
        if (deal == null) return;
        
        deal.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
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
