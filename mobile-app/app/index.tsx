import React, { useEffect, useState } from 'react';
import { View, Text, TouchableOpacity, ScrollView, SafeAreaView, ActivityIndicator, Image, StyleSheet, Dimensions, ImageBackground } from 'react-native';
import { useRouter } from 'expo-router';
import * as SecureStore from 'expo-secure-store';
import apiClient, { API_BASE_URL } from '../src/api/apiClient';
import * as SignalR from '@microsoft/signalr';
import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import Constants from 'expo-constants';
import { WebView } from 'react-native-webview';
import { Modal, TextInput, RefreshControl } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

// Textura de grano (Rich Brutalism)
const GRAIN_IMAGE = require('../assets/images/grain.png');

Notifications.setNotificationHandler({
    handleNotification: async () => ({
        shouldShowAlert: true,
        shouldPlaySound: true,
        shouldSetBadge: false,
    }),
});

export default function ResidentPanel() {
    const router = useRouter();
    const [authReady, setAuthReady] = useState(false);
    const [token, setToken] = useState<string | null>(null);
    const [houseLabel, setHouseLabel] = useState('');
    const [houseId, setHouseId] = useState('');
    const [connectionLabel, setConnectionLabel] = useState('Conectando...');
    const [notification, setNotification] = useState<any>(null);
    const [verCamara, setVerCamara] = useState(false);
    const [history, setHistory] = useState<any[]>([]);
    const [refreshing, setRefreshing] = useState(false);
    const [isFullscreen, setIsFullscreen] = useState(false);
    
    // Estados para Perfil
    const [settingsVisible, setSettingsVisible] = useState(false);
    const [newName, setNewName] = useState('');
    const [newEmail, setNewEmail] = useState('');
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [savingSettings, setSavingSettings] = useState(false);

    useEffect(() => {
        const checkAuth = async () => {
            const savedToken = await SecureStore.getItemAsync('accessToken');
            const savedHouseId = await SecureStore.getItemAsync('houseId');
            const savedResidentName = await SecureStore.getItemAsync('residentName');

            if (!savedToken || !savedHouseId) {
                router.replace('/login');
            } else {
                setToken(savedToken);
                setHouseId(savedHouseId);
                setHouseLabel(savedResidentName ? `${savedResidentName}` : 'Mi Residencia');
                setAuthReady(true);
            }
        };
        checkAuth();
    }, []);

    useEffect(() => {
        if (authReady && token && houseId) {
            connectSignalR();
            registerForPushNotificationsAsync();
            fetchHistory();

            // Auto-refresco del historial cada 30 segundos
            const interval = setInterval(fetchHistory, 30000);
            return () => clearInterval(interval);
        }
    }, [authReady]);

    const fetchHistory = async () => {
        try {
            const response = await apiClient.get(`/api/mobile/visits`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            setHistory(response.data);
        } catch (error) {
            console.error('Error al cargar historial:', error);
        }
    };

    const onRefresh = async () => {
        setRefreshing(true);
        await fetchHistory();
        setRefreshing(false);
    };

    const handleDecision = async (visitorLogId: string, status: number) => {
        try {
            await apiClient.post(`/api/mobile/visits/${visitorLogId}/decision`, {
                status,
                decisionDetail: status === 2 ? 'Aceptado desde App' : 'Rechazado desde App'
            }, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            setNotification(null);
            fetchHistory();
        } catch (error) {
            console.error('Error al enviar decisión:', error);
            alert('No se pudo enviar la respuesta.');
        }
    };

    const saveSettings = async () => {
        setSavingSettings(true);
        try {
            // Actualizar Nombre
            if (newName.trim()) {
                await apiClient.patch('/api/mobile/me/name', { name: newName }, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                await SecureStore.setItemAsync('residentName', newName);
                setHouseLabel(`${newName}`);
            }

            // Actualizar Credenciales (Email/Password)
            if (newEmail.trim() || newPassword.trim()) {
                if (!currentPassword) {
                    alert('Debes ingresar tu contraseña actual para cambiar el email o la clave.');
                    setSavingSettings(false);
                    return;
                }
                await apiClient.post('/api/mobile/me/credentials', {
                    currentPassword,
                    newEmail: newEmail.trim() || null,
                    newPassword: newPassword.trim() || null
                }, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
            }

            setSettingsVisible(false);
            setCurrentPassword('');
            setNewPassword('');
            alert('Perfil actualizado con éxito');
        } catch (error) {
            console.error('Error guardando perfil:', error);
            alert('Error al actualizar perfil. Verifica tu contraseña actual.');
        } finally {
            setSavingSettings(false);
        }
    };

    const registerForPushNotificationsAsync = async () => {
        if (!Device.isDevice) return;
        const { status: existingStatus } = await Notifications.getPermissionsAsync();
        let finalStatus = existingStatus;
        if (existingStatus !== 'granted') {
            const { status } = await Notifications.requestPermissionsAsync();
            finalStatus = status;
        }
        if (finalStatus !== 'granted') return;

        try {
            const projectId = Constants.expoConfig?.extra?.eas?.projectId;
            if (!projectId) {
                console.warn('Falta Project ID en app.json');
                return;
            }

            const tokenResponse = await Notifications.getExpoPushTokenAsync({ projectId });
            const pushToken = tokenResponse.data;
            console.log('Push Token Generado:', pushToken);

            await apiClient.post('/api/mobile/devices', {
                deviceName: Device.deviceName || 'Smartphone',
                platform: 1, 
                pushToken: pushToken,
                notificationSound: 'default'
            }, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
        } catch (error) {
            console.error('Error al registrar Push Token:', error);
        }
    };

    const connectSignalR = async () => {
        try {
            const connection = new SignalR.HubConnectionBuilder()
                .withUrl(`${API_BASE_URL}/hubs/resident-notifications`, {
                    accessTokenFactory: () => token!
                })
                .withAutomaticReconnect()
                .build();

            connection.on('ReceiveNotification', (data) => {
                console.log('SignalR Notification Data:', data);
                const mappedData = {
                    id: data.id || data.Id,
                    visitorName: data.visitorName || data.VisitorName,
                    reason: data.reason || data.Reason,
                    status: data.status || data.Status,
                    timestamp: data.timestamp || data.Timestamp
                };
                setNotification(mappedData);
                setVerCamara(true);
            });

            await connection.start();
            await connection.invoke('JoinHouseGroup', houseId);
            setConnectionLabel(`En línea`);
        } catch (err) {
            console.error('SignalR Error:', err);
            setConnectionLabel('Sin conexión');
        }
    };

    useEffect(() => {
        const foregroundSubscription = Notifications.addNotificationReceivedListener(notification => {
            const data = notification.request.content.data;
            if (data && (data.visitorName || data.VisitorName)) {
                const mappedData = {
                    id: data.id || data.Id,
                    visitorName: data.visitorName || data.VisitorName,
                    reason: data.reason || data.Reason,
                    timestamp: data.timestamp || data.Timestamp
                };
                setNotification(mappedData);
                setVerCamara(true);
            }
        });

        const responseSubscription = Notifications.addNotificationResponseReceivedListener(response => {
            const data = response.notification.request.content.data;
            if (data && (data.visitorName || data.VisitorName)) {
                const mappedData = {
                    id: data.id || data.Id,
                    visitorName: data.visitorName || data.VisitorName,
                    reason: data.reason || data.Reason,
                    timestamp: data.timestamp || data.Timestamp
                };
                setNotification(mappedData);
                setVerCamara(true);
            }
        });

        return () => {
            foregroundSubscription.remove();
            responseSubscription.remove();
        };
    }, []);

    const logout = async () => {
        await SecureStore.deleteItemAsync('accessToken');
        await SecureStore.deleteItemAsync('houseId');
        router.replace('/login');
    };

    const [cameraTimestamp, setCameraTimestamp] = useState(Date.now());

    useEffect(() => {
        if (verCamara || isFullscreen) {
            setCameraTimestamp(Date.now());
        }
    }, [verCamara, isFullscreen]);

    if (!authReady) {
        return (
            <View style={styles.loadingContainer}>
                <ActivityIndicator color="black" />
            </View>
        );
    }

    const streamUrl = `${API_BASE_URL}/api/camera/stream?t=${cameraTimestamp}`;

    const formatTimestamp = (isoString: string) => {
        try {
            const date = new Date(isoString);
            return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
        } catch {
            return isoString;
        }
    };

    return (
        <ImageBackground source={GRAIN_IMAGE} style={styles.container} imageStyle={{ opacity: 0.05 }}>
            <SafeAreaView style={{ flex: 1 }}>
                <View style={styles.topBar}>
                    <View>
                        <Text style={styles.topBarTitle}>{houseLabel}</Text>
                        <View style={styles.statusIndicator}>
                            <View style={[styles.dot, { backgroundColor: connectionLabel === 'En línea' ? '#0F0' : '#F00' }]} />
                            <Text style={styles.topBarStatus}>{connectionLabel}</Text>
                        </View>
                    </View>
                    <View style={{ flexDirection: 'row', gap: 12, alignItems: 'center' }}>
                        <TouchableOpacity onPress={() => setSettingsVisible(true)} style={styles.iconBtn}>
                            <Ionicons name="person-circle-outline" size={28} color="black" />
                        </TouchableOpacity>
                        <TouchableOpacity onPress={logout} style={styles.logoutBtn}>
                            <Ionicons name="log-out-outline" size={20} color="#666" />
                        </TouchableOpacity>
                    </View>
                </View>

                <ScrollView 
                    style={styles.content}
                    showsVerticalScrollIndicator={false}
                    refreshControl={
                        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="black" />
                    }
                >
                    {notification ? (
                        <View style={styles.notificationCard}>
                            <View style={styles.cardHeader}>
                                <View style={styles.tag}>
                                    <Text style={styles.tagText}>LLAMANDO</Text>
                                </View>
                                <Text style={styles.visitorName}>{notification.visitorName}</Text>
                                <Text style={styles.visitorReason}>{notification.reason}</Text>
                            </View>

                            <TouchableOpacity 
                                style={styles.cameraFrame}
                                onPress={() => setIsFullscreen(true)}
                                activeOpacity={0.9}
                            >
                                {verCamara ? (
                                    <View style={{ width: '100%', height: '100%' }}>
                                        <WebView 
                                            key={`small-${cameraTimestamp}`}
                                            source={{ html: `
                                                <body style="margin:0;padding:0;background-color:black;display:flex;align-items:center;justify-content:center;height:100vh;">
                                                    <img src="${streamUrl}" style="width:100%;height:auto;max-height:100%;filter: brightness(1.1) contrast(1.1);">
                                                </body>
                                            ` }}
                                            style={styles.cameraImage}
                                            scrollEnabled={false}
                                            pointerEvents="none"
                                        />
                                        <View style={styles.expandOverlay}>
                                            <Ionicons name="expand" size={16} color="white" />
                                        </View>
                                    </View>
                                ) : (
                                    <View style={styles.cameraOff}>
                                        <Ionicons name="videocam-off-outline" size={32} color="#DDD" />
                                        <Text style={styles.offlineText}>Cámara No Disponible en la Nube</Text>
                                        <Text style={styles.offlineSubText}>Usa la IP local para ver video</Text>
                                    </View>
                                )}
                            </TouchableOpacity>

                            <View style={styles.actions}>
                                <TouchableOpacity onPress={() => setVerCamara(!verCamara)} style={styles.viewCameraBtn}>
                                    <Text style={styles.viewCameraBtnText}>{verCamara ? 'Cerrar Cámara' : 'Ver Cámara'}</Text>
                                </TouchableOpacity>
                                
                                <View style={styles.mainActions}>
                                    <TouchableOpacity 
                                        onPress={() => handleDecision(notification.id, 2)} 
                                        style={styles.acceptBtn}
                                    >
                                        <Text style={styles.acceptBtnText}>Aceptar</Text>
                                    </TouchableOpacity>
                                    <TouchableOpacity 
                                        onPress={() => handleDecision(notification.id, 3)} 
                                        style={styles.rejectBtn}
                                    >
                                        <Text style={styles.rejectBtnText}>Rechazar</Text>
                                    </TouchableOpacity>
                                </View>
                            </View>
                        </View>
                    ) : (
                        <View style={styles.emptyState}>
                            <Ionicons name="notifications-off-outline" size={40} color="#CCC" />
                            <Text style={styles.emptyStateText}>
                                Todo está tranquilo.{"\n"}Esperando llamadas...
                            </Text>
                        </View>
                    )}

                    <View style={styles.historySection}>
                        <Text style={styles.sectionTitle}>Historial de Visitas</Text>
                        <View style={styles.divider} />
                        {history.length > 0 ? (
                            history.map((item, index) => (
                                <View key={item.id} style={styles.historyItem}>
                                    <View style={styles.historyInfo}>
                                        <Text style={styles.historyName}>{item.visitorName}</Text>
                                        <Text style={styles.historyReason}>{item.reason}</Text>
                                    </View>
                                    <View style={styles.historyMeta}>
                                        <View style={[
                                            styles.statusBadge, 
                                            { 
                                                backgroundColor: item.status === 'Approved' ? '#EFFFEE' : (item.status === 'Rejected' ? '#FFEEFF' : '#F5F5F5'),
                                                borderColor: item.status === 'Approved' ? '#0A0' : (item.status === 'Rejected' ? '#A0A' : '#666'),
                                            }
                                        ]}>
                                            <Text style={[
                                                styles.statusBadgeText, 
                                                { color: item.status === 'Approved' ? '#060' : (item.status === 'Rejected' ? '#606' : '#666') }
                                            ]}>
                                                {item.status === 'Approved' ? 'ACEPTADO' : (item.status === 'Rejected' ? 'RECHAZADO' : 'SIN RESPUESTA')}
                                            </Text>
                                        </View>
                                        <Text style={styles.historyTime}>{formatTimestamp(item.requestedAtUtc)}</Text>
                                    </View>
                                </View>
                            ))
                        ) : (
                            <Text style={styles.historyEmpty}>No hay registros hoy.</Text>
                        )}
                    </View>
                </ScrollView>

                {/* Modal Perfil */}
                <Modal visible={settingsVisible} animationType="slide" transparent={true}>
                    <View style={styles.modalOverlay}>
                        <View style={styles.settingsModal}>
                            <View style={styles.modalHeader}>
                                <Text style={styles.modalTitle}>Mi Perfil</Text>
                                <TouchableOpacity onPress={() => setSettingsVisible(false)}>
                                    <Ionicons name="close-outline" size={32} color="black" />
                                </TouchableOpacity>
                            </View>
                            
                            <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={{ paddingBottom: 40 }}>
                                <Text style={styles.label}>Nombre para el Directorio</Text>
                                <TextInput 
                                    style={styles.input} 
                                    value={newName} 
                                    onChangeText={setNewName} 
                                    placeholder={houseLabel}
                                    placeholderTextColor="#AAA"
                                />

                                <Text style={styles.label}>Nuevo Email (Opcional)</Text>
                                <TextInput 
                                    style={styles.input} 
                                    value={newEmail} 
                                    onChangeText={setNewEmail} 
                                    keyboardType="email-address"
                                    autoCapitalize="none"
                                    placeholder="ejemplo@email.com"
                                    placeholderTextColor="#AAA"
                                />

                                <View style={styles.modalDivider} />

                                <Text style={styles.label}>Contraseña Actual</Text>
                                <TextInput 
                                    style={styles.input} 
                                    value={currentPassword} 
                                    onChangeText={setCurrentPassword} 
                                    secureTextEntry
                                    placeholder="********"
                                    placeholderTextColor="#AAA"
                                />

                                <Text style={styles.label}>Nueva Contraseña (Opcional)</Text>
                                <TextInput 
                                    style={styles.input} 
                                    value={newPassword} 
                                    onChangeText={setNewPassword} 
                                    secureTextEntry
                                    placeholder="********"
                                    placeholderTextColor="#AAA"
                                />

                                <TouchableOpacity 
                                    onPress={saveSettings} 
                                    style={[styles.saveBtn, savingSettings && { opacity: 0.5 }]}
                                    disabled={savingSettings}
                                >
                                    {savingSettings ? (
                                        <ActivityIndicator color="white" />
                                    ) : (
                                        <Text style={styles.saveBtnText}>Actualizar Perfil</Text>
                                    )}
                                </TouchableOpacity>
                            </ScrollView>
                        </View>
                    </View>
                </Modal>

                {/* Modal Cámara Pantalla Completa */}
                <Modal visible={isFullscreen} animationType="fade" transparent={false}>
                    <View style={styles.fullscreenContainer}>
                        <WebView 
                            key={`full-${cameraTimestamp}`}
                            source={{ html: `
                                <body style="margin:0;padding:0;background-color:black;display:flex;align-items:center;justify-content:center;height:100vh;">
                                    <img src="${streamUrl}" style="width:100%;height:auto;max-height:100%;">
                                </body>
                            ` }}
                            style={{ flex: 1, backgroundColor: 'black' }}
                        />
                        <TouchableOpacity style={styles.closeFullscreen} onPress={() => setIsFullscreen(false)}>
                            <Ionicons name="close-circle" size={54} color="white" />
                        </TouchableOpacity>
                    </View>
                </Modal>
            </SafeAreaView>
        </ImageBackground>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#F7F7F7', // Blanco hueso
    },
    loadingContainer: {
        flex: 1,
        backgroundColor: '#F7F7F7',
        alignItems: 'center',
        justifyContent: 'center'
    },
    topBar: {
        paddingHorizontal: 24,
        paddingTop: 16,
        paddingBottom: 20,
        backgroundColor: 'rgba(255,255,255,0.8)',
        borderBottomWidth: 3,
        borderBottomColor: '#000',
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center'
    },
    topBarTitle: {
        color: '#000',
        fontSize: 22,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: -1
    },
    statusIndicator: {
        flexDirection: 'row',
        alignItems: 'center',
        marginTop: 2
    },
    dot: {
        width: 6,
        height: 6,
        borderRadius: 3,
        marginRight: 6
    },
    topBarStatus: {
        color: '#666',
        fontSize: 9,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: 1
    },
    iconBtn: {
        padding: 4
    },
    logoutBtn: {
        padding: 8,
        marginLeft: 4
    },
    content: {
        flex: 1,
        paddingHorizontal: 24,
        paddingTop: 24
    },
    notificationCard: {
        backgroundColor: '#FFF',
        borderWidth: 3,
        borderColor: '#000',
        padding: 24,
        marginBottom: 40,
        shadowColor: '#000',
        shadowOffset: { width: 8, height: 8 },
        shadowOpacity: 1,
        shadowRadius: 0,
        elevation: 10
    },
    tag: {
        backgroundColor: '#000',
        alignSelf: 'flex-start',
        paddingHorizontal: 10,
        paddingVertical: 5
    },
    tagText: {
        color: '#FFF',
        fontSize: 10,
        fontWeight: '900',
        letterSpacing: 2
    },
    visitorName: {
        color: '#000',
        fontSize: 32,
        fontWeight: '900',
        marginTop: 12,
        letterSpacing: -1
    },
    visitorReason: {
        color: '#666',
        fontSize: 14,
        fontWeight: '700',
        textTransform: 'uppercase',
        marginTop: 0
    },
    cameraFrame: {
        backgroundColor: '#000',
        aspectRatio: 16/9,
        borderWidth: 2,
        borderColor: '#000',
        marginTop: 20,
        overflow: 'hidden'
    },
    cameraImage: {
        width: '100%',
        height: '100%',
        backgroundColor: 'black'
    },
    cameraOff: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center'
    },
    offlineText: {
        color: '#444',
        fontSize: 9,
        fontWeight: '900',
        textTransform: 'uppercase',
        marginTop: 8,
        letterSpacing: 2
    },
    expandOverlay: {
        position: 'absolute',
        bottom: 10,
        right: 10,
        backgroundColor: 'rgba(0,0,0,0.6)',
        padding: 6,
        borderRadius: 4
    },
    actions: {
        marginTop: 24
    },
    viewCameraBtn: {
        borderWidth: 2,
        borderColor: '#000',
        paddingVertical: 14,
        alignItems: 'center',
        backgroundColor: '#FFF'
    },
    viewCameraBtnText: {
        color: '#000',
        fontWeight: '900',
        textTransform: 'uppercase',
        fontSize: 12
    },
    mainActions: {
        flexDirection: 'row',
        marginTop: 12,
        gap: 12
    },
    acceptBtn: {
        flex: 1.2,
        backgroundColor: '#000',
        paddingVertical: 18,
        alignItems: 'center',
        borderWidth: 2,
        borderColor: '#000'
    },
    acceptBtnText: {
        color: '#FFF',
        fontWeight: '900',
        textTransform: 'uppercase',
        fontSize: 14
    },
    rejectBtn: {
        flex: 0.8,
        borderWidth: 2,
        borderColor: '#000',
        paddingVertical: 18,
        alignItems: 'center',
        backgroundColor: '#F5F5F5'
    },
    rejectBtnText: {
        color: '#999',
        fontWeight: '900',
        textTransform: 'uppercase',
        fontSize: 14
    },
    emptyState: {
        paddingVertical: 60,
        borderWidth: 2,
        borderColor: '#DDD',
        borderStyle: 'dashed',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(0,0,0,0.02)'
    },
    emptyStateText: {
        color: '#AAA',
        fontSize: 11,
        fontWeight: '900',
        textTransform: 'uppercase',
        textAlign: 'center',
        lineHeight: 18,
        marginTop: 12
    },
    historySection: {
        marginTop: 32,
        marginBottom: 60
    },
    sectionTitle: {
        color: '#000',
        fontSize: 14,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: 1
    },
    divider: {
        height: 2,
        backgroundColor: '#000',
        marginTop: 8,
        marginBottom: 16
    },
    historyItem: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        paddingVertical: 18,
        borderBottomWidth: 1,
        borderBottomColor: '#EEE'
    },
    historyInfo: {
        flex: 1
    },
    historyName: {
        color: '#000',
        fontSize: 16,
        fontWeight: '900',
        textTransform: 'uppercase'
    },
    historyReason: {
        color: '#999',
        fontSize: 11,
        fontWeight: '700',
        marginTop: 1
    },
    historyMeta: {
        alignItems: 'flex-end'
    },
    statusBadge: {
        borderWidth: 1.5,
        paddingHorizontal: 8,
        paddingVertical: 4,
        marginBottom: 4
    },
    statusBadgeText: {
        fontSize: 8,
        fontWeight: '900',
        textTransform: 'uppercase'
    },
    historyTime: {
        color: '#AAA',
        fontSize: 9,
        fontWeight: '900'
    },
    modalOverlay: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.85)',
        justifyContent: 'flex-end'
    },
    settingsModal: {
        backgroundColor: '#FFF',
        borderTopWidth: 5,
        borderTopColor: '#000',
        height: '85%',
        padding: 32
    },
    modalHeader: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 40
    },
    modalTitle: {
        color: '#000',
        fontSize: 32,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: -1
    },
    label: {
        color: '#999',
        fontSize: 9,
        fontWeight: '900',
        textTransform: 'uppercase',
        marginBottom: 10,
        marginTop: 20,
        letterSpacing: 2
    },
    input: {
        backgroundColor: '#FFF',
        borderBottomWidth: 3,
        borderBottomColor: '#000',
        color: '#000',
        padding: 16,
        fontSize: 18,
        fontWeight: '700'
    },
    modalDivider: {
        height: 1,
        backgroundColor: '#EEE',
        marginVertical: 32
    },
    saveBtn: {
        backgroundColor: '#000',
        paddingVertical: 22,
        alignItems: 'center',
        marginTop: 48,
        shadowColor: '#000',
        shadowOffset: { width: 4, height: 4 },
        shadowOpacity: 0.3,
        shadowRadius: 0
    },
    saveBtnText: {
        color: '#FFF',
        fontWeight: '900',
        textTransform: 'uppercase',
        fontSize: 16
    },
    fullscreenContainer: {
        flex: 1,
        backgroundColor: '#000'
    },
    closeFullscreen: {
        position: 'absolute',
        top: 60,
        right: 24,
        zIndex: 10
    },
    historyEmpty: {
        color: '#CCC',
        fontSize: 10,
        fontWeight: '700',
        textTransform: 'uppercase',
        fontStyle: 'italic'
    }
});

