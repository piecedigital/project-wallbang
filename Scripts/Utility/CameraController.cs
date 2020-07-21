using Godot;
using System;

public class CameraController : Camera
{
    [Export]
    int gap = 2;
    [Export]
    int innerLineLength = 10;

    // public override void _Ready()
    // {
    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //  }

    public void ConnectSignals()
    {
        Main.localPlayer.Connect("rot_camera", this, "RotCamera");
        Main.localPlayer.Connect("move_camera", this, "MoveCamera");
        Main.localPlayer.Connect("draw_crosshair", this, "DrawCrosshair");
    }

    // set cam rotation
    public void RotCamera(Vector3 newCamRot)
    {
        var camRot = RotationDegrees;
        camRot.x -= newCamRot.x;
        camRot.x = Mathf.Clamp(camRot.x, -90, 90);
        camRot.y = newCamRot.y;
        camRot.z = newCamRot.z;
        RotationDegrees = camRot;
    }

    // set cam position
    public void MoveCamera(Vector3 newCamPos)
    {
        var newTrans = GlobalTransform;
        newTrans.origin = newCamPos;
        GlobalTransform = newTrans;
    }

    public void DrawCrosshair()
    {
        var pos = Helpers.GetScreenCenter();
        // Main.ui.DrawRect();
    }
}
