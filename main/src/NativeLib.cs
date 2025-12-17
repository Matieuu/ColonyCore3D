using System.Reflection;
using System.Runtime.InteropServices;

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
            }
            else if (OperatingSystem.IsLinux()) {
                return NativeLibrary.Load("lib" + LibName + ".so", assembly, searchPath);
            }
            else if (OperatingSystem.IsMacOS()) {
                return NativeLibrary.Load("lib" + LibName + ".dylib", assembly, searchPath);
            }
        }

        return IntPtr.Zero;
    }

    [LibraryImport(LibName, EntryPoint = "sim_init")]
    public static partial IntPtr Sim_Init(uint x, uint y, uint z);

    [LibraryImport(LibName, EntryPoint = "sim_destroy")]
    public static partial void Sim_Destroy(IntPtr ptr);

    [LibraryImport(LibName, EntryPoint = "sim_get_map_ptr")]
    public static partial IntPtr Sim_GetMapPtr(IntPtr ptr);

    [LibraryImport(LibName, EntryPoint = "sim_get_map_len")]
    public static partial long Sim_GetMapLen(IntPtr ptr);

    [LibraryImport(LibName, EntryPoint = "sim_entity_get_float")]
    public static partial byte Sim_TryGetEntityFloat(IntPtr ptr, uint x, uint y, uint z, ushort propId, out float out_value);

    [LibraryImport(LibName, EntryPoint = "sim_entity_get_int")]
    public static partial byte Sim_TryGetEntityInt(IntPtr ptr, uint x, uint y, uint z, ushort propId, out float out_value);

    [LibraryImport(LibName, EntryPoint = "sim_tick")]
    public static partial void Sim_Tick(IntPtr ptr);

}
