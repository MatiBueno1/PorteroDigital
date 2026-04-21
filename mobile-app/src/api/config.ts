/**
 * CONFIGURACIÓN DE CONEXIÓN
 * -------------------------
 * Cambia 'USE_LOCAL' a false para usar la URL de tu túnel (ngrok)
 * y que funcione fuera de tu WiFi.
 */
const USE_TUNNEL = false; 

const TUNNEL_URL = 'https://tu-url-de-ngrok.ngrok-free.app'; // <--- PEGA TU URL DE NGROK ACÁ
const LOCAL_IP = '192.168.3.2'; // Tu IP local actual

export const API_BASE_URL = USE_TUNNEL 
    ? TUNNEL_URL 
    : `http://${LOCAL_IP}:5166`;

console.log('Conectando a:', API_BASE_URL);
