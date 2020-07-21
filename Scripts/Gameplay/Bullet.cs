using Godot;
using System.Collections.Generic;

public class Bullet : Node
{
    ImmediateGeometry debugDrawNode;
    SpatialMaterial material;
    Vector3 start;
    bool calcDone = false;
    float distance = 1000.0f;
    List<List<Vector3>> stepPoints;
    PhysicsDirectSpaceState spaceState;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (start == null) {
            start = new Vector3();
        }

        stepPoints = new List<List<Vector3>>();

        debugDrawNode = Main.instance.GetNode("DebugDraw") as ImmediateGeometry;
        material = new SpatialMaterial();
        material.FlagsUnshaded = true;
        material.FlagsUsePointSize = true;
        material.FlagsNoDepthTest = true;
        debugDrawNode.MaterialOverride = material;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!calcDone) {
            GD.Print("Bang!");
            CalcProjectile();
            calcDone = true;
            AddToDebugDraw();
        }
    }

    public static Bullet Fire()
    {
        var b = new Bullet();
        b.start = new Vector3(0, 0, 0);
        Main.instance.AddChild(b);
        return b;
    }

    public static Bullet Fire(Vector3 start)
    {
        var b = new Bullet();
        b.start = start;
        Main.instance.AddChild(b);
        return b;
    }

    public void AddToDebugDraw()
    {
        if (Main.debugDraw) {
            int index = 0;
            // bool afterHit = false;

            foreach (List<Vector3> item in stepPoints)
            {
                var thisStart = item[0];
                var thisEnd = item[1];

                debugDrawNode.Begin(Mesh.PrimitiveType.LineStrip, null);
                // if (index % 2 == 0) {
                //     debugDrawNode.SetColor(Colors.Red);
                //     afterHit = true;
                // } else {
                //     if (afterHit) {
                //         debugDrawNode.SetColor(Colors.Orange);
                //     } else {
                //         debugDrawNode.SetColor(Colors.Green);
                //     }
                // }
                debugDrawNode.AddVertex(thisStart);
                debugDrawNode.AddVertex(thisEnd);
                debugDrawNode.End();
                index++;
            }
        }
    }

    public void CalcProjectile()
    {
        spaceState = Main.instance.GetWorld().DirectSpaceState;
        var camera = Main.camera;
        var end = start + camera.ProjectRayNormal(Helpers.GetScreenCenter()) * distance;
        calcProjectileStepForward(start, end);
    }

    public void calcProjectileStepForward(Vector3 start, Vector3 origEnd)
    {
        // {
        //     position: Vector3 # point in world space for collision
        //     normal: Vector3 # normal in world space for collision
        //     collider: Object # Object collided or null (if unassociated)
        //     collider_id: ObjectID # Object it collided against
        //     rid: RID # RID it collided against
        //     shape: int # shape index of collider
        //     metadata: Variant() # metadata of collider
        // }
        var hit = spaceState.IntersectRay(start, origEnd);

        var end = origEnd;
        if (hit.Contains("position")) {
            ProjectSettings.GetSetting("layer_names/3d_physics");
            var body = (PhysicsBody)hit["collider"];
            GD.Print(body.CollisionLayer);
            body.CollisionLayer = (uint)(CollisionMask.LAYER0 | CollisionMask.LAYER19);
            end = (Vector3)hit["position"];
            spaceState = Main.instance.GetWorld().DirectSpaceState;
        }

        // distance -= start.DistanceTo(end) * 500;
        // if (distance < 0) {
        //     distance = 0;
        // }

        // var l = new List<Vector3>();
        // l.Add(start);
        // l.Add(end);
        // stepPoints.Add(l);

        if (distance > 0) {
            calcProjectileStepBackward(origEnd, end);
        }
    }

    public void calcProjectileStepBackward(Vector3 start, Vector3 end)
    {
        // {
        //     position: Vector3 # point in world space for collision
        //     normal: Vector3 # normal in world space for collision
        //     collider: Object # Object collided or null (if unassociated)
        //     collider_id: ObjectID # Object it collided against
        //     rid: RID # RID it collided against
        //     shape: int # shape index of collider
        //     metadata: Variant() # metadata of collider
        // }
        var hit = spaceState.IntersectRay(start, end, null, (uint)CollisionMask.LAYER19);
        GD.Print(hit);
        if (hit.Contains("position")) {
            end = (Vector3)hit["position"];
            distance -= start.DistanceTo(end);
            var l = new List<Vector3>();
            l.Add(start);
            l.Add(end);
            stepPoints.Add(l);
        }
    }
}
