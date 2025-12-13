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