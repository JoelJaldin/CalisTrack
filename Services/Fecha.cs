namespace CalisTrack.Services;

public static class Fecha
{
    public static DateTime Hoy => DateTime.UtcNow.AddHours(-3).Date;
    public static DateTime Local(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(-3);
}
