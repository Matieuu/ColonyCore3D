using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ColonyCore;

class ColonyCore {

    private readonly IWindow _window;
    private IKeyboard _keyboard = null!;
    private IMouse _mouse = null!;

    private GL _gl = null!;
    private IInputContext _input = null!;
    private ImGuiController _controller = null!;
    private Shader _shader = null!;

    private IntPtr _simHandle = IntPtr.Zero;
    private Camera _camera = null!;
    private World _world = null!;
    private SelectionRenderer _selectionRenderer = null!;

    private Vector2D<float> _lastMousePos = Vector2D<float>.Zero;
    private Vector3D<int> _blockSelected = Vector3D<int>.Zero;

    public ColonyCore() {
        _window = Window.Create(WindowOptions.Default with {
            Title = "Colony Core 3D",
            Size = new Vector2D<int>(1600, 900),
            VSync = true
        });

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;

        _window.Run();
    }

    private void OnLoad() {
        _gl = _window.CreateOpenGL();
        _input = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _input);
        _shader = new Shader(_gl, "shader.vert", "shader.frag");

        _keyboard = _input.Keyboards[0];
        _mouse = _input.Mice[0];

        _mouse.Scroll += OnScroll;
        _mouse.MouseDown += OnMouseDown;
        _mouse.MouseMove += OnMouseMove;

        _simHandle = NativeLib.Sim_Init(100, 20, 100);
        _camera = new Camera(_window.Size.X, _window.Size.Y);
        _world = new World(_gl, _simHandle);
        _selectionRenderer = new SelectionRenderer(_gl);

        _gl.Enable(EnableCap.DepthTest);
        // _gl.Disable(EnableCap.CullFace);
    }

    private void OnUpdate(double deltaTime) {
        NativeLib.Sim_Tick(_simHandle);

        float keySpeed = 150f * (float)deltaTime;
        if (_keyboard.IsKeyPressed(Key.W)) _camera.Pan(0, -keySpeed);
        if (_keyboard.IsKeyPressed(Key.S)) _camera.Pan(0, keySpeed);
        if (_keyboard.IsKeyPressed(Key.A)) _camera.Pan(-keySpeed, 0);
        if (_keyboard.IsKeyPressed(Key.D)) _camera.Pan(keySpeed, 0);

        float rotSpeed = 90f * (float)deltaTime;
        if (_keyboard.IsKeyPressed(Key.Q)) _camera.Rotate(rotSpeed, 0);
        if (_keyboard.IsKeyPressed(Key.E)) _camera.Rotate(-rotSpeed, 0);

        _controller.Update((float)deltaTime);
    }

    private void OnRender(double deltaTime) {
        _gl.ClearColor(Color.CornflowerBlue);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _shader.SetUniform("uView", _camera.ViewMatrix);
        _shader.SetUniform("uProjection", _camera.ProjectionMatrix);

        _world.Render();

        if (_blockSelected != Vector3D<int>.Zero) {
            _selectionRenderer.Render(_camera, _blockSelected);
        }

        _controller.Render();
    }

    private void OnClose() {
        _selectionRenderer.Dispose();
        _world.Dispose();
        NativeLib.Sim_Destroy(_simHandle);
        _shader.Dispose();
        _controller.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }

    private void OnScroll(IMouse mouse, ScrollWheel wheel) {
        _camera.Zoom(wheel.Y * 5f);
    }

    private void OnMouseDown(IMouse mouse, MouseButton button) {
        if (button == MouseButton.Left) {
            var ray = _camera.GetRayFromMouse(
                new(mouse.Position.X, mouse.Position.Y),
                new(_window.Size.X, _window.Size.Y)
            );
            var raycastResult = NativeLib.Sim_Raycast(_simHandle, ray);

            if (raycastResult.Hit == 1) {
                _blockSelected = new(raycastResult.X, raycastResult.Y, raycastResult.Z);
            } else {
                _blockSelected = Vector3D<int>.Zero;
            }
        }
    }

    private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 pos) {
        Vector2D<float> mousePos = new(pos.X, pos.Y);
        var delta = _lastMousePos - mousePos;

        if (mouse.IsButtonPressed(MouseButton.Right)) {
            _camera.Pan(delta.X, delta.Y);
        }

        if (mouse.IsButtonPressed(MouseButton.Middle)) {
            _camera.Rotate(delta.X, delta.Y);
        }

        _lastMousePos = mousePos;
    }

}
