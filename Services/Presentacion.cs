namespace CalisTrack.Services;

public static class Presentacion
{
    public static string Iniciales(string? nombre) => string.Concat((nombre ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0])).ToUpperInvariant();
}
