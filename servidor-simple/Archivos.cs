using System.Text;

// Busca el archivo pedido dentro de la carpeta web (requisitos 2 y 5).
static class Archivos
{
    public static string CarpetaWeb = "";

    public static (int, string, byte[]) Buscar(string ruta)
    {
        // Requisito 2: si la URL no nombra un archivo, se sirve index.html
        if (ruta.EndsWith("/")) ruta += "index.html";

        string archivo = Path.GetFullPath(Path.Combine(CarpetaWeb, ruta.TrimStart('/')));

        // Seguridad: no permitir salir de la carpeta web con "../"
        if (!archivo.StartsWith(CarpetaWeb + Path.DirectorySeparatorChar))
            return (403, "text/plain", Encoding.UTF8.GetBytes("Acceso denegado."));

        if (File.Exists(archivo))
            return (200, TipoDeContenido(archivo), File.ReadAllBytes(archivo));

        // Requisito 5: 404 con documento personalizado
        string pagina404 = Path.Combine(CarpetaWeb, "404.html");
        byte[] cuerpo404 = File.Exists(pagina404)
            ? File.ReadAllBytes(pagina404)
            : Encoding.UTF8.GetBytes("<h1>404 - Archivo no encontrado</h1>");
        return (404, "text/html", cuerpo404);
    }

    static string TipoDeContenido(string archivo) => Path.GetExtension(archivo).ToLower() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css" => "text/css; charset=utf-8",
        ".js" => "text/javascript; charset=utf-8",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };
}
