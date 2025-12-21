# ColonyCore 3D

**ColonyCore 3D** to silnik symulacji kolonii i automatyzacji oparty na wokselach, zbudowany z naciskiem na wydajność i czystą separację logiki od warstwy prezentacji. Projekt wykorzystuje architekturę hybrydową, łączącą niskopoziomową wydajność i bezpieczeństwo pamięci Rusta z ekosystemem .NET do obsługi okna i grafiki.

## 🏛 Architektura

System działa w jednej przestrzeni pamięci, ale jest logicznie podzielony na dwie niezależne warstwy:

### 1. CORE (Mózg) – Rust
Odpowiada za kompletny stan symulacji. Nie posiada żadnych zależności do bibliotek graficznych ani systemowych (OS).
* **Data-Oriented Design:** Świat reprezentowany jest jako płaskie tablice (`Vec<u16>`), co zapewnia optymalizację pod kątem CPU Cache.
* **Logika "Headless":** Symulacja może działać bez okna (np. na serwerze).
* **Entity System:** Obsługa maszyn i obiektów z własnym stanem (np. `Furnace`, `Chest`) poprzez trait `BlockEntity`.
* **Raycasting:** Własna implementacja algorytmu śledzenia promienia w siatce wokselowej (DDA) do precyzyjnej selekcji bloków i ścian.

### 2. HOST (Ciało) – C# (.NET 10 + Silk.NET)
Odpowiada za wizualizację i interakcję z użytkownikiem.
* **OpenGL 3.3+:** Bezpośrednie wywołania OpenGL przez Silk.NET.
* **Instanced Rendering:** Cały teren renderowany jest za pomocą jednego wywołania `glDrawArraysInstanced` (lub kilku dla różnych typów meshy), co pozwala na wyświetlanie dziesiątek tysięcy wokseli w 60+ FPS.
* **Zero-Copy Rendering:** Host pobiera wskaźniki (`unsafe`) bezpośrednio do pamięci Rusta i przesyła je do GPU, unikając kosztownego kopiowania tablic w pamięci RAM.
* **ImGui:** Zintegrowany interfejs debugowania i narzędziowy.

---

## 🛠 Stack Technologiczny

* **Core:** Rust (edycja 2024), kompilowany do biblioteki dynamicznej (`.dll` / `.so`).
* **Host:** C# / .NET 10.
* **Grafika:** OpenGL (via Silk.NET).
* **UI:** ImGui.NET.
* **Matematyka:** Silk.NET.Maths (System.Numerics).

---

## 🔌 Interfejs FFI (Rust <-> C#)

Komunikacja odbywa się poprzez surowe wskaźniki C. Rust eksponuje API, które C# importuje jako funkcje `[LibraryImport]`.

Przykładowy przepływ danych:
1.  **Init:** C# prosi Rusta o alokację świata (`sim_init`).
2.  **Tick:** C# wywołuje `sim_tick` (logika symulacji, np. spalanie paliwa w piecach).
3.  **Render:** C# pobiera wskaźnik do mapy (`sim_get_map_ptr`) i aktualizuje bufory instancji VBO.
4.  **Input:** C# przelicza pozycję myszy na promień (Ray) i wysyła do Rusta (`sim_raycast`), otrzymując wynik trafienia (blok + ściana).

Przykładowa sygnatura API (Rust):
```rust
#[unsafe(no_mangle)]
pub extern "C" fn sim_raycast(ptr: *mut World, ray: Ray) -> RaycastResult {
    // ... Logika traversalu wokseli ...
}
```

---

## 🚀 Jak uruchomić

### Wymagania
* **Rust:** Zainstalowany toolchain (`cargo`).
* **C#:** .NET SDK 10.0 (lub nowszy).

W katalogu głównym jest skrypt `build.sh`, należy go uruchomić, ewentualnie dodać flagę `--release`.

---

## 🗺 Roadmapa i Status

Projekt jest w fazie aktywnego rozwoju fundamentów silnika.

### ✅ Zaimplementowano (Milestone 0-3)
* [x] Dwukierunkowa komunikacja FFI (C# <-> Rust).
* [x] Renderowanie świata metodą Instanced Rendering.
* [x] Kamera izometryczna z obsługą Zoom i Pan.
* [x] Raycasting 3D (wybieranie bloków myszką z uwzględnieniem ścian).
* [x] Podstawowy system encji (bloków z logiką, np. Piece).
* [x] Integracja ImGui do podglądu zmiennych.

### 🚧 W toku (Milestone 4: Life & Construction)
* [ ] Dynamiczne stawianie i niszczenie bloków przez gracza (PPM/Shift+PPM).
* [ ] System "Dirty Chunks" do optymalizacji przesyłu danych do GPU.
* [ ] Wprowadzenie jednostek (Pawn) niezależnych od siatki wokseli.
* [ ] Interpolacja ruchu jednostek między tickami logicznymi.

### 🔮 Plany (Milestone 5+)
* [ ] **System Zadań (Job System):** Inteligentne przydzielanie pracy (kopanie, transport) dostępnym jednostkom.
* [ ] **Pathfinding:** A* na grafie wokselowym.
* [ ] **Logistyka:** Taśmociągi i automatyczne podajniki między maszynami.
* [ ] **Potrzeby:** System głodu, energii i morale dla kolonistów.

---

## 📝 Sterowanie (Debug)

| Klawisz / Mysz | Akcja |
| :--- | :--- |
| **WSAD** | Przesuwanie kamery (Pan) |
| **Q / E** | Obrót kamery (Orbit) |
| **Scroll** | Przybliżanie / Oddalanie (Zoom) |
| **LPM** | Selekcja bloku (Raycast test) |
| **PPM** | Stawianie bloków bądź wydawanie takich poleceń |
| **Shift + PPM** | Niszczenie bloków bądź wydawanie takich poleceń |
