namespace Ivy.Open.Raise.Apps.Settings.InteractionTypes;

public class InteractionTypeCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InteractionTypeCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new InteractionTypeCreateRequest());

        return details
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Interaction Type", submitTitle: "Create");

        async Task OnSubmit(InteractionTypeCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var interactionType = new InteractionType
            {
                Name = request.Name
            };

            db.InteractionTypes.Add(interactionType);
            await db.SaveChangesAsync();
            refreshToken.Refresh(interactionType.Id);
        }
    }
}
