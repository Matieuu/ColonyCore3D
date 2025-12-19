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

    private IntPtr _simHandle = IntPtr.Zero;

    private BufferObject<float> _vbo = null!;
    private BufferObject<float> _instanceVbo = null!;
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

        _simHandle = NativeLib.Sim_Init(100, 20, 100);
        _gl.Enable(EnableCap.DepthTest);
        // _gl.Disable(EnableCap.CullFace);

        float[] cubeVertices = {
            // X, Y, Z,           R, G, B
            // Ściana Przednia
            -0.5f, -0.5f,  0.5f,  0.6f, 0.6f, 0.6f,
            0.5f, -0.5f,  0.5f,  0.6f, 0.6f, 0.6f,
            0.5f,  0.5f,  0.5f,  0.6f, 0.6f, 0.6f,
            0.5f,  0.5f,  0.5f,  0.6f, 0.6f, 0.6f,
            -0.5f,  0.5f,  0.5f,  0.6f, 0.6f, 0.6f,
            -0.5f, -0.5f,  0.5f,  0.6f, 0.6f, 0.6f,

            // Ściana Tylna
            -0.5f, -0.5f, -0.5f,  0.4f, 0.4f, 0.4f,
            0.5f, -0.5f, -0.5f,  0.4f, 0.4f, 0.4f,
            0.5f,  0.5f, -0.5f,  0.4f, 0.4f, 0.4f,
            0.5f,  0.5f, -0.5f,  0.4f, 0.4f, 0.4f,
            -0.5f,  0.5f, -0.5f,  0.4f, 0.4f, 0.4f,
            -0.5f, -0.5f, -0.5f,  0.4f, 0.4f, 0.4f,

            // Ściana Lewa
            -0.5f,  0.5f,  0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f,  0.5f, -0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f, -0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f, -0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f,  0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f,  0.5f,  0.5f,  0.5f, 0.5f, 0.5f,

            // Ściana Prawa
            0.5f,  0.5f,  0.5f,  0.7f, 0.7f, 0.7f,
            0.5f,  0.5f, -0.5f,  0.7f, 0.7f, 0.7f,
            0.5f, -0.5f, -0.5f,  0.7f, 0.7f, 0.7f,
            0.5f, -0.5f, -0.5f,  0.7f, 0.7f, 0.7f,
            0.5f, -0.5f,  0.5f,  0.7f, 0.7f, 0.7f,
            0.5f,  0.5f,  0.5f,  0.7f, 0.7f, 0.7f,

            // Ściana Dolna
            -0.5f, -0.5f, -0.5f,  0.3f, 0.3f, 0.3f,
            0.5f, -0.5f, -0.5f,  0.3f, 0.3f, 0.3f,
            0.5f, -0.5f,  0.5f,  0.3f, 0.3f, 0.3f,
            0.5f, -0.5f,  0.5f,  0.3f, 0.3f, 0.3f,
            -0.5f, -0.5f,  0.5f,  0.3f, 0.3f, 0.3f,
            -0.5f, -0.5f, -0.5f,  0.3f, 0.3f, 0.3f,

            // Ściana Górna
            -0.5f,  0.5f, -0.5f,  0.8f, 0.8f, 0.8f,
            0.5f,  0.5f, -0.5f,  0.8f, 0.8f, 0.8f,
            0.5f,  0.5f,  0.5f,  0.8f, 0.8f, 0.8f,
            0.5f,  0.5f,  0.5f,  0.8f, 0.8f, 0.8f,
            -0.5f,  0.5f,  0.5f,  0.8f, 0.8f, 0.8f,
            -0.5f,  0.5f, -0.5f,  0.8f, 0.8f, 0.8f
        };

        _vbo = new BufferObject<float>(_gl, cubeVertices, BufferTargetARB.ArrayBuffer);
        _instanceVbo = new BufferObject<float>(_gl, new float[30_000], BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw);

        _vao = new VertexArrayObject<float, uint>(_gl, _vbo);
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);

        _instanceVbo.Bind();
        unsafe {
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
            _gl.VertexAttribDivisor(2, 1);
        }
    }

    private void OnUpdate(double deltaTime) {
        NativeLib.Sim_Tick(_simHandle);

        _controller.Update((float)deltaTime);
    }

    private void OnRender(double deltaTime) {
        _gl.ClearColor(Color.CornflowerBlue);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        // var model = Matrix4X4.CreateRotationY(time * 2f) * Matrix4X4.CreateRotationX(time * .5f);
        var view = Matrix4X4.CreateLookAt(new Vector3D<float>(20, 5, 20), new Vector3D<float>(10, 1, 10), Vector3D<float>.UnitY);
        var projection = Matrix4X4.CreatePerspectiveFieldOfView(
            (float)(60f * Math.PI / 180f),
            (float)_window.Size.X / (float)_window.Size.Y,
            .1f,
            100f
        );

        // _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);

        IntPtr mapPtr = NativeLib.Sim_GetMapPtr(_simHandle);
        ulong mapLen = NativeLib.Sim_GetMapLen(_simHandle);
        uint width = NativeLib.World_GetWidth(_simHandle);
        uint height = NativeLib.World_GetHeight(_simHandle);
        uint depth = NativeLib.World_GetDepth(_simHandle);

        var instancePositions = new List<float>();

        unsafe {
            ushort* map = (ushort*)mapPtr;

            for (uint y = 0; y < height; y++)
                for (uint z = 0; z < depth; z++)
                    for (uint x = 0; x < width; x++) {
                        long idx = x + (y * width) + (z * width * height);
                        if (map[idx] != 0) {
                            instancePositions.Add(x);
                            instancePositions.Add(y);
                            instancePositions.Add(z);
                        }
                    }
        }

        _instanceVbo.Bind();
        unsafe {
            fixed (float* d = instancePositions.ToArray()) {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(instancePositions.Count * sizeof(float)), d);
            }
        }

        _vao.Bind();
        unsafe {
            _gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, (uint)(instancePositions.Count / 3));
        }

        _controller.Render();
    }

    private void OnClose() {
        _instanceVbo.Dispose();
        _vbo.Dispose();
        _vao.Dispose();

        NativeLib.Sim_Destroy(_simHandle);
        _shader.Dispose();
        _controller?.Dispose();
        _gl?.Dispose();
    }
}
