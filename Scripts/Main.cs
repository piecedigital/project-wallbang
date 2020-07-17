using Godot;
using Godot.Collections;
using System;

public class Main : Spatial
{
    public static Main instance;
    public static Player localPlayer = null;
    public static bool debugDraw = true;
    public static CameraController camera;

    PackedScene scene;

    [Export]
    int playerLimit = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        instance = this;
        scene = ResourceLoader.Load<PackedScene>("res://Scenes/Test.tscn");
        AddChild(scene.Instance());
        camera = GetNode("Camera") as CameraController;

        SpawnPlayers();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //
    //  }

    public Main MainInstance()
    {
        return instance;
    }

    public void SpawnPlayers()
    {
        var children = scene.Instance()
            .GetNode("PlayerSpawners")
            .GetChildren();

        for (int i = 0; i < playerLimit; i++)
        {
            var rng = new RandomNumberGenerator();
            int index = rng.RandiRange(0, children.Count-1);
            Spatial spawner = children[index] as Spatial;
            Player player = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn")
                .Instance() as Player;
            GetNode("PlayerInstances").AddChild(player);

            var newPos = spawner.Translation;
            newPos.y += player.GetNode<CollisionShape>("CollisionShape").Scale.y / 2;
            var newTrans = player.GlobalTransform;
            newTrans.origin = newPos;
            player.GlobalTransform = newTrans;

            if (Main.localPlayer == null) {
                Main.localPlayer = player;
                camera.ConnectSignals();
            }

            children.Remove(spawner);
        }
    }
}
