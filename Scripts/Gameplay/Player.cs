using Godot;
using System;

public class Player : KinematicBody
{
    // config variables
    [Export]
    Vector3 cameraPosition;
    [Export]
    float maxMoveSpeed = 5.0f;
    [Export]
    float moveAccel = 0.25f;
    [Export]
    float moveDeccel = 0.25f;
    [Export]
    float jumpSpeed = 30.0f;
    [Export]
    float maxFallSpeed = 30.0f;
    [Export]
    float gravity = 0.75f;
    [Export]
    float hLookSens = 1.0f;
    [Export]
    float vLookSens = 1.0f;

    [Signal]
    delegate void rot_camera(Vector3 newCamRot);
    [Signal]
    delegate void move_camera(Vector3 newCamPos);
    // changing variables
    float currForwardSpeed = 0;
    float currBackwardSpeed = 0;
    float currLeftSpeed = 0;
    float currRightSpeed = 0;
    float yVelocity = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cameraPosition.y = 0.96f;
        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    // Called when there's an input to handle
    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_cancel")) {
            Input.SetMouseMode(Input.MouseMode.Visible);
        }
        if (@event is InputEventMouseButton inputEventMouseButton) {
            if (inputEventMouseButton.IsPressed()) {
                Input.SetMouseMode(Input.MouseMode.Captured);

                if (inputEventMouseButton.ButtonIndex == (int)ButtonList.Left) {
                    var s = Main.camera.Translation;
                    Bullet.Fire(s);
                }
            }
        }

        if (
            @event is InputEventMouseMotion inputEventMouseMotion &&
            Input.GetMouseMode() == Input.MouseMode.Captured) {
            // set character rotation
            var charRot = RotationDegrees;
            charRot.y -= (inputEventMouseMotion.Relative.x * hLookSens) * 0.08f;
            RotationDegrees = charRot;
            var newCamX = (inputEventMouseMotion.Relative.y * vLookSens) * 0.08f;
            var newCamY = charRot.y;
            var newCamZ = 0;

            EmitSignal("rot_camera",
                new Vector3(
                    newCamX,
                    newCamY,
                    newCamZ
                ));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.GetMouseMode() == Input.MouseMode.Captured) {
            ProcessMovement(delta);
        }

        var orig = Transform.origin;
        orig.y += cameraPosition.y;
        EmitSignal("move_camera", orig);
    }

    // Custom functions below

    public void ProcessMovement(float delta)
    {
        Vector3 movement = new Vector3();
        bool forwardKey = Input.IsActionPressed("kb_forward");
        bool backwardKey = Input.IsActionPressed("kb_backward");
        bool leftKey = Input.IsActionPressed("kb_left");
        bool rightKey = Input.IsActionPressed("kb_right");

        if (forwardKey) {
            currForwardSpeed += moveAccel;
        } else {
            currForwardSpeed -= moveDeccel;
        }
        if (backwardKey) {
            currBackwardSpeed += moveAccel;
        } else {
            currBackwardSpeed -= moveDeccel;
        }

        if (leftKey) {
            currLeftSpeed += moveAccel;
        } else {
            currLeftSpeed -= moveDeccel;
        }
        if (rightKey) {
            currRightSpeed += moveAccel;
        } else {
            currRightSpeed -= moveDeccel;
        }
        currForwardSpeed = Mathf.Clamp(currForwardSpeed, 0, maxMoveSpeed);
        currBackwardSpeed = Mathf.Clamp(currBackwardSpeed, 0, maxMoveSpeed);
        currLeftSpeed = Mathf.Clamp(currLeftSpeed, 0, maxMoveSpeed);
        currRightSpeed = Mathf.Clamp(currRightSpeed, 0, maxMoveSpeed);

        var currZSpeed = currBackwardSpeed - currForwardSpeed;
        var currXSpeed = currRightSpeed - currLeftSpeed;

        movement.z = Mathf.Clamp(Mathf.RoundToInt(currZSpeed), -1, 1);
        movement.x = Mathf.Clamp(Mathf.RoundToInt(currXSpeed), -1, 1);

        var vvv = new Vector3(0, 1, 0);
        movement = movement
            .Normalized()
            .Rotated(vvv, Rotation.y);

        var currSpeed = Mathf.Max(Mathf.Abs(currZSpeed), Mathf.Abs(currXSpeed));
        movement *= currSpeed;

        var grounded = IsOnFloor();
        // GD.Print(grounded);
        var justJumped = false; // TODO: use with animation
        yVelocity -= gravity;

        if (grounded && Input.IsActionJustPressed("kb_jump")) {
            justJumped = true;
            yVelocity = jumpSpeed;
        }
        if (grounded && yVelocity <= 0) {
            yVelocity = -gravity;
        }
        if (yVelocity < -maxFallSpeed) {
            yVelocity = -maxFallSpeed;
        }

        movement.y = yVelocity;
        MoveAndSlide(movement, vvv);

        // TODO: make real animations and play them
        if (justJumped) {
            PlayAnim("jump");
        } else if (grounded) {
            if (movement.x == 0 && movement.z == 0) {
                PlayAnim("idle");
            } else {
                PlayAnim("walk");
            }
        }
    }

    public void PlayAnim(string name)
    {
        // TODO: make animation work
        // pseudo code.
        // if (anim.current_animation == name) {
        //     return;
        // }

        // anim.play(name)
    }
}
