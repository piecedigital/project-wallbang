using Godot;
using Godot.Collections;
// using System.Collections.Generic;

public class Bullet : KinematicBody
{
    ImmediateGeometry debugDrawNode;
    ImmediateGeometry debugDrawNode2;
    Vector3 bulletStartPos;
    Vector3 bulletEndPos;
    Vector3 bulletPos;
    bool calcDone = false;
    float travelDistanceLimit = 1000.0f;
    Dictionary stepPoints;
    int stepCount = 0;
    PhysicsDirectSpaceState spaceState;
    PackedScene ball;
    SpatialMaterial material;
    SpatialMaterial material2;
    Array<PhysicsBody> collisionList;
    int penLimit = 10;
    int pens = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (bulletStartPos == null) {
            bulletStartPos = new Vector3();
        }

        if (bulletEndPos == null) {
            bulletEndPos = new Vector3();
        }

        bulletPos = bulletStartPos;

        stepPoints = new Dictionary();
        collisionList = new Array<PhysicsBody>();

        debugDrawNode = Main.instance.GetNode("DebugDraw") as ImmediateGeometry;
        material = new SpatialMaterial();
        material.FlagsUnshaded = true;
        material.FlagsUsePointSize = true;
        material.FlagsNoDepthTest = true;
        debugDrawNode.MaterialOverride = material;

        debugDrawNode2 = Main.instance.GetNode("DebugDrawPen") as ImmediateGeometry;
        material2 = new SpatialMaterial();
        material2.FlagsUnshaded = true;
        material2.FlagsUsePointSize = true;
        material2.FlagsNoDepthTest = true;
        material2.AlbedoColor = Colors.Red;
        debugDrawNode2.MaterialOverride = material2;

        ball = ResourceLoader.Load<PackedScene>("res://Scenes/Ball.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!calcDone) {
            CalcProjectile();
            calcDone = true;
            RestorePrimaryLayer();
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
                GD.Print("Points: ", thisStart, thisEnd);
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
        bulletEndPos = bulletStartPos + camera.ProjectRayNormal(Helpers.GetScreenCenter()) * travelDistanceLimit;

        CalcProjectileStepForward();
    }

    public void CalcProjectileStepForward()
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
        var hit = spaceState.IntersectRay(bulletStartPos, bulletEndPos, null, (uint)Wallbang.CollisionMask.LAYER0, true, true);
        var hitPosition = bulletEndPos;
        // GD.Print(hit);

        if (hit.Contains("position")) {
            hitPosition = (Vector3)hit["position"];
            // hit position is ahead of bullet
            if (hitPosition.DistanceTo(bulletEndPos) < bulletPos.DistanceTo(bulletEndPos)) {
                AddBall(hitPosition);
                AddStep(bulletPos, hitPosition, false);
                bulletPos = hitPosition;
            }
            var distanceTraveled = bulletStartPos.DistanceTo(bulletPos);

            var body = (PhysicsBody)hit["collider"];
            body.CollisionLayer = (uint)(Wallbang.CollisionMask.LAYER19);
            collisionList.Add(body);

            if (distanceTraveled <= travelDistanceLimit) {
                CalcProjectileStepBackward();
            }
        } else {
            AddStep(bulletPos, hitPosition, false);
        }
    }

    public void CalcProjectileStepBackward()
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
        var hit = spaceState.IntersectRay(bulletEndPos, bulletPos, null, (uint)Wallbang.CollisionMask.LAYER19);
        Vector3 bulletExit = bulletPos;
        GD.Print(hit);

        if (hit.Contains("position")) {
            bulletExit = (Vector3)hit["position"];
            AddBall(bulletExit, true);
            // var distanceTraveled = bulletStartPos.DistanceTo(bulletPos);
            // var penetrationDistance = bulletExit.DistanceTo(bulletPos);
            var body = (PhysicsBody)hit["collider"];

            AddStep(bulletExit, bulletPos, true);

            bulletPos = bulletExit;
        }

        // if (pens < penLimit) {
            CalcProjectileStepForward();
        //     pens++;
        // }
    }

    public void AddStep(Vector3 start, Vector3 end, bool isPen)
    {
        var d = new Dictionary();
        d["start"] = start;
        d["end"] = end;
        d["isPen"] = isPen;
        AddStep(d);
    }

    public void AddStep(Dictionary d)
    {
        stepPoints[stepCount] = d;
        stepCount++;
    }

    public void AddBall(Vector3 pos, bool isPen = false)
    {
        Spatial ballStart = (Spatial)ball.Instance();
        ballStart.GetNode<CSGMesh>("CSGMesh").MaterialOverride = isPen ? material2 : material;
        var newTrans = ballStart.Transform;
        newTrans.origin = pos;
        ballStart.Transform = newTrans;
        Main.instance.AddChild(ballStart);
    }

    public void RestorePrimaryLayer()
    {
        foreach (PhysicsBody body in collisionList) {
            body.CollisionLayer = (uint)Wallbang.CollisionMask.LAYER0;
        }
    }
}
