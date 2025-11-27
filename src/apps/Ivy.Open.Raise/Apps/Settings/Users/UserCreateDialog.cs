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
        var details = UseState(() => new UserCreateRequest());

        return details
            .ToForm()
            .Builder(e => e.Title, e => e.ToTextAreaInput())
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New User", submitTitle: "Create");

        async Task OnSubmit(UserCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var user = new User
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
            await db.SaveChangesAsync();
            refreshToken.Refresh(user.Id);
        }
    }
}
