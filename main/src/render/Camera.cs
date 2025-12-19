using Silk.NET.Maths;

namespace ColonyCore;

public class Camera {

    public Vector3D<float> Position { get; private set; }
    public Vector3D<float> Target { get; private set; }

    public Matrix4X4<float> ViewMatrix { get; private set; }
    public Matrix4X4<float> ProjectionMatrix { get; private set; }

    private Vector3D<float> _up = Vector3D<float>.UnitY;

    private float _zoom = 50f;
    private float _aspectRatio;
    private float _yaw = 45f;
    private float _pitch = -35f;

    public Camera(float width, float height) {
        Position = new Vector3D<float>(100, 50, 100);
        _aspectRatio = width / height;
        UpdateMatrices();
    }

    public void UpdateAspectRatio(float width, float height) {
        _aspectRatio = width / height;
        UpdateMatrices();
    }

    public void Move(Vector3D<float> delta) {
        Position += delta;
        UpdateMatrices();
    }

    public void Zoom(float delta) {
        _zoom = Math.Clamp(_zoom - delta, 5f, 100f);
        UpdateMatrices();
    }

    public void Rotate(float deltaX, float deltaY) {
        _yaw += deltaX;
        _pitch += deltaY;
        UpdateMatrices();
    }

    public void Pan(float deltaX, float deltaZ) {
        float sensitivity = .002f * _zoom;

        var forward = Vector3D.Normalize(Target - Position);
        forward.Y = 0;
        forward = Vector3D.Normalize(forward);

        var right = Vector3D.Normalize(Vector3D.Cross(forward, _up));

        var moveVector = (right * deltaX * sensitivity) + (forward * -deltaZ * sensitivity);

        Target += moveVector;
        UpdateMatrices();
    }

    private void UpdateMatrices() {
        if (_yaw > 360f) _yaw -= 360f;
        else if (_yaw < 0f) _yaw += 360f;
        _pitch = Math.Clamp(_pitch, -90f, 0f);

        float yawRad = _yaw * (MathF.PI / 180f);
        float pitchRad = _pitch * (MathF.PI / 180f);
        float distance = 200f;

        float x = distance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);
        float y = distance * MathF.Sin(MathF.Abs(pitchRad));
        float z = distance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);

        Position = Target + new Vector3D<float>(x, y, z);
        ViewMatrix = Matrix4X4.CreateLookAt(Position, Target, _up);

        float orthoWidth = _zoom * _aspectRatio;
        float orthoHeight = _zoom;

        ProjectionMatrix = Matrix4X4.CreateOrthographic(orthoWidth, orthoHeight, .1f, 2000f);
    }

}
