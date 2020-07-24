using Godot;
using Godot.Collections;
// using System.Collections.Generic;

public class Bullet : KinematicBody
{
    ImmediateGeometry debugDrawNode;
    ImmediateGeometry debugDrawNode2;
    Vector3 bulletStartPos;
    bool calcDone = false;
    float distance = 1000.0f;
    Dictionary stepPoints;
    int stepCount = 0;
    PhysicsDirectSpaceState spaceState;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (bulletStartPos == null) {
            bulletStartPos = new Vector3();
        }

        stepPoints = new Dictionary();

        debugDrawNode = Main.instance.GetNode("DebugDraw") as ImmediateGeometry;
        var material = new SpatialMaterial();
        material.FlagsUnshaded = true;
        material.FlagsUsePointSize = true;
        material.FlagsNoDepthTest = true;
        debugDrawNode.MaterialOverride = material;

        debugDrawNode2 = Main.instance.GetNode("DebugDrawPen") as ImmediateGeometry;
        var material2 = new SpatialMaterial();
        material2.FlagsUnshaded = true;
        material2.FlagsUsePointSize = true;
        material2.FlagsNoDepthTest = true;
        material2.AlbedoColor = Colors.Red;
        debugDrawNode2.MaterialOverride = material2;
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
        b.bulletStartPos = new Vector3(0, 0, 0);
        Main.instance.AddChild(b);
        return b;
    }

    public static Bullet Fire(Vector3 start)
    {
        var b = new Bullet();
        b.bulletStartPos = start;
        Main.instance.AddChild(b);
        return b;
    }

    public void AddToDebugDraw()
    {
        if (Main.debugDraw) {
            for (int i = 0; i < stepCount; i++)
            {
                Vector3 thisStart = (Vector3)((Dictionary)stepPoints[i])["start"];
                Vector3 thisEnd = (Vector3)((Dictionary)stepPoints[i])["end"];
                bool isPen = (bool)((Dictionary)stepPoints[i])["isPen"];
                // GD.Print("Drawing: ", thisStart, thisEnd);

                if (isPen) {
                    debugDrawNode2.Begin(Mesh.PrimitiveType.LineStrip, null);
                    debugDrawNode2.AddVertex(thisStart);
                    debugDrawNode2.AddVertex(thisEnd);
                    debugDrawNode2.End();
                } else {
                    debugDrawNode.Begin(Mesh.PrimitiveType.LineStrip, null);
                    debugDrawNode.AddVertex(thisStart);
                    debugDrawNode.AddVertex(thisEnd);
                    debugDrawNode.End();
                }
            }
        }
    }

    public void CalcProjectile()
    {
        spaceState = Main.instance.GetWorld().DirectSpaceState;
        var camera = Main.camera;
        var end = bulletStartPos + camera.ProjectRayNormal(Helpers.GetScreenCenter()) * distance;
        GD.Print("Initial distance: ", bulletStartPos.DistanceTo(end));
        CalcProjectileStepForward(bulletStartPos, end);
    }

    public void CalcProjectileStepForward(Vector3 start, Vector3 origEnd)
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
        var hitEnd = origEnd;

        if (hit.Contains("position")) {
            hitEnd = (Vector3)hit["position"];
            distance -= start.DistanceTo(hitEnd);
            var body = (PhysicsBody)hit["collider"];
            body.CollisionLayer = (uint)(Wallbang.CollisionMask.LAYER0 | Wallbang.CollisionMask.LAYER19);
            spaceState = Main.instance.GetWorld().DirectSpaceState;

            if (distance > 0) {
                CalcProjectileStepBackward(origEnd, hitEnd);
            }
        }

        var d = new Dictionary();
        d["start"] = start;
        d["end"] = hitEnd;
        d["isPen"] = false;
        AddStep(d);
        GD.Print("Bullet travel: ", start, hitEnd);
    }

    public void CalcProjectileStepBackward(Vector3 origEnd, Vector3 bulletEntry)
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
        var hit = spaceState.IntersectRay(origEnd, bulletEntry, null, (uint)Wallbang.CollisionMask.LAYER19);
        Vector3 bulletExit = bulletEntry;

        if (hit.Contains("position")) {
            bulletExit = (Vector3)hit["position"];
            distance -= bulletEntry.DistanceTo(bulletExit);
            var body = (PhysicsBody)hit["collider"];
            body.CollisionLayer = (uint)Wallbang.CollisionMask.LAYER0;

            var d = new Dictionary();
            d["start"] = bulletExit;
            d["end"] = bulletEntry;
            d["isPen"] = true;
            AddStep(d);
            GD.Print("Bullet penetration: ", bulletExit, bulletEntry);

            CalcProjectileStepForward(bulletExit, origEnd);
        }
    }

    public void AddStep(Dictionary d)
    {
        stepPoints[stepCount] = d;
        stepCount++;
    }
}
