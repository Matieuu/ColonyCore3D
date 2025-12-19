# ColonyCore 3D – Micro-Milestones

## M0: Setup & Handshake (Konsola, bez grafiki)
*Cel: Sprawienie, by C# i Rust w ogóle ze sobą rozmawiały. Jeśli to nie zadziała, dalsza praca nie ma sensu.*

### M0.1: Rust Library
- [x] Stwórz nowy projekt Rust: `cargo new --lib colony_core`.
- [x] W `Cargo.toml` ustaw typ crate'a: `crate-type = ["cdylib"]`.
- [x] Napisz funkcję `add(a, b)`:
    - Musi być publiczna (`pub`).
    - Musi mieć `extern "C"`.
    - Musi mieć `#[no_mangle]`.
- [x] Zbuduj projekt: `cargo build`. Sprawdź, czy masz plik `.dll` (Windows) w folderze `target/debug`.

### M0.2: C# Host
- [x] Stwórz projekt konsolowy C# (.NET 8/9).
- [x] Skopiuj `.dll` z Rusta do folderu, gdzie buduje się C# (np. `bin/Debug/net8.0`) – **automatyzacja tego to Twój przyjaciel (Post-Build Event)**.
- [x] Napisz klasę `NativeLib` z `[DllImport("colony_core.dll")]` dla funkcji `add`.
- [x] Wywołaj funkcję w `Main`. Wypisz wynik na konsolę.
- [x] **Sanity Check:** Czy `Console.WriteLine` pokazuje poprawny wynik dodawania?

### M0.3: Pamięć i Wskaźniki
- [x] W Rust: Stwórz struct `GameState { ticks: u32 }`.
- [x] W Rust: Napisz funkcję `init_game() -> *mut GameState`, która alokuje struct na stercie (`Box::new`) i zwraca wskaźnik (`Box::into_raw`).
- [x] W C#: Odbierz ten wskaźnik jako `IntPtr`.
- [x] W Rust: Napisz funkcję `get_ticks(ptr: *mut GameState) -> u32`.
- [x] W C#: Wywołaj `get_ticks` w pętli `while(true)`, przekazując wskaźnik.
- [x] **Sanity Check:** Czy licznik rośnie, a aplikacja nie wywala się z "Access Violation"?

---

## M1: Pierwsze Piksele (Hello Triangle)
*Cel: Uruchomienie OpenGL. To najtrudniejszy etap "setupowy". Po nim będzie z górki.*

### M1.1: Okno Silk.NET
- [x] Dodaj NuGety: `Silk.NET.OpenGL`, `Silk.NET.Windowing`, `Silk.NET.Input`.
- [x] Skopiuj boilerplate do stworzenia okna z dokumentacji Silk.NET.
- [x] Ustaw kolor tła (`gl.ClearColor`) na np. "Cornflower Blue" (taki ładny fioletowo-niebieski).
- [x] **Sanity Check:** Czy po odpaleniu masz kolorowe okno, które można zamknąć krzyżykiem?

### M1.2: Shadery (Kopiuj-Wklej)
- [x] Stwórz plik `shader.vert` (prosty GLSL, przekazuje pozycję).
- [x] Stwórz plik `shader.frag` (prosty GLSL, zwraca stały kolor, np. biały).
- [x] Napisz w C# klasę `Shader` (ładowanie tekstu z pliku -> kompilacja -> linkowanie programu). To jest nudne, możesz skopiować gotowca z tutoriali Silk.NET.

### M1.3: Trójkąt
- [x] Zdefiniuj tablicę `float[] vertices` dla jednego trójkąta.
- [x] Utwórz VAO (Vertex Array Object) i VBO (Vertex Buffer Object).
- [x] W pętli `OnRender`: `shader.Use()`, `vao.Bind()`, `gl.DrawArrays()`.
- [x] **Sanity Check:** Widzisz biały trójkąt na niebieskim tle? Gratulacje, masz silnik graficzny.

### M1.4: ImGui (Must Have)
- [x] Dodaj `Silk.NET.OpenGL.Extensions.ImGui`.
- [x] Zainicjalizuj kontroler ImGui.
- [x] W pętli renderowania dodaj `ImGui.Begin("Debug")`, `ImGui.Text("FPS: " + fps)`, `ImGui.End()`.
- [x] **Sanity Check:** Czy masz małe okienko z tekstem nad swoim trójkątem?

---

## M2: Voxel World (To na co czekasz)
*Cel: Zamiast trójkąta, rysujemy świat gry z Rusta.*

### M2.1: Model Sześcianu
- [x] Zmień dane wierzchołków z trójkąta na sześcian (Cube).
- [x] Użyj `glDrawElements` (EBO - Element Buffer Object), żeby nie powtarzać wierzchołków.
- [x] Dodaj macierze transformacji w C# (`Matrix4x4.CreatePerspective...`, `CreateLookAt...`).
- [x] Prześlij macierze do shadera jako `Uniform`.
- [x] **Sanity Check:** Widzisz obracający się sześcian 3D?

### M2.2: Płaski Świat w Rust
- [x] W Rust: Stwórz `Vec<BlockType>` o rozmiarze np. 16x16. Wypełnij losowymi danymi.
- [x] W Rust: Eksportuj funkcję `get_map_ptr()` i `get_map_len()`.

### M2.3: Instanced Rendering (Big Boss)
- [x] W C#: Zmień `glDrawElements` na `glDrawElementsInstanced`.
- [x] Stwórz drugi VBO (Instance VBO) trzymający pozycje (X, Y, Z) dla każdej instancji.
- [x] W `OnRender`: Pobierz wskaźnik z Rusta, skopiuj dane do Instance VBO (`glBufferSubData`).
- [x] W Vertex Shader: Dodaj `layout (location = 1) in vec3 aInstancePos` i dodaj to do pozycji wierzchołka.
- [x] **Sanity Check:** Widzisz siatkę 16x16 sześcianów? Wygląda jak podłoga?

### M2.4: Izometria
- [x] Zmień macierz projekcji z Perspective na Orthographic.
- [x] Ustaw kamerę pod kątem (np. 45 stopni w dół, 45 w bok).
- [x] **Sanity Check:** Wygląda jak klasyczny RPG/RTS?

---

## M3: Interakcja (Myszka)
*Cel: Klikanie w klocki.*

### M3.1: Kamera Ruchoma
- [x] Obsłuż zdarzenia klawiatury (WSAD) w C#.
- [x] Aktualizuj pozycję kamery ("Eye") w macierzy View.
- [x] **Sanity Check:** Możesz "latać" nad mapą?

### M3.2: Raycast Math
- [ ] Pobierz pozycję myszy (X, Y) z okna.
- [ ] Przekształć ją na "Ray World Direction" (korzystając z odwrotności macierzy). Wyświetl wartości w ImGui.
- [ ] **Sanity Check:** Gdy ruszasz myszką, wartości wektora w ImGui się zmieniają sensownie?

### M3.3: Integracja
- [ ] Prześlij Origin i Direction kamery do Rusta.
- [ ] W Rust: Napisz prostą pętlę sprawdzającą kolizję z podłogą (na razie brute-force lub proste `if z == 0`).
- [ ] W C#: Narysuj "Wireframe Cube" (ramkę) w miejscu, które zwrócił Rust.
- [ ] **Sanity Check:** Czy ramka "skacze" po klockach pod Twoją myszką?

---

## M4: Logika (Dopiero teraz!)
*Cel: Coś się rusza samo.*

### M4.1: Jednostka
- [ ] W Rust: Dodaj `struct Pawn { x: f32, y: f32 }`.
- [ ] W Rust: W `sim_tick` zmieniaj `x` o małą wartość (np. `0.01`).
- [ ] W C#: Pobierz listę pionków i wyrenderuj je jako inny kolor sześcianu (lub inny model).
- [ ] **Sanity Check:** Czy czerwony klocek przesuwa się powoli po mapie?

### M4.2: Game Loop
- [ ] Zaimplementuj akumulator czasu w C# (Fixed Time Step).
- [ ] Rozdziel logiczny FPS (60) od graficznego FPS (unlimited).
- [ ] **Sanity Check:** Gra działa tak samo szybko na 60 FPS i na 500 FPS?