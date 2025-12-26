using Silk.NET.Maths;

namespace ColonyCore;

public class Camera {

    public Vector2D<float> View { get; private set; }

    public Matrix4X4<float> ViewMatrix { get; private set; }
    public Matrix4X4<float> ProjectionMatrix { get; private set; }

    private Vector3D<float> _up = Vector3D<float>.UnitY;

    private float _distance = 100f;
    private float _aspectRatio;
    private float _fov = MathF.PI / 3.25f;
    // private float _yaw = 45f; // Obrót lewo / prawo
    // private float _pitch = -35f; // Obrót góra / dół

    public Camera(float width, float height) {
        if (height == 0) height = 1;
        if (height < 0) height = MathF.Abs(height);

        View = new Vector2D<float>(45f, 35f);
        _aspectRatio = width / height;
        // UpdateMatrices();
    }

    public void UpdateAspectRatio(float width, float height) {
        if (height == 0) height = 1;
        if (height < 0) height = MathF.Abs(height);

        _aspectRatio = width / height;
        // UpdateMatrices();
    }

    public void Zoom(float delta) {
        _distance = Math.Clamp(_distance - delta, 5f, 200f);
        // UpdateMatrices();
    }

    public void Rotate(float deltaX, float deltaY) {
        View += new Vector2D<float>(deltaX, deltaY);
        // _yaw += deltaX;
        // _pitch += deltaY;
        // UpdateMatrices();
    }

    public void UpdateMatrices(Vector3D<float> playerPosition) {
        if (View.X > 360f) View -= new Vector2D<float>(360, 0);
        else if (View.X < 0f) View += new Vector2D<float>(360, 0);

        if (View.Y > 89f) View = new Vector2D<float>(View.X, 89);
        else if (View.Y < 1f) View = new Vector2D<float>(View.X, 1);
        // _pitch = Math.Clamp(_pitch, -89f, -1f);

        // float yawRad = _yaw * (MathF.PI / 180f);
        // float pitchRad = _pitch * (MathF.PI / 180f);
        Vector2D<float> viewRad = View * MathF.PI / 180f;

        float x = _distance * MathF.Cos(viewRad.Y) * MathF.Sin(viewRad.X);
        float y = _distance * MathF.Sin(MathF.Abs(viewRad.Y));
        float z = _distance * MathF.Cos(viewRad.Y) * MathF.Cos(viewRad.X);

        Vector3D<float> position = playerPosition + new Vector3D<float>(x, y, z);
        ViewMatrix = Matrix4X4.CreateLookAt(position, playerPosition, _up);

        ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, .1f, 2000f);
    }

    public Ray GetRayFromMouse(Vector2D<float> mousePos, Vector2D<float> windowSize, Vector3D<float> playerPos) {
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

        Vector2D<float> viewRad = View * MathF.PI / 180f;
        Vector3D<float> position = new Vector3D<float>(
            _distance * MathF.Cos(viewRad.Y) * MathF.Sin(viewRad.X),
            _distance * MathF.Sin(MathF.Abs(viewRad.Y)),
            _distance * MathF.Cos(viewRad.Y) * MathF.Cos(viewRad.X)
        );

        return Ray.FromVectors(playerPos + position, direction);
    }

    public (Vector2D<float> Forward, Vector2D<float> Right) GetFlatDirectionVectors() {
        float yawRad = View.X * MathF.PI / 180f;

        Vector2D<float> forward = new Vector2D<float>(-MathF.Sin(yawRad), -MathF.Cos(yawRad));
        Vector2D<float> right = new Vector2D<float>(-forward.Y, forward.X);

        return (forward, right);
    }

}
