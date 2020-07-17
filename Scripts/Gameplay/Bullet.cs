using Godot;

public class Bullet : Node
{
    ImmediateGeometry debugDrawNode;
    SpatialMaterial material;
    Vector3 start;
    Vector3 end;
    bool calcDone = false;
    int distance = 1000;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (start == null) {
            start = new Vector3();
        }
        if (end == null) {
            end = new Vector3();
        }

        debugDrawNode = Main.instance.GetNode("DebugDraw") as ImmediateGeometry;
        material = new SpatialMaterial();
        material.FlagsUnshaded = true;
        material.FlagsUsePointSize = true;
        material.AlbedoColor = Colors.Green;
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
        b.end = new Vector3(0, 0, 0);
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
            debugDrawNode.MaterialOverride = material;
            debugDrawNode.Begin(Mesh.PrimitiveType.LineStrip, null);
            debugDrawNode.AddVertex(start);
            debugDrawNode.AddVertex(end);
            debugDrawNode.End();
        }
    }

    public void CalcProjectile()
    {
        var camera = Main.camera;
        var screenCenter = GetViewport().Size / 2;
        end = start + camera.ProjectRayNormal(screenCenter);

        var spaceState = Main.instance.GetWorld().DirectSpaceState;
        // {
        //     position: Vector2 # point in world space for collision
        //     normal: Vector2 # normal in world space for collision
        //     collider: Object # Object collided or null (if unassociated)
        //     collider_id: ObjectID # Object it collided against
        //     rid: RID # RID it collided against
        //     shape: int # shape index of collider
        //     metadata: Variant() # metadata of collider
        // }
        var hit = spaceState.IntersectRay(start, end);
    }
}
