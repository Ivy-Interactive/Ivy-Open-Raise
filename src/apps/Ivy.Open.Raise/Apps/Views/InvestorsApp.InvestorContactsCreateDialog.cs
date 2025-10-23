namespace Ivy.Open.Raise.Apps.Views;

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
        var contact = UseState(() => new ContactCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var contactId = CreateContact(factory, contact.Value);
                refreshToken.Refresh(contactId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [contact]);

        return contact
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .ToDialog(isOpen, title: "Create Contact", submitTitle: "Create");
    }

    private Guid CreateContact(DataContextFactory factory, ContactCreateRequest request)
    {
        using var db = factory.CreateDbContext();

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
        db.SaveChanges();

        return contact.Id;
    }
}