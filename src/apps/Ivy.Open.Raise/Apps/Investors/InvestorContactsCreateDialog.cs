namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorContactsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
{
    private record ContactCreateRequest
    {
        [Required]
        public string Email { get; init; } = "";

        [Required]
        public string FirstName { get; init; } = "";

        [Required]
        public string LastName { get; init; } = "";

        public string? Title { get; init; }

        public string? LinkedinUrl { get; init; }

        public string? XUrl { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new ContactCreateRequest());

        return details
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Contact", submitTitle: "Create");

        async Task OnSubmit(ContactCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Title = request.Title,
                LinkedinUrl = request.LinkedinUrl,
                XUrl = request.XUrl,
                InvestorId = investorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
            refreshToken.Refresh(contact.Id);
        }
    }
}