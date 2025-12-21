public static class Constants {

    public const ushort PROP_FUEL_LEVEL = 1;
    public const ushort PROP_MAX_FUEL = 2;

    public const ushort PROP_IS_ACTIVE = 100;
    public const ushort PROP_ITEM_COUNT = 101;

    public static float[] CubeVertices => [
        // X, Y, Z,           R, G, B
        // Ściana Przednia
        0f, 0f, 1f,  0.6f, 0.6f, 0.6f,
        1f, 0f, 1f,  0.6f, 0.6f, 0.6f,
        1f, 1f, 1f,  0.6f, 0.6f, 0.6f,
        1f, 1f, 1f,  0.6f, 0.6f, 0.6f,
        0f, 1f, 1f,  0.6f, 0.6f, 0.6f,
        0f, 0f, 1f,  0.6f, 0.6f, 0.6f,

        // Ściana Tylna
        0f, 0f, 0f,  0.4f, 0.4f, 0.4f,
        1f, 0f, 0f,  0.4f, 0.4f, 0.4f,
        1f, 1f, 0f,  0.4f, 0.4f, 0.4f,
        1f, 1f, 0f,  0.4f, 0.4f, 0.4f,
        0f, 1f, 0f,  0.4f, 0.4f, 0.4f,
        0f, 0f, 0f,  0.4f, 0.4f, 0.4f,

        // Ściana Lewa
        0f, 1f, 1f, 0.5f, 0.5f, 0.5f,
        0f, 1f, 0f, 0.5f, 0.5f, 0.5f,
        0f, 0f, 0f, 0.5f, 0.5f, 0.5f,
        0f, 0f, 0f, 0.5f, 0.5f, 0.5f,
        0f, 0f, 1f, 0.5f, 0.5f, 0.5f,
        0f, 1f, 1f, 0.5f, 0.5f, 0.5f,

        // Ściana Prawa
        1f, 1f, 1f,  0.7f, 0.7f, 0.7f,
        1f, 1f, 0f,  0.7f, 0.7f, 0.7f,
        1f, 0f, 0f,  0.7f, 0.7f, 0.7f,
        1f, 0f, 0f,  0.7f, 0.7f, 0.7f,
        1f, 0f, 1f,  0.7f, 0.7f, 0.7f,
        1f, 1f, 1f,  0.7f, 0.7f, 0.7f,

        // Ściana Dolna
        0f, 0f, 0f,  0.3f, 0.3f, 0.3f,
        1f, 0f, 0f,  0.3f, 0.3f, 0.3f,
        1f, 0f, 1f,  0.3f, 0.3f, 0.3f,
        1f, 0f, 1f,  0.3f, 0.3f, 0.3f,
        0f, 0f, 1f,  0.3f, 0.3f, 0.3f,
        0f, 0f, 0f,  0.3f, 0.3f, 0.3f,

        // Ściana Górna
        0f, 1f, 0f,  0.8f, 0.8f, 0.8f,
        1f, 1f, 0f,  0.8f, 0.8f, 0.8f,
        1f, 1f, 1f,  0.8f, 0.8f, 0.8f,
        1f, 1f, 1f,  0.8f, 0.8f, 0.8f,
        0f, 1f, 1f,  0.8f, 0.8f, 0.8f,
        0f, 1f, 0f,  0.8f, 0.8f, 0.8f
    ];

    public static float[] WireframeVertices => [
        // 8 rogów sześcianu (X, Y, Z)
        0.01f, 0.01f, 0.01f, // 0: Lewy-Dół-Tył
        1.01f, 0.01f, 0.01f, // 1: Prawy-Dół-Tył
        1.01f, 1.01f, 0.01f, // 2: Prawy-Góra-Tył
        0.01f, 1.01f, 0.01f, // 3: Lewy-Góra-Tył
        0.01f, 0.01f, 1.01f, // 4: Lewy-Dół-Przód
        1.01f, 0.01f, 1.01f, // 5: Prawy-Dół-Przód
        1.01f, 1.01f, 1.01f, // 6: Prawy-Góra-Przód
        0.01f, 1.01f, 1.01f, // 7: Lewy-Góra-Przód
    ];

    public static uint[] WireframeIndices => [
        0, 1, 1, 2, 2, 3, 3, 0, // Tylna ściana
        4, 5, 5, 6, 6, 7, 7, 4, // Przednia ściana
        0, 4, 1, 5, 2, 6, 3, 7  // Łączniki
    ];

}
