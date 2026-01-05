using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksBlade(Guid deckId) : ViewBase
{
    public record DeckLinkDto(Guid Id, string? Reference, string? ContactName, string? InvestorName, int Views, string Secret);

    public override object? Build()
    {
        var args = UseService<Ivy.Apps.AppContext>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = UseRefreshToken();
        var client = UseService<IClientProvider>();
        var (editView, showEdit) = UseTrigger((IState<bool> isOpen, Guid linkId)
            => new DeckLinksEditDialog(isOpen, refreshToken, linkId));

        var linksQuery = UseQuery(
            key: (nameof(DeckLinksBlade), deckId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DeckLinks
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
                    .ToArrayAsync(ct);
            },
            tags: [(typeof(DeckLink[]), deckId)]
        );

        if (linksQuery.Loading) return Text.Muted("Loading...");

        var deckLinks = linksQuery.Value ?? [];

        MenuItem CreateDeleteBtn(Guid id) => MenuItem.Default("Delete")
            .Icon(Icons.Trash)
            .HandleSelect(async () =>
            {
                await DeleteAsync(factory, id);
                linksQuery.Mutator.Revalidate();
            });

        void OnCopy(string secret)
        {
            var link = $"{args.Scheme}://{args.Host}/links/{secret}";
            client.CopyToClipboard(link);
            client.Toast($"{link} copied to clipboard");
        }

        var table = deckLinks.Select(dl => new
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
                                CreateDeleteBtn(dl.Id),
                                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(() => showEdit(dl.Id))
                            )
                        | Icons.Clipboard
                            .ToButton()
                            .Outline()
                            .Tooltip("Copy Link")
                            .HandleClick(() => OnCopy(dl.Secret))
                })
                .ToTable()
                .Width(Size.Units(120))
                .ColumnWidth(e => e.Reference, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Views, Size.Units(50))
                .Align(e => e.Views, Align.Right)
                .ColumnWidth(e => e._, Size.Fit())
            ;

        var addBtn = new Button("New Link").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckLinksCreateDialog(isOpen, refreshToken, deckId));

        return new Fragment()
               | new BladeHeader(addBtn)
               | table
               | editView
            ;
    }

    private async Task DeleteAsync(DataContextFactory factory, Guid deckLinkId)
    {
        await using var db = factory.CreateDbContext();
        var deckLink = await db.DeckLinks.SingleAsync(dl => dl.Id == deckLinkId);
        deckLink.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
