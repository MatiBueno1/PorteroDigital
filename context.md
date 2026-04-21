# 🏛️ Master Context: Portero Digital Rosario

Este documento es el cerebro central del proyecto. Contiene la arquitectura, lógica de negocio y estado técnico actual para que cualquier desarrollador (IA o Humano) pueda continuar el trabajo con precisión total.

---

## 1. Visión General
**Portero Digital** es un sistema de seguridad residencial de vanguardia. Permite que visitantes en una puerta física escaneen un código QR para anunciarse, y que los residentes reciban una notificación nativa en sus teléfonos para ver la cámara en vivo y decidir si permiten el acceso.

**Estética Visual:** "Brutalismo Moderno". Alto contraste (Negro/Blanco/Gris), tipografías pesadas (`Black`/`900`), sin redondeces excesivas, bordes marcados y minimalismo absoluto.

---

## 2. Stack Tecnológico

### Backend (Core API)
- **Framework:** .NET 10.0 (C#)
- **Arquitectura:** Clean Architecture (Domain, Application, Infrastructure, WebAPI).
- **Persistencia:** Entity Framework Core con SQLite (Desarrollo) y soporte para SQL Server.
- **Comunicación Real-Time:** SignalR Hubs para mensajería instantánea.
- **Seguridad:** JWT (JSON Web Tokens) con Claims por `HouseId` y `ResidentId`.
- **Procesamiento de Video:** FFmpeg (integrado vía Process en C#) actuando como Proxy RTSP a MJPEG.

### Frontend Web (Visitante / Legacy Inquilino)
- **JS:** Alpine.js (ligero y reactivo).
- **CSS:** Tailwind CSS (Estilo Brutalista).
- **SignalR Client:** Integración nativa para recibir notificaciones en navegador.

### Mobile App (Residente Definitive)
- **Framework:** React Native con Expo (SDK 54+).
- **Estilo:** NativeWind (Tailwind nativo) + CSS Global.
- **Navegación:** Expo Router (File-based routing).
- **Persistencia:** Expo SecureStore para tokens y sesión.
- **Notificaciones:** Expo Notifications (integrado con el backend .NET).

---

## 3. Arquitectura de Archivos y Responsabilidades

### Backend `src/`
- **`PorteroDigital.Domain`**: Entidades puras (`Resident`, `House`, `VisitorLog`, `CameraConfiguration`). Lógica agnóstica de persistencia.
- **`PorteroDigital.Application`**: Contratos (`Abstractions`), Modelos (`DTOs`) y lógica de servicios. Define CÓMO se interactúa con el sistema.
- **`PorteroDigital.Infrastructure`**: Implementación de servicios, EF Core DbContext (`PorteroDigitalDbContext`), Seguridad (JWT), y Notificaciones Push (Expo API).
- **`PorteroDigital.WebAPI`**: Controladores REST, SignalR Hubs, y la configuración de `Program.cs`. Contiene el ejecutable `ffmpeg.exe` para el streaming.

### Mobile `mobile-app/`
- **`app/`**: Rutas de la aplicación.
    - `_layout.tsx`: Configuración global, temas y NativeWind.
    - `index.tsx`: Panel principal del residente (historial, cámara, SignalR).
    - `login.tsx`: Pantalla de acceso con persistencia de sesión.
- **`src/api/`**: Cliente Axios configurado con la IP local (`192.168.3.2`).

---

## 4. Lógica de Funcionalidades Críticas

### Transmisión de Cámara (Zero Latency)
- **Ruta:** `GET /api/camera/stream`
- **Lógica:** El servidor levanta un proceso de `ffmpeg.exe`. Toma una URL `rtsp://` local, aplica parámetros de ultra-baja latencia (`-fflags nobuffer`, `-rtsp_transport udp`, `-analyzeduration 0`) y escupe un stream `multipart/x-mixed-replace` directamente al Response.
- **Consumo:** En Web y Mobile se consume simplemente apuntando un tag `<img src="...">` a este endpoint del servidor.

### Notificaciones Push (El "Timbre")
1. **Registro:** Al hacer login, la App Móvil solicita un `ExpoPushToken` y lo guarda en la base de datos vinculado al `ResidentDevice`.
2. **Disparo:** El visitante envía un `POST` a `/api/visits/notify/{houseId}`.
3. **Distribución:**
   - Envía mensaje instantáneo vía **SignalR** a todos los clientes web/app conectados al grupo de la casa.
   - Dispara una llamada al **ResidentPushNotificationService** que envía un POST a la API de Expo (`https://exp.host/--/api/v2/push/send`) para despertar los celulares físicos aunque estén bloqueados.

---

## 5. Configuración de Desarrollo Actual

- **Base de Datos:** SQLite (`portero-digital.dev.db`). Pre-cargada con casas 01 a 12 y credenciales `casaXX@portero.local` / `Portero123!`.
- **IP Local Server:** `192.168.3.2:5166`.
- **Dependencias Externas:** Requiere `ffmpeg.exe` en la raíz de la WebAPI para el streaming de cámara.

---

## 6. Estado de Avance
- [x] Backend Clean Architecture base.
- [x] Streaming de Cámara RTSP -> MJPEG funcional con <1s delay.
- [x] Sistema de Notificaciones Real-time vía SignalR.
- [x] Interfaz de Visitante Web lista para escaneo QR.
- [x] App Móvil (Expo) inicializada y configurada con diseño Brutalista.
- [x] Integración de Backend con Expo Push API.
- [ ] Finalización de registro de Tokens Push desde la App Móvil.
- [ ] Empaquetado definitivo para producción.