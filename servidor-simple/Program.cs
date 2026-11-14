using System.Net;
using System.Net.Sockets;
using System.Text.Json;

// ============================================================
//  Servidor web simple - Programación sobre Redes
//  Punto de entrada: lee la configuración y acepta clientes.
// ============================================================

// --- Requisitos 3 y 4: puerto y carpeta desde config.json ---
string baseDir = AppContext.BaseDirectory;
var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(Path.Combine(baseDir, "config.json")))!;

Archivos.CarpetaWeb = Path.GetFullPath(Path.Combine(baseDir, config.CarpetaWeb));
Logger.CarpetaLogs = Path.Combine(baseDir, "logs");
Directory.CreateDirectory(Logger.CarpetaLogs);

TcpListener servidor = new TcpListener(IPAddress.Any, config.Puerto);
servidor.Start();
Console.WriteLine($"Servidor escuchando en http://localhost:{config.Puerto}");
Console.WriteLine($"Sirviendo archivos desde: {Archivos.CarpetaWeb}");

// Ctrl+C no mata el proceso de golpe: cancela la espera de conexiones y cierra el listener.
using var cancelacion = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancelacion.Cancel();
};

// --- Requisito 1: concurrencia ---
// Cada cliente se atiende en su propia tarea; el bucle vuelve enseguida a aceptar otro.
try
{
    while (true)
    {
        TcpClient cliente = await servidor.AcceptTcpClientAsync(cancelacion.Token);
        _ = Task.Run(() => Cliente.Atender(cliente));
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cerrando el servidor...");
}
finally
{
    servidor.Stop();
}
