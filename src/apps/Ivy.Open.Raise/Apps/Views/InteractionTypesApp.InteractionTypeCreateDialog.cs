namespace Ivy.Open.Raise.Apps.Views;

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
        var interactionType = UseState(() => new InteractionTypeCreateRequest());

        UseEffect(() =>
        {
            var interactionTypeId = CreateInteractionType(factory, interactionType.Value);
            refreshToken.Refresh(interactionTypeId);
        }, [interactionType]);

        return interactionType
            .ToForm()
            .ToDialog(isOpen, title: "Create Interaction Type", submitTitle: "Create");
    }

    private int CreateInteractionType(DataContextFactory factory, InteractionTypeCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var interactionType = new InteractionType
        {
            Name = request.Name
        };

        db.InteractionTypes.Add(interactionType);
        db.SaveChanges();

        return interactionType.Id;
    }
}