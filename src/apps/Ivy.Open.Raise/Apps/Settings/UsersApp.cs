using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.User, path: ["Apps", "Settings"])]
public class UsersApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new UserListBlade(), "Search");
    }
}