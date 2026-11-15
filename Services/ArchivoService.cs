using CalisTrack.Models;

namespace CalisTrack.Services;

public class ArchivoService
{
    private readonly string carpeta;
    public ArchivoService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var raiz = configuration["Storage:Root"] ?? Path.Combine(environment.ContentRootPath, "App_Data");
        carpeta = Path.Combine(raiz, "archivos");
    }
    public string Ruta(ArchivoAdjunto archivo) => Path.Combine(carpeta, archivo.Ruta);
    public async Task<ArchivoAdjunto?> Guardar(IFormFile? archivo, int usuarioId)
    {
        if (archivo == null || archivo.Length == 0) return null;
        if (archivo.Length > 20 * 1024 * 1024) throw new InvalidOperationException("El archivo no puede superar los 20 MB.");
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var bytes = new byte[16];
        await using var entrada = archivo.OpenReadStream();
        var leidos = await entrada.ReadAsync(bytes);
        var tipo = extension switch
        {
            ".png" when leidos >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) => "image/png",
            ".jpg" or ".jpeg" when leidos >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255 => "image/jpeg",
            ".webp" when leidos >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP" => "image/webp",
            ".pdf" when leidos >= 5 && System.Text.Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-" => "application/pdf",
            ".mp4" when leidos >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 4, 4) == "ftyp" => "video/mp4",
            ".webm" when leidos >= 4 && bytes[0] == 26 && bytes[1] == 69 && bytes[2] == 223 && bytes[3] == 163 => "video/webm",
            _ => throw new InvalidOperationException("Formato no admitido. Usá JPG, PNG, WebP, PDF, MP4 o WebM válidos.")
        };
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + extension;
        await using var salida = File.Create(Path.Combine(carpeta, nombre));
        await salida.WriteAsync(bytes.AsMemory(0, leidos));
        await entrada.CopyToAsync(salida);
        return new ArchivoAdjunto { Nombre = Path.GetFileName(archivo.FileName)[..Math.Min(Path.GetFileName(archivo.FileName).Length, 200)], Tipo = tipo, Ruta = nombre, PropietarioId = usuarioId, Tamano = archivo.Length };
    }
}
