using System;

namespace IdleBuilder.World
{
    [Flags]
    public enum RoadDirections
    {
        None  = 0,       // Brak połączeń (pojedyncza kropka)
        North = 1 << 0,  // 1
        East  = 1 << 1,  // 2
        South = 1 << 2,  // 4
        West  = 1 << 3   // 8
    }

    // Kombinacje masek dla 16 ikonek:
    // 0  (None)      -> Pojedynczy fragment drogi
    // 1  (N)         -> Ślepy zaułek Północ
    // 2  (E)         -> Ślepy zaułek Wschód
    // 3  (N+E)       -> Zakręt Północ-Wschód
    // 4  (S)         -> Ślepy zaułek Południe
    // 5  (N+S)       -> Prosta Pionowa
    // 6  (E+S)       -> Zakręt Wschód-Południe
    // 7  (N+E+S)     -> Skrzyżowanie T (Północ-Wschód-Południe)
    // 8  (W)         -> Ślepy zaułek Zachód
    // 9  (N+W)       -> Zakręt Północ-Zachód
    // 10 (E+W)       -> Prosta Pozioma
    // 11 (N+E+W)     -> Skrzyżowanie T (Północ-Wschód-Zachód)
    // 12 (S+W)       -> Zakręt Południe-Zachód
    // 13 (N+S+W)     -> Skrzyżowanie T (Północ-Południe-Zachód)
    // 14 (E+S+W)     -> Skrzyżowanie T (Wschód-Południe-Zachód)
    // 15 (N+E+S+W)   -> Skrzyżowanie 4-kierunkowe (Krzyżówka)
}