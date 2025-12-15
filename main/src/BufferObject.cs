using Silk.NET.OpenGL;

namespace ColonyCore;

// T musi być "unmanaged" (typem prostym: float, int, struct), żeby wskaźniki działały
public class BufferObject<T> : IDisposable where T : unmanaged {

    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;

    public unsafe BufferObject(
        GL gl,
        Span<T> data,
        BufferTargetARB bufferType,
        BufferUsageARB bufferUsage = BufferUsageARB.StaticDraw
    ) {
        _gl = gl;
        _bufferType = bufferType;

        _handle = _gl.GenBuffer();
        Bind();

        // Magia wskaźników zamknięta w środku
        fixed (void* d = data) {
            _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(T)), d, bufferUsage);
        }
    }

    public void Bind() {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose() {
        _gl.DeleteBuffer(_handle);
    }

}