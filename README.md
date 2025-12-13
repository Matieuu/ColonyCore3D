# ColonyCore 3D – Specyfikacja Techniczna Projektu

## 1. Założenia Architektoniczne
Projekt to symulator kolonii w rzucie izometrycznym. Aplikacja podzielona jest na dwie warstwy działające w jednej przestrzeni pamięci procesora.

* **CORE (Rust):** "Mózg". Odpowiada za stan świata, logikę, pathfinding i zarządzanie pamięcią. Nie posiada zależności do bibliotek graficznych.
* **HOST (C#):** "Ciało". Odpowiada za okno systemowe, wejście (mysz/klawiatura), renderowanie grafiki (OpenGL/Vulkan) i UI.

### Model Pamięci
* Rust zarządza alokacją i zwalnianiem pamięci świata gry.
* C# otrzymuje jedynie wskaźniki (`IntPtr` / `unsafe pointer`) do danych Rusta.
* **Zasada Zero-Copy:** C# nigdy nie kopiuje całych tablic danych (np. mapy), czyta je bezpośrednio z pamięci Rusta przy użyciu `Span<T>`.

---

## 2. Moduł CORE (Rust) – "Mózg"

Tutaj piszemy czystą logikę. Ten kod nie wie, że istnieje ekran, karta graficzna czy klawiatura.

### 2.1 Zarządzanie Pamięcią i Struktury
Celujemy w **Zero-Cost Abstractions**.

#### A. Struktura Świata (The World)
* **Struktura:** `SimulationContext`
* **Safety:** Używamy `Arc<RwLock<World>>`.
* **Cel:** Nawet pisząc własny silnik, warto oddzielić wątek renderowania (C#) od wątku symulacji (Rust). `RwLock` pozwoli Ci bezpiecznie robić snapshoty danych do rysowania w trakcie trwania obliczeń.

#### B. Płaska struktura danych (Data-Oriented Design)
Zamiast trzymać obiekty rozsiane po pamięci, w podejściu "low-level" warto trzymać je w wektorach.
* **Mapa:** `Vec<Tile>` (jednowymiarowa tablica reprezentująca grid 2D: `index = y * width + x`). To jest drastycznie szybsze dla procesora (CPU Cache) niż tablice tablic.
* **Jednostki:** `Vec<Pawn>`.

#### C. System Zadań (Jobs)
* **Typ:** `Rc<JobDefinition>` (jeśli symulacja jest jednowątkowa) lub `Arc<JobDefinition>` (jeśli wielowątkowa).
* **Zasada:** Definicje zadań ("Zetnij", "Buduj") są ładowane raz przy starcie i są tylko do odczytu (static data). Jednostki odnoszą się do nich przez wskaźnik.

### 2.2 Główna Pętla (The Tick)
Funkcja `update` wykonuje jeden krok dyskretny.
1.  **Logika:** A* Pathfinding (na spłaszczonym wektorze mapy).
2.  **Stan:** Aktualizacja maszyn stanów (State Machine) jednostek.
3.  **Snapshotting (Opcjonalne, ale zalecane):** Przygotowanie bufora danych dla renderera, aby C# nie musiał skakać po całej pamięci Rusta.

---

## 3. Moduł HOST (C# + Silk.NET) – "Silnik"

Tutaj budujesz fundamenty silnika gry. Będziesz musiał ręcznie obsłużyć OpenGL.

### 3.1 Stack Technologiczny
* **Silk.NET.OpenGL:** Nowoczesny, bardzo chudy wrapper na OpenGL (lżejszy niż OpenTK).
* **Silk.NET.Windowing:** Do stworzenia okna i obsługi kontekstu.
* **ImGui.NET:** Gorąco rekomenduję zintegrować to od razu. Własny silnik bez UI do debugowania to koszmar.

### 3.2 Pipeline Graficzny (To musisz napisać sam)

#### A. Shadery (GLSL)
Będziesz potrzebować dwóch prostych programów:
* **Vertex Shader:** Przymuje pozycję wierzchołka (lokalną) + pozycję instancji (ze świata). Mnoży to przez macierze MVP (Model-View-Projection).
* **Fragment Shader:** Ustala kolor piksela.

#### B. Instanced Rendering (Klucz do wydajności)
To jest najważniejszy punkt w silniku klockowym.
* **Nie rób:** `foreach (cube) { DrawCube(); }` – to "zabije" CPU (tysiące draw calls).
* **Zrób:** **Instancing**.
    1.  Wgrywasz model sześcianu do GPU **raz** (VBO - Vertex Buffer Object).
    2.  Tworzysz drugi bufor (Instance VBO) z pozycjami (x,y,z) i kolorami wszystkich 10,000 obiektów.
    3.  Wywołujesz `glDrawElementsInstanced` **raz**. Karta graficzna narysuje cały świat w jednym rzucie.

### 3.3 Kamera Matematyczna
Musisz ręcznie zbudować macierze (używając `System.Numerics.Matrix4x4`).
* **Projection:** `Matrix4x4.CreateOrthographic(...)`.
* **View:** `Matrix4x4.CreateLookAt(...)`.
    * Pozycja kamery: np. `(100, 100, 100)`
    * Cel: `(0, 0, 0)`
    * Góra: `(0, 1, 0)`
* **Model:** Macierz transformacji dla każdego sześcianu (często wystarczy tylko wektor przesunięcia w shaderze dla optymalizacji).

---

## 4. Interfejs FFI (Most)

Ponieważ piszesz "bare metal", musisz bardzo uważać na wskaźniki.

### 4.1 4.1 Struktury Danych (DTO)
Struktura przesyłana do GPU musi mieć identyczny układ w pamięci w obu językach.

**Rust (Core):**
```rust
#[repr(C)]
pub struct RenderEntity {
    pub pos_x: f32,
    pub pos_y: f32, // Wysokość
    pub pos_z: f32,
    pub color_pack: u32, // Spakowany kolor RGBA (4 bajty)
}
```

**C# (Engine):**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct RenderEntity {
    public float X;
    public float Y;
    public float Z;
    public uint Color;
}
```

### 4.2 Eksportowane API (Rust)
| Funkcja | Sygnatura | Opis |
| :--- | :--- | :--- |
| `sim_create` | `(w, h) -> *mut void` | Inicjalizuje świat i zwraca wskaźnik. |
| `sim_tick` | `(ctx, dt)` | Wykonuje krok symulacji. |
| `sim_get_render_len` | `(ctx) -> usize` | Zwraca liczbę obiektów do narysowania. |
| `sim_get_render_ptr` | `(ctx) -> *const RenderEntity` | Zwraca wskaźnik do tablicy obiektów. |

### 4.3 Pętla Renderowania (C# Pseudokod)
```csharp
protected override void OnRender(double delta) {
    // 1. Logika (Rust)
    NativeLib.sim_tick(_simHandle, (float)delta);

    // 2. Pobranie danych
    var count = NativeLib.sim_get_render_len(_simHandle);
    var ptr = NativeLib.sim_get_render_ptr(_simHandle);

    // 3. Transfer do GPU (Zero-GC)
    // Bezpośrednie kopiowanie pamięci z Rusta do bufora OpenGL
    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceBuffer);
    _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(count * sizeof(RenderEntity)), ptr);

    // 4. Rysowanie
    _gl.UseProgram(_shaderProgram);
    // ... bind textures / uniforms ...
    _gl.DrawElementsInstanced(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, null, (uint)count);
}
```

---

## 5. Interakcja: Raycasting i Mouse Picking
W grze izometrycznej/3D najtrudniejszym elementem UX jest to, że myszka porusza się po płaszczyźnie ekranu (2D), a Ty musisz precyzyjnie wskazać konkretny sześcian (lub ścianę sześcianu) w przestrzeni 3D.

### 5.1 Matematyka Unprojection (C# Host)
Host musi zamienić koordynaty kursora myszy (x, y) na promień (Ray) w świecie gry. Jest to doskonałe ćwiczenie z algebry liniowej.
1.  **Normalizacja (NDC):** Zamień pozycję myszy na zakres [-1, 1].
2.  **Odwrócenie Macierzy:** Użyj odwrotności macierzy View i Projection: `P_world = Inverse(M_proj * M_view) * P_ndc`.
3.  **Ray Origin & Direction:** Obliczasz punkt startowy (kamera) i wektor kierunkowy. Te dane (6 floatów) przesyłasz do Rusta przez FFI.

### 5.2 Voxel Traversal (Rust Core)
Nie używaj prostej kolizji ze wszystkimi obiektami (to za wolne i nieedukacyjne). Ponieważ Twój świat to grid (siatka), użyj algorytmu **DDA (Digital Differential Analyzer)** lub "Fast Voxel Traversal Algorithm" (Amanatides & Woo). To standard w silnikach voxelowych.

**API FFI (Rust):**
```rust
#[no_mangle]
pub extern "C" fn sim_raycast(
    ctx: *mut SimulationContext, 
    ray_origin_x: f32, ray_origin_y: f32, ray_origin_z: f32,
    ray_dir_x: f32, ray_dir_y: f32, ray_dir_z: f32
) -> RaycastResult {
    // Algorytm "kroczy" po siatce voxeli wzdłuż promienia.
    // Zwraca koordynaty pierwszego napotkanego nie-pustego bloku
    // ORAZ normalną ściany (żeby wiedzieć, czy budujemy "na", czy "obok").
}
```

### 5.3 Selekcja i Gizmo
Host (C#) po otrzymaniu wyniku z Rusta musi narysować "ducha" (ghost object) lub ramkę selekcji (wireframe cube) w miejscu wskazanym przez `RaycastResult`. To daje graczowi natychmiastowy feedback.

---

## 6. Architektura Pętli Czasu (Game Loop)
RimWorld jest deterministyczny. Oznacza to, że przy tych samych danych wejściowych, symulacja zawsze przebiegnie tak samo. Wymaga to **rozdzielenia czasu renderowania od czasu symulacji**. To jedno z najważniejszych zagadnień w inżynierii silników gier.

### 6.1 Fixed Time Step (Akumulator)
Renderowanie (C#) może działać w 144 FPS lub 30 FPS, ale fizyka/logika kolonii (Rust) musi działać zawsze stałym tempie, np. 60 Tickach na sekundę (TPS).

**Algorytm w C# (Główna pętla):**
```csharp
double accumulator = 0.0;
double dt = 1.0 / 60.0; // Stały krok symulacji (16.6ms)

void OnUpdate(double deltaRender) {
    accumulator += deltaRender;

    // Pętla "doganiająca" symulację (Decoupled sim speed from frame rate)
    // Jeśli gra zwolni (lag), ta pętla wykona się kilka razy,
    // aby logika gry "dogoniła" czas rzeczywisty.
    while (accumulator >= dt) {
        NativeLib.sim_tick(_simHandle, dt); // Rust liczy logiczny krok
        accumulator -= dt;
    }

    // Renderowanie z interpolacją
    // alpha mówi nam, w którym momencie "pomiędzy" tickami fizyki jesteśmy (0.0 - 1.0)
    double alpha = accumulator / dt; 
    RenderWorld(alpha); 
}
```

### 6.2 Interpolacja Stanu (Render State)
Aby ruch jednostek był płynny przy Fixed Time Step (szczególnie przy monitorach o wysokim odświeżaniu), nie możesz po prostu rysować `Position`.
Rust powinien zwracać dwie pozycje dla każdego obiektu:
1.  `PreviousPosition` (z poprzedniego ticka)
2.  `CurrentPosition` (z obecnego ticka)

Shader w C# (lub logika przed wysłaniem do GPU) wylicza pozycję wizualną:
`Pos_visual = Lerp(Pos_prev, Pos_current, alpha)`

Bez tego mechanizmu ruch kamery i jednostek będzie szarpał (jitter), nawet przy wysokim FPS.