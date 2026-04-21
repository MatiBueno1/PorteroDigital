import { DarkTheme, DefaultTheme, ThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import 'react-native-reanimated';
import '../global.css'; // Habilitar NativeWind v4

import { useColorScheme } from '@/hooks/use-color-scheme';

// Eliminamos unstable_settings para manejar la navegación nosotros mismos


export default function RootLayout() {
  const colorScheme = useColorScheme();

  return (
    <ThemeProvider value={DarkTheme}>
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="index" options={{ title: 'Inicio' }} />
        <Stack.Screen name="login" options={{ title: 'Ingreso' }} />
      </Stack>
      <StatusBar style="light" />
    </ThemeProvider>
  );
}
