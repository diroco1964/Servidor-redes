using System.Net;
using System.Net.Sockets;
using System.Text;

// Atiende UNA conexión: lee la solicitud HTTP, arma la respuesta y la envía.
static class Cliente
{
    public static void Atender(TcpClient cliente)
    {
        using (cliente)
        {
            string ip = ((IPEndPoint)cliente.Client.RemoteEndPoint!).Address.ToString();
            try
            {
                NetworkStream red = cliente.GetStream();
                red.ReadTimeout = 10000; // si el cliente no manda nada en 10 s, se corta
                var entrada = new BufferedStream(red);

                // 1) Línea de solicitud: "GET /index.html?x=1 HTTP/1.1"
                string? primeraLinea = LeerLinea(entrada);
                if (string.IsNullOrEmpty(primeraLinea)) return;
                string[] partes = primeraLinea.Split(' ');
                if (partes.Length < 3) return;
                string metodo = partes[0].ToUpper();
                string url = partes[1];

                // 2) Encabezados, hasta la línea vacía
                var encabezados = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string? linea;
                while (!string.IsNullOrEmpty(linea = LeerLinea(entrada)))
                {
                    int dosPuntos = linea.IndexOf(':');
                    if (dosPuntos > 0)
                        encabezados[linea[..dosPuntos].Trim()] = linea[(dosPuntos + 1)..].Trim();
                }

                // 3) Cuerpo (solo si hay Content-Length; se lee en BYTES)
                string cuerpo = "";
                if (encabezados.TryGetValue("Content-Length", out string? largoTexto)
                    && int.TryParse(largoTexto, out int largo) && largo > 0)
                {
                    byte[] buffer = new byte[largo];
                    int leidos = 0;
                    while (leidos < largo) // puede llegar en varios pedazos
                    {
                        int n = entrada.Read(buffer, leidos, largo - leidos);
                        if (n == 0) break;
                        leidos += n;
                    }
                    cuerpo = Encoding.UTF8.GetString(buffer, 0, leidos);
                }

                // --- Requisito 7: separar ruta y parámetros de consulta ---
                string ruta = url, parametros = "";
                int signo = url.IndexOf('?');
                if (signo >= 0)
                {
                    ruta = url[..signo];
                    parametros = Uri.UnescapeDataString(url[(signo + 1)..]);
                }
                ruta = Uri.UnescapeDataString(ruta); // "mi%20archivo.html" -> "mi archivo.html"

                // 4) Armar la respuesta según el método
                int codigo;
                string tipo;
                byte[] contenido;

                if (metodo == "GET")
                {
                    (codigo, tipo, contenido) = Archivos.Buscar(ruta);
                }
                else if (metodo == "POST")
                {
                    // --- Requisito 6: en POST solo se loguean los datos ---
                    (codigo, tipo, contenido) = (200, "text/plain", Encoding.UTF8.GetBytes("Datos recibidos."));
                }
                else
                {
                    (codigo, tipo, contenido) = (405, "text/plain", Encoding.UTF8.GetBytes("Método no permitido."));
                }

                // --- Requisito 8: compresión GZip si el navegador la acepta ---
                bool usarGzip = encabezados.TryGetValue("Accept-Encoding", out string? enc) && enc.Contains("gzip");
                if (usarGzip) contenido = Respuesta.Comprimir(contenido);

                Respuesta.Enviar(red, codigo, tipo, contenido, usarGzip);

                // --- Requisito 9: log diario con IP ---
                string registro = $"{DateTime.Now:HH:mm:ss} | {ip} | {metodo} {ruta} | {codigo}";
                if (parametros != "") registro += $" | parámetros: {parametros}";
                if (metodo == "POST") registro += $" | datos: {cuerpo}";
                Logger.Escribir(registro);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ip}] Error: {ex.Message}");
            }
        }
    }

    // Lee una línea terminada en "\r\n" byte a byte (así no se mezcla con el cuerpo)
    static string? LeerLinea(Stream entrada)
    {
        var bytes = new List<byte>();
        int b;
        while ((b = entrada.ReadByte()) != -1)
        {
            if (b == '\n') break;
            if (b != '\r') bytes.Add((byte)b);
        }
        if (b == -1 && bytes.Count == 0) return null;
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}
