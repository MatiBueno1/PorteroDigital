using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PorteroDigital.Application.Abstractions.Services;

namespace PorteroDigital.Infrastructure.Services;

public sealed class CameraControlService(
    HttpClient httpClient, 
    IConfiguration configuration,
    ILogger<CameraControlService> logger) : ICameraControlService
{
    public async Task<bool> SetLightStatusAsync(bool on, CancellationToken cancellationToken)
    {
        var urlKey = on ? "Camera:LightOnUrl" : "Camera:LightOffUrl";
        var url = configuration[urlKey];

        if (string.IsNullOrEmpty(url))
        {
            logger.LogWarning("URL de control de luz no configurada: {Key}", urlKey);
            return false;
        }

        return await SendCgiCommandAsync(url, "Luz", cancellationToken);
    }

    public async Task<bool> SetNightVisionAsync(string mode, CancellationToken cancellationToken)
    {
        var url = mode.ToLower() switch
        {
            "on" => configuration["Camera:NightVisionOnUrl"],
            "off" => configuration["Camera:NightVisionOffUrl"],
            _ => configuration["Camera:NightVisionAutoUrl"]
        };

        if (string.IsNullOrEmpty(url))
        {
            logger.LogWarning("URL de visión nocturna ({Mode}) no configurada", mode);
            return false;
        }

        return await SendCgiCommandAsync(url, $"Visión Nocturna ({mode})", cancellationToken);
    }

    private async Task<bool> SendCgiCommandAsync(string url, string label, CancellationToken cancellationToken)
    {
        try
        {
            // Lógica dinámica para corregir la IP si está usando el ejemplo
            if (url.Contains("192.168.3.14"))
            {
                var rtspUrl = configuration["Camera:RtspUrl"];
                if (!string.IsNullOrEmpty(rtspUrl))
                {
                    // Extraer IP de algo como rtsp://admin:pass@192.168.1.50:554/...
                    var match = System.Text.RegularExpressions.Regex.Match(rtspUrl, @"@([\d\.]+)[:/]");
                    if (!match.Success) 
                    {
                         // Intento 2: rtsp://192.168.1.50/...
                         match = System.Text.RegularExpressions.Regex.Match(rtspUrl, @"//([\d\.]+)[:/]");
                    }

                    if (match.Success)
                    {
                        var realIp = match.Groups[1].Value;
                        url = url.Replace("192.168.3.14", realIp);
                        logger.LogInformation("IP detectada automáticamente de RTSP: {Ip}", realIp);
                    }
                }
            }

            logger.LogInformation("Enviando comando CGI a cámara: {Label} -> {Url}", label, url);
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Comando {Label} ejecutado con éxito", label);
                return true;
            }

            logger.LogWarning("La cámara respondió con error al comando {Label}: {Status}", label, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar comando CGI {Label}", label);
            return false;
        }
    }
}
