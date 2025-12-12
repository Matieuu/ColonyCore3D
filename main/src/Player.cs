using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Player {
    public IntPtr NamePtr;
    public byte Health;
    public double X;
    public double Y;
}

internal static class NativeLib {
    const string DllName = "libbrain.so";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr player_create(
        [MarshalAs(UnmanagedType.LPStr)] string name,
        double x,
        double y
    );

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void player_destroy(IntPtr ptr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void player_move(IntPtr playerPtr, double dx, double dy);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void player_damage(IntPtr playerPtr, byte amount);
}
