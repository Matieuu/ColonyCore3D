using Silk.NET.Maths;

namespace ColonyCore;

public class Camera {

    public Vector3D<float> Position { get; private set; }
    public Vector3D<float> Target { get; private set; }

    public Matrix4X4<float> ViewMatrix { get; private set; }
    public Matrix4X4<float> ProjectionMatrix { get; private set; }

    private Vector3D<float> _up = Vector3D<float>.UnitY;

    private float _distance = 100f;
    private float _aspectRatio;
    private float _fov = MathF.PI / 3.25f;
    private float _yaw = 45f; // Obrót lewo / prawo
    private float _pitch = -35f; // Obrót góra / dół

    public Camera(float width, float height) {
        if (height == 0) height = 1;
        if (height < 0) height = MathF.Abs(height);

        Position = new Vector3D<float>(200, 50, 200);
        _aspectRatio = width / height;
        UpdateMatrices();
    }

    public void UpdateAspectRatio(float width, float height) {
        if (height == 0) height = 1;
        if (height < 0) height = MathF.Abs(height);

        _aspectRatio = width / height;
        UpdateMatrices();
    }

    public void Move(Vector3D<float> delta) {
        Position += delta;
        UpdateMatrices();
    }

    public void Zoom(float delta) {
        _distance = Math.Clamp(_distance - delta, 5f, 200f);
        UpdateMatrices();
    }

    public void Rotate(float deltaX, float deltaY) {
        _yaw += deltaX;
        _pitch += deltaY;
        UpdateMatrices();
    }

    public void Pan(float deltaX, float deltaZ) {
        float sensitivity = .002f * _distance;

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
        _pitch = Math.Clamp(_pitch, -89f, -1f);

        float yawRad = _yaw * (MathF.PI / 180f);
        float pitchRad = _pitch * (MathF.PI / 180f);

        float x = _distance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);
        float y = _distance * MathF.Sin(MathF.Abs(pitchRad));
        float z = _distance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);

        Position = Target + new Vector3D<float>(x, y, z);
        ViewMatrix = Matrix4X4.CreateLookAt(Position, Target, _up);

        ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, .1f, 2000f);
    }

    public Ray GetRayFromMouse(Vector2D<float> mousePos, Vector2D<float> windowSize) {
        // Zamiana pikseli na NDC (-1 do 1)
        float x = 2f * mousePos.X / windowSize.X - 1f;
        float y = 1f - 2f * mousePos.Y / windowSize.Y;

        Vector4D<float> clipCoords = new(x, y, -1f, 1f);

        // Odwracanie Projekcji (Ekran -> Przestrzeń Kamery)
        Matrix4X4.Invert(ProjectionMatrix, out var invProj);
        Vector4D<float> eyeCoords = Vector4D.Transform(clipCoords, invProj);
        eyeCoords = new Vector4D<float>(eyeCoords.X, eyeCoords.Y, -1f, 0f);

        // Odwracanie Widoku (Przestrzeń Kamery -> Świat)
        Matrix4X4.Invert(ViewMatrix, out var invView);
        Vector4D<float> rayWorld = Vector4D.Transform(eyeCoords, invView);

        Vector3D<float> direction = new Vector3D<float>(rayWorld.X, rayWorld.Y, rayWorld.Z);
        direction = Vector3D.Normalize(direction);

        return Ray.FromVectors(Position, direction);
    }

}
