import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ActivityIndicator, Alert, SafeAreaView, KeyboardAvoidingView, Platform, ScrollView, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import * as SecureStore from 'expo-secure-store';
import apiClient from '../src/api/apiClient';
import { Ionicons } from '@expo/vector-icons';

export default function LoginScreen() {
    const router = useRouter();
    const [form, setForm] = useState({ email: '', password: '' });
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const handleLogin = async () => {
        if (!form.email || !form.password) {
            Alert.alert('Error', 'Por favor completa todos los campos.');
            return;
        }

        setLoading(true);
        try {
            const res = await apiClient.post('/api/auth/login', form);
            const data = res.data;

            await SecureStore.setItemAsync('accessToken', data.accessToken);
            await SecureStore.setItemAsync('houseId', data.houseId);
            await SecureStore.setItemAsync('residentName', data.residentName || '');
            
            router.replace('/');
        } catch (error: any) {
            console.error(error);
            const msg = error.response?.data || 'No se pudo conectar al servidor.';
            Alert.alert('Error de ingreso', typeof msg === 'string' ? msg : 'Credenciales incorrectas');
        } finally {
            setLoading(false);
        }
    };

    return (
        <SafeAreaView style={styles.container}>
            <KeyboardAvoidingView 
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                style={{ flex: 1 }}
            >
                <ScrollView contentContainerStyle={styles.scrollContent}>
                    <View style={styles.header}>
                        <Text style={styles.subTitle}>Portería Digital</Text>
                        <Text style={styles.mainTitle}>
                            Acceso{"\n"}Residente
                        </Text>
                    </View>

                    <View style={styles.form}>
                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Email</Text>
                            <TextInput
                                style={styles.input}
                                placeholder="casa01@portero.local"
                                placeholderTextColor="#444"
                                keyboardType="email-address"
                                autoCapitalize="none"
                                autoCorrect={false}
                                value={form.email}
                                onChangeText={(text) => setForm({ ...form, email: text })}
                            />
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Password</Text>
                            <View style={styles.passwordContainer}>
                                <TextInput
                                    style={[styles.input, { flex: 1, borderRightWidth: 0 }]}
                                    placeholder="••••••••"
                                    placeholderTextColor="#444"
                                    secureTextEntry={!showPassword}
                                    value={form.password}
                                    onChangeText={(text) => setForm({ ...form, password: text })}
                                />
                                <TouchableOpacity 
                                    style={styles.eyeBtn}
                                    onPress={() => setShowPassword(!showPassword)}
                                >
                                    <Ionicons 
                                        name={showPassword ? "eye-off-outline" : "eye-outline"} 
                                        size={20} 
                                        color="#666" 
                                    />
                                </TouchableOpacity>
                            </View>
                        </View>

                        <TouchableOpacity
                            onPress={handleLogin}
                            disabled={loading}
                            style={[styles.button, loading && { opacity: 0.4 }]}
                        >
                            {loading ? (
                                <ActivityIndicator color="#000" />
                            ) : (
                                <Text style={styles.buttonText}>Ingresar</Text>
                            )}
                        </TouchableOpacity>
                    </View>

                    <Text style={styles.footer}>
                        Credenciales: casa01@portero.local / Portero123!
                    </Text>
                </ScrollView>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#000',
    },
    scrollContent: {
        flexGrow: 1,
        justifyContent: 'center',
        paddingHorizontal: 32,
    },
    header: {
        marginBottom: 48,
    },
    subTitle: {
        color: '#666',
        fontSize: 10,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: 3,
        marginBottom: 8,
    },
    mainTitle: {
        color: '#FFF',
        fontSize: 48,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: -2,
        lineHeight: 48,
    },
    form: {
        gap: 24,
    },
    inputGroup: {
        marginBottom: 20,
    },
    label: {
        color: '#666',
        fontSize: 10,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: 2,
        marginBottom: 8,
    },
    input: {
        borderWidth: 2,
        borderColor: '#333',
        color: '#FFF',
        paddingHorizontal: 16,
        paddingVertical: 16,
        fontSize: 14,
        fontWeight: '700',
    },
    passwordContainer: {
        flexDirection: 'row',
        alignItems: 'stretch',
    },
    eyeBtn: {
        borderWidth: 2,
        borderLeftWidth: 0,
        borderColor: '#333',
        justifyContent: 'center',
        paddingHorizontal: 16,
    },
    button: {
        backgroundColor: '#FFF',
        paddingVertical: 20,
        alignItems: 'center',
        marginTop: 32,
    },
    buttonText: {
        color: '#000',
        fontSize: 14,
        fontWeight: '900',
        textTransform: 'uppercase',
        letterSpacing: 2,
    },
    footer: {
        marginTop: 48,
        color: '#444',
        fontSize: 10,
        fontWeight: '700',
        textTransform: 'uppercase',
        letterSpacing: 1,
        textAlign: 'center',
    }
});

