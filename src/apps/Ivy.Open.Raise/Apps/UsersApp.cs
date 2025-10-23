using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.User, path: ["Apps"])]
public class UsersApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new UserListBlade(), "Search");
    }
}