# ☁️ Guía: Despliegue en Render (Opción A)

Ahora que el código está preparado para la nube, seguí estos pasos para tener tu backend funcionando 24/7 sin depender de tu PC.

## 1. Subir el Código a GitHub
Para desplegar en Render, primero tenés que tener este proyecto en un repositorio de GitHub (público o privado).

## 2. Crear la Base de Datos en Render
1. Entrá a [dashboard.render.com](https://dashboard.render.com) e iniciá sesión.
2. Clic en **New** -> **PostgreSQL**.
3. Poné nombre `portero-database`.
4. En **Instance Type**, elegí el plan **Free**.
5. Clic en **Create Database**.
6. Una vez creada, buscá donde dice **Internal Database URL** o **External Database URL**. La vamos a necesitar.

## 3. Crear el Servicio Web
1. Clic en **New** -> **Web Service**.
2. Conectá tu repositorio de GitHub.
3. **Name:** `portero-api`.
4. **Environment:** `Docker` (Render detectará automáticamente el `Dockerfile` que creamos).
5. **Instance Type:** `Free`.
6. Clic en **Advanced** y agregá estas **Environment Variables**:
   * `Database:Provider`: `Postgres`
   * `DATABASE_URL`: (Acá pegás la URL de la base de datos que creaste en el paso anterior)
   * `Jwt:SigningKey`: `Una-Clave-Súper-Secreta-De-Mas-De-32-Caracteres`
   * `PushNotifications:Enabled`: `true` (Si querés que sigan llegando notificaciones)

## 4. Actualizar la App y el Directorio
Una vez que Render termine de compilar (puede tardar 5-8 min), te va a dar una URL como `https://portero-api.onrender.com`.

### En la App Móvil (`config.ts`):
```typescript
const USE_TUNNEL = true;
const TUNNEL_URL = 'https://portero-api.onrender.com';
```

### En el Directorio (`visitante.html`):
```javascript
const USE_TUNNEL = true;
const TUNNEL_URL = 'https://portero-api.onrender.com';
```

---
> [!WARNING]
> **Recordatorio:** Como el servidor ahora está en la nube, no podrá ver tu cámara local. La App te mostrará un mensaje indicando que el video solo está disponible si usás la IP local.
