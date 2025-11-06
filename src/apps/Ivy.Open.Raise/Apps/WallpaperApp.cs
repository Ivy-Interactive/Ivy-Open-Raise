namespace Ivy.Open.Raise.Apps;

[App(isVisible:false)]
public class WallpaperApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Center() | "Welcome to Ivy Open Raise!";
    }
}