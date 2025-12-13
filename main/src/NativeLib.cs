using System.Reflection;
using System.Runtime.InteropServices;

namespace ColonyCore {
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

        // [DllImport(LibName, EntryPoint = "init_game", CallingConvention = CallingConvention.Cdecl)]
        // public static extern IntPtr InitGame();

        [LibraryImport(LibName, EntryPoint = "init_game")]
        public static partial IntPtr InitGame();

        [LibraryImport(LibName, EntryPoint = "destroy_game")]
        public static partial void DestroyGame(IntPtr ptr);

        [LibraryImport(LibName, EntryPoint = "add_ticks")]
        public static partial uint AddTicks(IntPtr ptr, uint amount);

        [LibraryImport(LibName, EntryPoint = "get_ticks")]
        public static partial uint GetTicks(IntPtr ptr);

    }
}
