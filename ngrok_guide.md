# 🌐 Guía: Cómo usar Portería Digital fuera de tu casa (ngrok)

Como el servidor corre en tu computadora local, necesitás un "túnel" para que el celular (con 4G) o el visitante puedan llegar a él.

## 1. Instalar ngrok
Si no lo tenés, descargalo en [ngrok.com](https://ngrok.com/download).

## 2. Iniciar el túnel
Con el servidor `dotnet run` ya iniciado, abrí una terminal nueva y ejecutá:
```powershell
ngrok http 5166
```

## 3. Obtener la URL
Ngrok te va a devolver una URL parecida a esta:
`https://a1b2-c3d4.ngrok-free.app`

## 4. Configurar la App y la Web
### En la App (Móvil):
Abrí el archivo `mobile-app/src/api/config.ts` y cambiá:
```typescript
const USE_TUNNEL = true; // <--- Ponelo en TRUE
const TUNNEL_URL = 'https://tu-url-de-ngrok.ngrok-free.app'; // <--- Pegá tu URL acá
```

### En el Directorio (Visitante):
Abrí `frontend/visitante.html` y al inicio del script cambiá:
```javascript
const USE_TUNNEL = true; // <--- Ponelo en TRUE
const TUNNEL_URL = 'https://tu-url.ngrok-free.app'; // <--- Pegá tu URL acá
```

---
> [!IMPORTANT]
> **Cámaras:** Si usás ngrok, la cámara stream funcionará pero puede tener un poco más de latencia que en WiFi local.
