using Silk.NET.OpenGL;

namespace ColonyCore;

public class World {

    private readonly float[] CUBE_VERTICES = {
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

    private GL _gl;
    private IntPtr _simHandle;

    private BufferObject<float> _vbo;
    private BufferObject<float> _instanceVbo;
    private VertexArrayObject<float, uint> _vao;

    public World(GL gl, IntPtr simHandle) {
        _gl = gl;
        _simHandle = simHandle;

        _vbo = new BufferObject<float>(_gl, CUBE_VERTICES, BufferTargetARB.ArrayBuffer);
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

    public void Render() {
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
    }

    public void Dispose() {
        _instanceVbo.Dispose();
        _vbo.Dispose();
        _vao.Dispose();
    }

}
