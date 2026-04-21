# 🏛️ Master Context: Portero Digital

Este documento es la fuente de verdad absoluta del proyecto. Contiene la arquitectura, el estado técnico actual y la hoja de ruta para que cualquier desarrollador (IA o Humano) pueda continuar el trabajo desde cero.

---

## 1. Visión General
**Portero Digital** es un sistema de seguridad residencial.
- **Flujo:** Visitantes escanean un QR -> Notificación push al celular del residente -> El residente ve la cámara en vivo y autoriza el acceso.
- **Estética:** "Brutalismo Moderno". Alto contraste, tipografía pesada, bordes marcados, minimalismo.

---

## 2. Estado Técnico Actual (Abril 2026)

### Cambios Recientes Críticos:
- **Renombrado:** El proyecto se consolidó en la carpeta raíz `PorteroDigital`.
- **Seguridad (Zero Leaks):** Se eliminaron todas las credenciales del código (`appsettings.json`). Ahora se usa un sistema de variables de entorno con un archivo `.env` (ignorado por Git).
- **Git:** Repositorio inicializado en la raíz. Archivos limpiados y bindeados al remoto `https://github.com/MatiBueno1/PorteroDigital.git`.

### Stack Tecnológico:
- **Backend:** .NET 10.0 (Clean Architecture).
    - **Persistencia:** EF Core (Soporte dual: Sqlite para dev, Postgres para nube).
    - **Streaming:** FFmpeg actuando como proxy de ultra-baja latencia para RTSP.
    - **Real-time:** SignalR para eventos instantáneos.
- **Mobile:** Expo / React Native (SDK 54+).
    - **Estilo:** NativeWind.
    - **Router:** Expo Router.
- **Frontend:** HTML + Alpine.js + Tailwind CSS.

---

## 3. Configuración de Seguridad e Inyección (.env)
Para mantener el proyecto seguro, el backend utiliza un cargador manual en `Program.cs` que mapea un archivo `.env` externo a la configuración de .NET:

**Variables Requeridas:**
- `CAMERA_RTSP_URL`: Enlace completo a la cámara con credenciales.
- `CAMERA_LIGHT_ON_URL` (y otras): Enlaces CGI para control de hardware.
- `JWT_SIGNING_KEY`: Clave secreta para firmar tokens de residentes.

---

## 4. Arquitectura de Archivos
- `/src`: Backend .NET (Domain, Application, Infrastructure, WebAPI).
- `/mobile-app`: Código fuente de la App de residentes.
- `/frontend`: Páginas de visitantes y directorios.
- `/.env`: Secretos locales (NO SUBIR).
- `/.env.example`: Plantilla de secretos.

---

## 5. Hoja de Ruta Inmediata (Roadmap)

### Fase 1: Despliegue en la Nube
1. **GitHub:** Subir el estado actual (`git push`).
2. **Render:** Crear un Web Service basado en el `Dockerfile` existente.
3. **Database:** Migrar de SQLite a PostgreSQL en Render.
4. **Secrets:** Configurar las Environment Variables en el dashboard de Render.

### Fase 2: Build y Distribución Mobile
1. **URL de Producción:** Actualizar `mobile-app/src/api/config.ts` con la URL de Render.
2. **EAS Build:** Generar el archivo `.apk` definitivo.
3. **Multi-Residente:** Asegurar que cada residente pueda loguearse y recibir notificaciones exclusivas de su casa.

---

## 6. Instrucciones para la IA
Para ayudar en el despliegue:
- El backend detecta automáticamente si está en Render vía la variable `PORT`.
- El streaming de cámara requiere que la IP de la cámara sea accesible desde el servidor (vía VPN o IP pública) o se verá solo localmente.
- Asegúrate de mapear `Database:Provider=Postgres` en producción.
la App Móvil.
- [ ] Empaquetado definitivo para producción.