using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PorteroDigital.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CameraController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CameraController> _logger;

    public CameraController(IConfiguration configuration, ILogger<CameraController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("stream")]
    // [Authorize] - Nota: Desactivado para esta prueba inicial del Proxy HTML. 
    // Si se activa, el <img src=""> en HTML requiere pasar el Token, 
    // lo cual no es nativo en img y requiere un workaround en frontend (fetch objectURL).
    public async Task GetStream(CancellationToken cancellationToken)
    {
        var rtspUrl = _configuration["Camera:RtspUrl"];
        if (string.IsNullOrEmpty(rtspUrl))
        {
            Response.StatusCode = 404;
            return;
        }

        Response.ContentType = "multipart/x-mixed-replace; boundary=ffmpeg";
        
        // Configuramos los argumentos de ffmpeg
        // Optimizaciones EXTREMAS de ultra-baja latencia (Zero Latency):
        // 1. -rtsp_transport udp: Cambiamos de TCP a UDP. Ignora paquetes perdidos para no trabarse y seguir en tiempo real.
        // 2. -analyzeduration 0 -probesize 32: Obliga a FFmpeg a NO leer datos por adelantado para analizar el formato.
        // 3. +discardcorrupt: si un frame llega mal por el WiFi, lo descarta rápido en lugar de intentar arreglarlo.
        var arguments = $"-fflags nobuffer+genpts+discardcorrupt -flags low_delay -max_delay 0 -analyzeduration 0 -probesize 32 -rtsp_transport udp -i \"{rtspUrl}\" -f mpjpeg -q:v 6 -fpsprobesize 0 -deadline realtime -";

        // Detección automática del ejecutable local para evitar problemas de PATH en Windows
        var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
        if (!System.IO.File.Exists(ffmpegPath))
        {
            ffmpegPath = "ffmpeg"; // Intentar usar el PATH global si el local no existe
        }

        var processInfo = new ProcessStartInfo(ffmpegPath, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogError("No se pudo iniciar el proceso de FFmpeg. Verifica si ffmpeg está instalado y en el PATH.");
                Response.StatusCode = 500;
                return;
            }

            // Capturar la salida de error (logs de ffmpeg)
            _ = Task.Run(async () =>
            {
                using var reader = process.StandardError;
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line != null)
                    {
                        _logger.LogInformation("[FFMPEG] {Line}", line);
                    }
                }
            }, cancellationToken);

            byte[] buffer = new byte[8192]; // 8KB buffer para leer la imagen en tránsito
            int bytesRead;

            // Stream de salida HTTP
            var responseStream = Response.Body;

            // Leer desde stdout de ffmpeg y pasarlo directamente a Response HTTP
            while (!cancellationToken.IsCancellationRequested && !process.HasExited)
            {
                bytesRead = await process.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) break;

                await responseStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                await responseStream.FlushAsync(cancellationToken);
            }

            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el pipeline de la cámara");
            if (!Response.HasStarted)
            {
                Response.StatusCode = 500;
            }
        }
    }
}
