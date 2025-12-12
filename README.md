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

## 5. Szczegółowy Backlog Zadań (Roadmapa od 0 do MVP)

### Milestone 1: The Virtual World (Fundamenty Terenu)
*Cel: Wyświetlenie statycznego świata voxelowego w rzucie izometrycznym.*

**Core (Rust):**
- [ ] Zaimplementowanie struktury `VoxelMap` jako płaskiej tablicy 1D (`Vec<u8>`) mapującej koordynaty (x,y,z).
- [ ] Implementacja logiki dostępu do sąsiadów (`get_neighbor(x,y,z, direction)`).
- [ ] Eksport surowych danych mapy przez FFI (wskaźnik do bufora kolorów/typów bloków).

**Host (C#):**
- [ ] Implementacja **Instanced Rendering** dla sześcianów (jeden mesh kostki, tysiące instancji przesłanych w buforze).
- [ ] Implementacja kamery izometrycznej (Ortograficzna) z obsługą Zoomu (zmiana skali projekcji) i Pan (przesuwanie kamery).
- [ ] Optymalizacja renderowania: Culling (nie wysyłaj do GPU kostek, które są "powietrzem").

---

### Milestone 2: The Cursor & Interaction (Interakcja)
*Cel: Gracz może wskazać konkretny blok w przestrzeni 3D myszką.*

**Host (C#):**
- [ ] Implementacja **Raycastingu 3D**:
    - [ ] Przeliczenie pozycji myszy (Screen Space) na promień w świecie (World Space).
    - [ ] Obliczenie punktu przecięcia promienia z płaszczyzną terenu (Plane $Y=0$).
- [ ] Implementacja "Ghost Cursor": Wyświetlanie półprzezroczystej kostki w miejscu, gdzie wskazuje mysz (snapping do siatki).
- [ ] Przesyłanie zdarzeń kliknięcia (LPM/PPM) do Rusta wraz ze współrzędnymi (x,y,z).

**Core (Rust):**
- [ ] Obsługa komendy "Inspect": Po otrzymaniu koordynatów (x,y,z), zwróć typ bloku i informacje o nim (np. "Dirt Block").

---

### Milestone 3: The Colonists (Ruch i AI)
*Cel: Jednostki poruszają się inteligentnie po mapie.*

**Core (Rust):**
- [ ] **Struktura Unit:** Zdefiniowanie `struct Pawn` z unikalnym ID i pozycją (float).
- [ ] **State Machine (`Box<dyn State>`):**
    - [ ] Stworzenie traita `UnitState`.
    - [ ] Implementacja stanu `StateIdle`.
    - [ ] Implementacja stanu `StateMoving`.
- [ ] **Pathfinding (A*):**
    - [ ] Implementacja algorytmu A-Star na grafie voxelowym.
    - [ ] Uwzględnienie kolizji (nie wchodź w ściany).
- [ ] **Tick Loop:** Interpolacja ruchu jednostki w każdej klatce (przesuwanie `x,y` w stronę kolejnego punktu ścieżki).

**Host (C#):**
- [ ] Pobieranie listy jednostek (ID, Typ, X, Y, Z) co klatkę.
- [ ] Renderowanie jednostek (np. jako proste kapsułki lub inne modele) w ich aktualnych pozycjach.
- [ ] Interpolacja wizualna (Smoothing): Jeśli tick Rusta jest rzadszy niż FPS C#, wygładzaj ruch między klatkami.

---

### Milestone 4: The Job System (System Pracy - Serce Gry)
*Cel: Jednostki same szukają zadań (np. zetnij drzewo) i je wykonują.*

**Core (Rust):**
- [ ] **Global Job Queue:** Kolejka dostępnych zadań (np. `JobType::CutTree`).
- [ ] **Job Definition (`Rc`):**
    - [ ] Stworzenie definicji zadań (czas trwania, wymagane narzędzie).
    - [ ] Użycie `Rc` do współdzielenia tych definicji między tysiącami instancji zadań.
- [ ] **AI Decision:** Logika "Brain" dla osadnika:
    - [ ] *Czy jestem bezrobotny?* -> Sprawdź kolejkę zadań.
    - [ ] *Czy zadanie jest osiągalne?* -> Sprawdź Pathfinding.
    - [ ] *Zarezerwuj zadanie* -> Przypisz do siebie, usuń z kolejki globalnej.
- [ ] **Action Execution:** Implementacja stanu `StateWorking` (pasek postępu pracy).

**Host (C#):**
- [ ] UI: Wyświetlanie paska postępu nad głową jednostki, gdy jest w stanie `StateWorking`.
- [ ] UI: Menu kontekstowe (Prawy przycisk na drzewo -> "Mark for harvest").

---

### Milestone 5: Mining & Building (Modyfikacja Świata)
*Cel: Gracz i jednostki mogą zmieniać strukturę mapy.*

**Core (Rust):**
- [ ] **Modyfikacja VoxelMap:** Funkcja zmieniająca typ bloku w (x,y,z) np. z `Stone` na `Air` (kopanie) lub z `Air` na `Wall` (budowanie).
- [ ] **Dirty Flag:** Oznaczanie chunka mapy jako "zmieniony", aby C# wiedział, że musi odświeżyć grafikę.
- [ ] **Pathfinding Recalculation:** Jeśli postawiono ścianę, jednostki muszą przeliczyć ścieżki.

**Host (C#):**
- [ ] Obsługa "Dirty Chunk": Jeśli Rust zgłosi zmianę, przebuduj bufory instancji dla danego fragmentu mapy.
- [ ] System "Designations": Wizualne oznaczanie bloków do wykopania (np. czerwona nakładka na blokach).

---

### Milestone 6: Needs & Survival (Zarządzanie Zasobami)
*Cel: Jednostki muszą jeść i spać.*

**Core (Rust):**
- [ ] **Need System:** Struktura trzymająca paski potrzeb (Głód, Energia, Komfort).
- [ ] **Resource Inventory:** Globalny magazyn surowców (Drewno, Jedzenie) zabezpieczony przez `Arc<Mutex<Inventory>>`.
- [ ] **AI Override:** Jeśli `Hunger < 10%`, przerwij pracę i szukaj jedzenia.
- [ ] **Item Hauling:** Zadanie przenoszenia surowców z ziemi do magazynu.

**Host (C#):**
- [ ] GUI: Panel boczny wyświetlający statystyki wybranego osadnika (Wykresy głodu/energii).
- [ ] GUI: Wyświetlanie surowców leżących na ziemi (małe modele 'lootu').

---

### Milestone 7: Optimization (Zaawansowane)
*Cel: Obsługa 100x100x20 świata i 100 jednostek w 60 FPS.*

**Core (Rust):**
- [ ] **Multithreading:** Przeniesienie Pathfindingu do `ThreadPool` (Rust: `rayon` lub `std::thread`).
- [ ] **Spatial Partitioning:** Użycie Octree lub Grid Partitioning do szybszego wyszukiwania sąsiadów/kolizji.

**Host (C#):**
- [ ] **Greedy Meshing:** Łączenie sąsiadujących ścian tego samego typu w jeden duży prostokąt, aby zmniejszyć liczbę wierzchołków.
- [ ] **Texture Atlases:** Użycie jednej dużej tekstury dla wszystkich bloków.