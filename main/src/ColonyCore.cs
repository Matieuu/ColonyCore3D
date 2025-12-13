using System.Drawing;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ColonyCore {
    class ColonyCore {
        
        private IWindow window;
        
        private GL gl = null!;
        private IInputContext input = null!;
        private ImGuiController controller = null!;

        private IntPtr gameState = IntPtr.Zero;

        public ColonyCore() {
            var options = WindowOptions.Default;
            options.Title = "Colony Core 3D";

            options.Size = new Vector2D<int>(1600, 900);
            options.VSync = true;

            window = Window.Create(options);

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Closing += OnClose;

            window.Run();
        }

        private void OnLoad() {
            gl = window.CreateOpenGL();
            input = window.CreateInput();
            controller = new ImGuiController(gl, window, input);

            gameState = NativeLib.InitGame();
        }

        private void OnUpdate(double deltaTime) {
            NativeLib.AddTicks(gameState, 1);

            controller.Update((float)deltaTime);
        }

        private void OnRender(double deltaTime) {
            gl.ClearColor(Color.CornflowerBlue);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            ImGui.Begin("Ticki");
            ImGui.Text(NativeLib.GetTicks(gameState).ToString());
            if (ImGui.Button("Zapisz ticki")) {
                Console.WriteLine("Ilość ticków: " + NativeLib.GetTicks(gameState));
            }
            ImGui.End();

            controller.Render();
        }

        private void OnClose() {
            NativeLib.DestroyGame(gameState);
            controller?.Dispose();
            gl?.Dispose();
        }
    }
}