/**
 * CONFIGURACIÓN DE CONEXIÓN
 * -------------------------
 * Cambia 'USE_LOCAL' a false para usar la URL de tu túnel (ngrok)
 * y que funcione fuera de tu WiFi.
 */
const USE_TUNNEL = true; 

const TUNNEL_URL = 'https://porterodigital.onrender.com'; // <--- URL DE PRODUCCIÓN EN RENDER
const LOCAL_IP = '192.168.3.2'; // Tu IP local actual para testing

export const API_BASE_URL = USE_TUNNEL 
    ? TUNNEL_URL 
    : `http://${LOCAL_IP}:5166`;

console.log('Conectando a:', API_BASE_URL);
