using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DeckCreateRequest
    {
        [Required]
        public string Title { get; } = "Deck";

        [Required]
        public FileUpload<BlobInfo>? File { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckState = UseState(() => new DeckCreateRequest());

        return deckState
            .ToForm()
            .Builder(e => e.File, FileUploadBuilder)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Deck", submitTitle: "Create");
        
        async Task OnSubmit(DeckCreateRequest details)
        {
            var deckId = await CreateDeckAsync(factory, details.Title, details.File!);
            refreshToken.Refresh(deckId);
        }
    }
}