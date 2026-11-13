# Servidor web simple (C# / .NET 8)

Proyecto final de Programación sobre Redes. El código está dividido en pocos archivos, uno por tema, y no usa paquetes externos.

## Ejecutar

```
dotnet run
```

Abrir http://localhost:8080

## Configuración (`config.json`)

```json
{
  "Puerto": 8080,
  "CarpetaWeb": "www"
}
```

## Dónde está cada requisito

| Requisito | Archivo | Qué mirar |
|---|---|---|
| 1. Concurrencia | `Program.cs` | `while (true)` + `Task.Run(() => Cliente.Atender(cliente))` |
| 2. index.html por defecto | `Archivos.cs` | `Buscar`: si la ruta termina en `/` se agrega `index.html` |
| 3 y 4. Carpeta y puerto | `Program.cs`, `Config.cs` | Se leen de `config.json` con `JsonSerializer` |
| 5. 404 personalizado | `Archivos.cs` | `Buscar`: devuelve `www/404.html` con código 404 |
| 6. GET y POST | `Cliente.cs` | `Atender`: GET sirve archivos; POST responde "Datos recibidos." y loguea el cuerpo |
| 7. Parámetros de consulta | `Cliente.cs` | `Atender`: se separa la URL en el `?` y los parámetros se loguean |
| 8. Compresión | `Respuesta.cs` | `Comprimir` con `GZipStream` si el navegador envía `Accept-Encoding: gzip` |
| 9. Log diario con IP | `Logger.cs` | `Escribir`: un archivo `logs/AAAA-MM-DD.log` por día, con `lock` |

Los logs quedan en `bin/Debug/net8.0/logs/`.
