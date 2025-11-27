namespace Ivy.Open.Raise.Apps.Settings.Users;

public class UserCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record UserCreateRequest
    {
        [Required]
        public string Email { get; init; } = "";

        [Required]
        public string FirstName { get; init; } = "";

        [Required]
        public string LastName { get; init; } = "";

        public string? Title { get; init; }

        public string? CalendarUrl { get; init; }

        public string? ProfilePictureUrl { get; init; }

        public string? LinkedinUrl { get; init; }

        public string? XUrl { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var user = UseState(() => new UserCreateRequest());

        UseEffect(() =>
        {
            var userId = CreateUser(factory, user.Value);
            refreshToken.Refresh(userId);
        }, [user]);

        return user
            .ToForm()
            .Builder(e => e.Title, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "New User", submitTitle: "Create");
    }

    private Guid CreateUser(DataContextFactory factory, UserCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var user = new User()
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Title = request.Title,
            CalendarUrl = request.CalendarUrl,
            ProfilePictureUrl = request.ProfilePictureUrl,
            LinkedinUrl = request.LinkedinUrl,
            XUrl = request.XUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        db.SaveChanges();

        return user.Id;
    }
}
