using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

// Arma y envía la respuesta HTTP; incluye la compresión (requisito 8).
static class Respuesta
{
    public static void Enviar(NetworkStream red, int codigo, string tipo, byte[] contenido, bool gzip)
    {
        string estado = codigo switch
        {
            200 => "OK", 403 => "Forbidden", 404 => "Not Found", 405 => "Method Not Allowed", _ => "Error"
        };

        // HTTP exige "\r\n" al final de cada línea y una línea vacía antes del cuerpo
        string cabecera = $"HTTP/1.1 {codigo} {estado}\r\n" +
                          $"Content-Type: {tipo}\r\n" +
                          $"Content-Length: {contenido.Length}\r\n" +
                          (gzip ? "Content-Encoding: gzip\r\n" : "") +
                          "Connection: close\r\n\r\n";

        red.Write(Encoding.ASCII.GetBytes(cabecera));
        red.Write(contenido);
    }

    static readonly HashSet<string> TiposComprimibles = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/html", "text/css", "text/javascript", "text/plain", "application/json", "image/svg+xml"
    };

    public static bool EsComprimible(string tipo) =>
        TiposComprimibles.Contains(tipo.Split(';')[0].Trim());

    public static byte[] Comprimir(byte[] datos)
    {
        using var salida = new MemoryStream();
        using (var gzip = new GZipStream(salida, CompressionMode.Compress))
            gzip.Write(datos);
        return salida.ToArray();
    }
}
