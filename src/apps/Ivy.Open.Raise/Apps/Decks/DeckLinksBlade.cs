namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksBlade(Guid deckId) : ViewBase
{
    public record DeckLinkDto(Guid Id, string? Reference, string? ContactName, string? InvestorName, int Views, string Secret);
    
    public override object? Build()
    {
        var args = UseService<AppArgs>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deckLinks = UseState<DeckLinkDto[]?>();
        var client = UseService<IClientProvider>();
        var (alertView, showAlert) = this.UseAlert();
        var (editView, showEdit) = this.UseTrigger((IState<bool> isOpen, Guid linkId) 
            => new DeckLinksEditDialog(isOpen, refreshToken, linkId));
        
        UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deckLinks.Set(
                await db.DeckLinks
                    .Include(dl => dl.Contact).ThenInclude(c => c.Investor)
                    .Include(dl => dl.DeckLinkViews)
                    .Where(dl => dl.DeckId == deckId && dl.DeletedAt == null)
                    .Select(dl => new DeckLinkDto(
                        dl.Id,
                        dl.Reference,
                        dl.Contact != null ? $"{dl.Contact.FirstName} {dl.Contact.LastName}" : null,
                        dl.Contact != null ? dl.Contact.Investor.Name : null,
                        dl.DeckLinkViews.Count(),
                        dl.Secret
                    ))
                    .ToArrayAsync()
            );
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this deck link?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Deck Link");
            };
        }
        
        Action OnCopy(string secret)
        {
            return () =>
            {
                var link = $"{args.Scheme}://{args.Host}/decks/link/{secret}";
                client.CopyToClipboard(link);
                client.Toast($"{link} copied to clipboard");
            };
        }

        if (deckLinks.Value == null) return null;

        var table = deckLinks.Value.Select(dl => new
                {
                    dl.Reference,
                    Contact =
                        dl.ContactName != null
                            ? (object)(Layout.Vertical().Gap(0)
                                       | Text.Inline($"{dl.ContactName}")
                                       | Text.Muted(dl.InvestorName))
                            : Text.Muted("<Anyone>"),
                    dl.Views,
                    _ = Layout.Horizontal().Gap(1)
                        | Icons.Ellipsis
                            .ToButton()
                            .Ghost()
                            .WithDropDown(
                                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(dl.Id)),
                                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(() => showEdit(dl.Id))
                            )
                        | Icons.Clipboard
                            .ToButton()
                            .Outline()
                            .Tooltip("Copy Link")
                            .HandleClick(OnCopy(dl.Secret))
                })
                .ToTable()
                //todo ivy: this isn't working
                // .Width(e => e.Reference, Size.Fraction(1/2f))
                // .Width(e => e.Contact, Size.Fraction(1/2f))
                // .Width(e => e.Views, Size.Shrink())
                .ColumnWidth(e => e._, Size.Shrink())
            ;

        var addBtn = new Button("Add Deck Link").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckLinksCreateDialog(isOpen, refreshToken, deckId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView
               | editView
            ;
    }

    private void Delete(DataContextFactory factory, Guid deckLinkId)
    {
        using var db = factory.CreateDbContext();
        var deckLink = db.DeckLinks.Single(dl => dl.Id == deckLinkId);
        deckLink.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}