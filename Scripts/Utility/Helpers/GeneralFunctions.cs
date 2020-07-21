using Godot;

class Helpers
{
    public static Vector2 GetScreenCenter()
    {
        return Main.instance.GetViewport().Size / 2;
    }
}
