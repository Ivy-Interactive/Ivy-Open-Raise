using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Contact, path: ["Apps"])]
public class ContactsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ContactListBlade(), "Search");
    }
}