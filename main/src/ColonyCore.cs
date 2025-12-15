using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ColonyCore;

class ColonyCore {

    private readonly IWindow _window;

    private GL _gl = null!;
    private IInputContext _input = null!;
    private ImGuiController _controller = null!;
    private Shader _shader = null!;

    // private IntPtr _gameState = IntPtr.Zero;

    private BufferObject<float> _vbo = null!;
    private BufferObject<uint> _ebo = null!;
    private VertexArrayObject<float, uint> _vao = null!;

    public ColonyCore() {
        var options = WindowOptions.Default;
        options.Title = "Colony Core 3D";

        options.Size = new Vector2D<int>(1600, 900);
        options.VSync = true;

        _window = Window.Create(options);

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

        // _gameState = NativeLib.InitGame();
        _gl.Enable(EnableCap.DepthTest);
        // _gl.Disable(EnableCap.CullFace);

        // 1. Wierzchołki: Każdy punkt definiujemy TYLKO RAZ
        // X, Y, Z,    R, G, B
        float[] vertices = {
            0.0f,  0.5f,  0.0f,   1.0f, 0.0f, 0.0f, // 0: Czubek (Czerwony)
           -0.5f, -0.5f,  0.5f,   0.0f, 1.0f, 0.0f, // 1: Lewy Przód (Zielony)
            0.5f, -0.5f,  0.5f,   0.0f, 0.0f, 1.0f, // 2: Prawy Przód (Niebieski)
            0.0f, -0.5f, -0.5f,   1.0f, 1.0f, 0.0f  // 3: Tył (Żółty)
        };

        // 2. Indeksy: Tylko liczby całkowite (uint)
        // Mówimy: "Zrób trójkąt z wierzchołka 0, 1 i 2"
        uint[] indices = {
            0, 1, 2, // Przód
            0, 2, 3, // Prawa
            0, 3, 1, // Lewa
            1, 2, 3  // Podstawa
        };

        _vbo = new BufferObject<float>(_gl, vertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, indices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);
    }

    private void OnUpdate(double deltaTime) {
        // NativeLib.AddTicks(_gameState, 1);

        _controller.Update((float)deltaTime);
    }

    private void OnRender(double deltaTime) {
        _gl.ClearColor(Color.CornflowerBlue);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        var model = Matrix4X4.CreateRotationY<float>(time * 2f) * Matrix4X4.CreateRotationX<float>(time * .5f);
        var view = Matrix4X4.CreateLookAt<float>(new Vector3D<float>(0, 1, 3), Vector3D<float>.Zero, Vector3D<float>.UnitY);
        var projection = Matrix4X4.CreatePerspectiveFieldOfView<float>(
            (float)(60f * Math.PI / 180f),
            (float)_window.Size.X / (float)_window.Size.Y,
            .1f,
            100f
        );

        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);

        _vao.Bind();
        unsafe {
            _gl.DrawElements(PrimitiveType.Triangles, 12, DrawElementsType.UnsignedInt, null);
        }

        _controller.Render();
    }

    private void OnClose() {
        _vbo.Dispose();
        _vao.Dispose();

        // NativeLib.DestroyGame(_gameState);
        _shader.Dispose();
        _controller?.Dispose();
        _gl?.Dispose();
    }
}
