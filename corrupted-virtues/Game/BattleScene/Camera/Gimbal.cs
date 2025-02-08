using Godot;

public partial class Gimbal : Node3D
{
    public Camera3D Camera;
    public Node3D InnerGimbal;

    [Export] float maxZoom = 3.0f;
    [Export] float minZoom = 0.5f;
    [Export] float zoomSpeed = 0.08f;
    [Export] float speed = 0.3f;
    [Export] float dragSpeed = 0.005f;
    [Export] float acceleration = 0.08f;
    [Export] float mouseSensitivity = 0.005f;

    float zoom = 1.5f;
    Vector3 move;

    public override void _Ready()
    {
        InnerGimbal = GetNode<Node3D>("InnerGimbal");
        Camera = GetNode<Camera3D>("InnerGimbal/Camera3D");
    }
    public override void _Input(InputEvent @event)
    {
        HandleCameraMovement(@event);
    }

    private void HandleCameraMovement(InputEvent @event)
    {
        if (Input.IsActionPressed("Rotate_Cam") && @event is InputEventMouseMotion mouseMotion)
        {
            if (mouseMotion.Relative.X != 0)
            {
                RotateObjectLocal(Vector3.Up, -mouseMotion.Relative.X * mouseSensitivity);
            }

            if (mouseMotion.Relative.Y != 0)
            {
                float yRotation = Mathf.Clamp(-mouseMotion.Relative.Y, -30, 30);
                InnerGimbal.RotateObjectLocal(Vector3.Right, yRotation * mouseSensitivity);
            }
        }
        if (Input.IsActionPressed("Move_Cam") && @event is InputEventMouseMotion mouseDrag)
        {
            move.X -= mouseDrag.Relative.X * dragSpeed;
            move.Z -= mouseDrag.Relative.Y * dragSpeed;
        }

        if (@event.IsActionPressed("Zoom_In"))
        {
            zoom -= zoomSpeed;
        }

        if (@event.IsActionPressed("Zoom_Out"))
        {
            zoom += zoomSpeed;
        }

        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public static float Lerp(float First, float Second, float Amount)
    {
        return First * (1 - Amount) + Second * Amount;
    }
    public static Vector3 Lerp(Vector3 First, Vector3 Second, float Amount)
    {
        float retX = Lerp(First.X, Second.X, Amount);
        float retY = Lerp(First.Y, Second.Y, Amount);
        float retZ = Lerp(First.Z, Second.Z, Amount);
        return new Vector3(retX, retY, retZ);
    }
    public override void _Process(double delta)
    {
        Scale = Lerp(Scale, Vector3.One * zoom, zoomSpeed);
        InnerGimbal.Rotation = new Vector3(Mathf.Clamp(InnerGimbal.Rotation.X, -1.1f, 0.3f), InnerGimbal.Rotation.Y, InnerGimbal.Rotation.Z);
    }
}