using Silk.NET.OpenGL;

namespace ColonyCore;

public class VertexArrayObject<TType, TIndex> : IDisposable
    where TType : unmanaged
    where TIndex : unmanaged {

    private uint _handle;
    private GL _gl;

    public VertexArrayObject(GL gl, BufferObject<TType> vbo, BufferObject<TIndex> ebo = null!) {
        _gl = gl;
        _handle = _gl.GenVertexArray();
        Bind();

        // VAO musi wiedzieć, z jakiego VBO ma czytać przepis
        vbo.Bind();
        ebo?.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint stride, int offset) {
        // Tu jest ta cała matematyka sizeof, której nie chcesz widzieć w Main
        _gl.VertexAttribPointer(index, count, type, false, stride * (uint)sizeof(TType), (void*)(offset * sizeof(TType)));
        _gl.EnableVertexAttribArray(index);
    }

    public void Bind() {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose() {
        _gl.DeleteVertexArray(_handle);
    }

}