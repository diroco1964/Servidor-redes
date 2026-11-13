// Log diario: un archivo logs/AAAA-MM-DD.log por día (requisito 9).
static class Logger
{
    public static string CarpetaLogs = "";
    static readonly object candado = new object(); // para que dos hilos no escriban a la vez

    public static void Escribir(string texto)
    {
        texto = texto.Replace("\r", " ").Replace("\n", " "); // una solicitud = una línea
        Console.WriteLine(texto);
        string archivoDelDia = Path.Combine(CarpetaLogs, $"{DateTime.Now:yyyy-MM-dd}.log");
        lock (candado)
        {
            File.AppendAllText(archivoDelDia, texto + Environment.NewLine);
        }
    }
}
