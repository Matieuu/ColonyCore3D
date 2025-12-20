using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace ColonyCore;

public class SelectionRenderer : IDisposable {

    private GL _gl;
    private uint _shaderProgram;
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;

    private const string VertSrc = @"
        #version 330 core
        layout (location = 0) in vec3 aPos;
        
        uniform mat4 uMVP; // Model-View-Projection w jednym

        void main() {
            gl_Position = uMVP * vec4(aPos, 1.0);
        }";

    private const string FragSrc = @"
        #version 330 core
        out vec4 FragColor;
        void main() {
            FragColor = vec4(0.0, 0.0, 0.0, 1.0); // Czarny kolor
        }";

    public SelectionRenderer(GL gl) {
        _gl = gl;

        uint v = CompileShader(ShaderType.VertexShader, VertSrc);
        uint f = CompileShader(ShaderType.FragmentShader, FragSrc);
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, v);
        _gl.AttachShader(_shaderProgram, f);
        _gl.LinkProgram(_shaderProgram);
        _gl.DeleteShader(v);
        _gl.DeleteShader(f);

        _vbo = new BufferObject<float>(_gl, Constants.WireframeVertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, Constants.WireframeIndices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0);
    }

    private uint CompileShader(ShaderType type, string src) {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        return handle;
    }

    public void Render(Camera camera, Vector3D<int> blockPos) {
        _gl.UseProgram(_shaderProgram);
        _vao.Bind();

        var model = Matrix4X4.CreateTranslation((float)blockPos.X, (float)blockPos.Y, (float)blockPos.Z);
        var mvp = model * camera.ViewMatrix * camera.ProjectionMatrix;

        int loc = _gl.GetUniformLocation(_shaderProgram, "uMVP");
        unsafe {
            _gl.UniformMatrix4(loc, 1, false, (float*)&mvp);
        }

        unsafe {
            _gl.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedInt, null);
        }
    }

    public void Dispose() {
        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
        _gl.DeleteProgram(_shaderProgram);
    }

}