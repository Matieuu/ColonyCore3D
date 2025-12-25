using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace ColonyCore;

public static partial class NativeLib {

    private const string LibName = "brain";

    static NativeLib() {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
        if (libraryName == LibName) {
            if (OperatingSystem.IsWindows()) {
                return NativeLibrary.Load(LibName + ".dll", assembly, searchPath);
            } else if (OperatingSystem.IsLinux()) {
                return NativeLibrary.Load("lib" + LibName + ".so", assembly, searchPath);
            } else if (OperatingSystem.IsMacOS()) {
                return NativeLibrary.Load("lib" + LibName + ".dylib", assembly, searchPath);
            }
        }

        return IntPtr.Zero;
    }

    [LibraryImport(LibName, EntryPoint = "sim_init")]
    public static partial IntPtr Sim_Init(uint x, uint y, uint z);

    [LibraryImport(LibName, EntryPoint = "sim_destroy")]
    public static partial void Sim_Destroy(IntPtr simHandle);

    [LibraryImport(LibName, EntryPoint = "sim_tick")]
    public static partial void Sim_Tick(IntPtr simHandle, float dt, Vector2D<float> input, byte jump);



    [LibraryImport(LibName, EntryPoint = "sim_raycast")]
    public static partial RaycastResult Sim_Raycast(IntPtr simHandle, Ray ray, float distance);

    [LibraryImport(LibName, EntryPoint = "sim_get_player_pos")]
    public static partial Vector3D<float> Sim_GetPlayerPos(IntPtr simHandle);



    [LibraryImport(LibName, EntryPoint = "entity_get_float")]
    public static partial byte Entity_TryGetFloat(IntPtr simHandle, uint x, uint y, uint z, ushort propId, out float out_value);

    [LibraryImport(LibName, EntryPoint = "entity_get_int")]
    public static partial byte Entity_TryGetInt(IntPtr simHandle, uint x, uint y, uint z, ushort propId, out int out_value);



    [LibraryImport(LibName, EntryPoint = "world_get_map_ptr")]
    public static partial IntPtr World_GetMapPtr(IntPtr simHandle);

    [LibraryImport(LibName, EntryPoint = "world_get_map_len")]
    public static partial ulong World_GetMapLen(IntPtr simHandle);



    [LibraryImport(LibName, EntryPoint = "world_get_width")]
    public static partial uint World_GetWidth(IntPtr simHandle);

    [LibraryImport(LibName, EntryPoint = "world_get_height")]
    public static partial uint World_GetHeight(IntPtr simHandle);

    [LibraryImport(LibName, EntryPoint = "world_get_depth")]
    public static partial uint World_GetDepth(IntPtr simHandle);

}
