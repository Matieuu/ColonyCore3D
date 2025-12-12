using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ColonyCore {
    class ColonyCore {
        
        private IWindow window;
        private GL? gl;
        private ImGuiController? controller;

        public ColonyCore() {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1600, 900);
            options.Title = "Colony Core 3D";

            window = Window.Create(options);

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Closing += OnClose;

            window.Run();
        }

        private void OnLoad() {
            IInputContext input = window.CreateInput();
            gl = window.CreateOpenGL();
            controller = new ImGuiController(gl, window, input);

            IntPtr playerHandle = NativeLib.player_create("Matieuu", 10d, 10d);
            Console.WriteLine($"[C#] Otrzymano wskaźnik do gracza: {playerHandle}");

            for (int i = 0; i < 3; i++) {
                NativeLib.player_move(playerHandle, .5d, .5d);
                if (i == 1) NativeLib.player_damage(playerHandle, 25);

                unsafe {
                    Player* player = (Player*)playerHandle;
                    Console.WriteLine($"[C#] RENDER: Rysuję gracza w ({player->X:F1}, {player->Y:F1}), HP: {player->Health}");
                }
            }

            NativeLib.player_destroy(playerHandle);
            Console.WriteLine("\n[C#] Gracz usunięty");
        }

        private void OnUpdate(double deltaTime) {
            if (controller == null) return;

            controller.Update((float)deltaTime);
        }

        private void OnRender(double deltaTime) {
            if (controller == null || gl == null) return;

            gl.ClearColor(.1f, .1f, .1f, 1f);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            ImGui.Begin("Moje okienko");
            ImGui.Text("Witaj w C# i Silk.NET");
            if (ImGui.Button("Kliknij mnie!")) {
                Console.WriteLine("Przycisk został kliknięty");
            }
            ImGui.End();

            controller.Render();
        }

        private void OnClose() {
            controller?.Dispose();
            gl?.Dispose();
        }
    }
}