import { useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5126';
const API_BASE_URL_FULL = `${API_BASE_URL}/api`;

interface StationStatusChangedNotification {
  Type: string;
  StationId: string;
  Status: string;
  Message?: string;
  Timestamp: string;
}

interface ConnectorStatusChangedNotification {
  Type: string;
  ConnectorId: string;
  Status: string;
  Message?: string;
  Timestamp: string;
}

interface SessionUpdateNotification {
  Type: string;
  SessionId: string;
  Status: string;
  Message?: string;
  Timestamp: string;
}

export const useSignalR = () => {
  const [isConnected, setIsConnected] = useState(false);
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const isConnectingRef = useRef(false);

  const connect = useCallback(async () => {
    // Prevent multiple simultaneous connection attempts
    if (isConnectingRef.current) {
      console.log('SignalR: Already connecting, skipping...');
      return;
    }

    // Dynamischer Token-Abruf für SignalR - wird bei jedem Request neu abgerufen
    const getToken = async (): Promise<string | null> => {
      let token = localStorage.getItem('token');
      
      // Prüfe, ob Token abgelaufen ist
      if (token) {
        try {
          // Dekodiere JWT Token (Base64)
          const payload = JSON.parse(atob(token.split('.')[1]));
          const expirationTime = payload.exp * 1000; // exp ist in Sekunden, konvertiere zu Millisekunden
          const currentTime = Date.now();
          
          // Wenn Token in weniger als 5 Minuten abläuft oder bereits abgelaufen ist, erneuere es
          if (expirationTime - currentTime < 5 * 60 * 1000) {
            console.log('SignalR: Token läuft bald ab oder ist abgelaufen, erneuere...');
            token = null; // Setze auf null, um Refresh zu triggern
          }
        } catch (error) {
          console.warn('SignalR: Fehler beim Dekodieren des Tokens, versuche Refresh:', error);
          token = null; // Bei Fehler, versuche Refresh
        }
      }
      
      // Wenn kein Token vorhanden oder abgelaufen, versuche Token zu erneuern
      if (!token) {
        const refreshTokenValue = localStorage.getItem('refreshToken');
        if (refreshTokenValue) {
          try {
            console.log('SignalR: Erneuere Token...');
            const response = await fetch(`${API_BASE_URL_FULL}/auth/refresh`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify(refreshTokenValue)
            });
            
            if (response.ok) {
              const data = await response.json();
              if (data.success && data.token) {
                localStorage.setItem('token', data.token);
                if (data.refreshToken) {
                  localStorage.setItem('refreshToken', data.refreshToken);
                }
                token = data.token;
                console.log('SignalR: Token erfolgreich erneuert');
              } else {
                console.error('SignalR: Token-Refresh fehlgeschlagen - keine Token in Response');
              }
            } else {
              console.error('SignalR: Token-Refresh fehlgeschlagen - Response nicht OK:', response.status);
            }
          } catch (error) {
            console.error('SignalR: Token refresh failed:', error);
          }
        } else {
          console.warn('SignalR: Kein Refresh-Token vorhanden');
        }
      }
      
      return token;
    };

    const token = await getToken();
    console.log('SignalR: Token status:', token ? `Available (${token.length} chars)` : 'Not available');

    isConnectingRef.current = true;
    console.log('SignalR: Attempting connection to:', `${API_BASE_URL}/hubs/notifications`);

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notifications`, {
        // Dynamischer Token-Abruf - wird bei jedem Request aufgerufen
        accessTokenFactory: async () => {
          const currentToken = await getToken();
          if (!currentToken) {
            console.warn('SignalR: Kein Token verfügbar für Handshake');
            return '';
          }
          return currentToken;
        },
        // Verwende WebSocket als primären Transport, mit Fallback auf Long Polling
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling | signalR.HttpTransportType.ServerSentEvents,
        skipNegotiation: false // Erlaube Negotiation für bessere Kompatibilität
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 2s, 4s, 8s, max 10s
          return Math.min(10000, Math.pow(2, retryContext.previousRetryCount) * 1000);
        }
      })
      .configureLogging(signalR.LogLevel.Warning) // Reduziertes Logging
      .build();

    try {
      console.log('SignalR: Starting connection...');
      await newConnection.start();
      console.log('✅ SignalR: Connected successfully!');
      setIsConnected(true);
      setConnection(newConnection);
      isConnectingRef.current = false;

      // Clear any pending reconnect
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
        reconnectTimeoutRef.current = null;
      }
    } catch (err: any) {
      console.error('❌ SignalR: Connection Error:', err);
      console.error('Error details:', {
        message: err.message,
        name: err.name,
        stack: err.stack
      });
      setIsConnected(false);
      isConnectingRef.current = false;
      
      // Wenn der Fehler ein Handshake-Fehler ist, versuche es mit einem neuen Token
      if (err.message?.includes('handshake') || err.message?.includes('Handshake')) {
        console.log('SignalR: Handshake-Fehler erkannt, versuche Token zu erneuern...');
        // Lösche aktuellen Token und versuche Refresh
        localStorage.removeItem('token');
        // Retry nach kurzer Verzögerung
        reconnectTimeoutRef.current = setTimeout(() => {
          console.log('SignalR: Retrying connection after handshake error...');
          connect();
        }, 2000);
      } else {
        // Retry connection after 10 seconds für andere Fehler
        reconnectTimeoutRef.current = setTimeout(() => {
          console.log('SignalR: Retrying connection...');
          connect();
        }, 10000);
      }
    }

    // Handle reconnecting state
    newConnection.onreconnecting((error) => {
      console.log('SignalR: Reconnecting...', error?.message);
      setIsConnected(false);
      isConnectingRef.current = true;
    });

    // Handle reconnected state
    newConnection.onreconnected((connectionId) => {
      console.log('✅ SignalR: Reconnected successfully!');
      setIsConnected(true);
      isConnectingRef.current = false;
    });

    // Handle closed connection
    newConnection.onclose(async (error) => {
      console.log('SignalR: Connection Closed', error?.message);
      setIsConnected(false);
      isConnectingRef.current = false;
      
      // Wenn Verbindung wegen Authentifizierung geschlossen wurde, versuche Token zu erneuern
      if (error) {
        const refreshTokenValue = localStorage.getItem('refreshToken');
        if (refreshTokenValue) {
          try {
            console.log('SignalR: Attempting token refresh after connection close...');
            const response = await fetch(`${API_BASE_URL_FULL}/auth/refresh`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify(refreshTokenValue)
            });
            
            if (response.ok) {
              const data = await response.json();
              if (data.success && data.token) {
                localStorage.setItem('token', data.token);
                if (data.refreshToken) {
                  localStorage.setItem('refreshToken', data.refreshToken);
                }
                console.log('SignalR: Token refreshed, reconnecting...');
                // Reconnect mit neuem Token
                setTimeout(() => connect(), 2000);
                return;
              }
            }
          } catch (err) {
            console.error('SignalR: Token refresh failed:', err);
          }
        }
      }
      
      // Attempt to reconnect after 10 seconds
      if (!reconnectTimeoutRef.current) {
        reconnectTimeoutRef.current = setTimeout(() => {
          console.log('SignalR: Attempting reconnect after close...');
          connect();
        }, 10000);
      }
    });
  }, []);

  const disconnect = useCallback(async () => {
    if (connection) {
      await connection.stop();
      setIsConnected(false);
      setConnection(null);
      
      // Clear any pending reconnect
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
        reconnectTimeoutRef.current = null;
      }
    }
  }, [connection]);

  // Subscribe to station status changes
  const onStationStatusChanged = useCallback((callback: (notification: StationStatusChangedNotification) => void) => {
    if (connection) {
      console.log('📡 Registriere StationStatusChanged Handler...');
      connection.on('StationStatusChanged', (notification) => {
        console.log('📡 SignalR: StationStatusChanged empfangen:', notification);
        callback(notification);
      });
      
      return () => {
        console.log('📡 Entferne StationStatusChanged Handler...');
        connection.off('StationStatusChanged', callback);
      };
    } else {
      console.warn('⚠️ SignalR: Keine Verbindung vorhanden, kann StationStatusChanged nicht registrieren');
    }
    return () => {};
  }, [connection]);

  // Subscribe to connector status changes
  const onConnectorStatusChanged = useCallback((callback: (notification: ConnectorStatusChangedNotification) => void) => {
    if (connection) {
      connection.on('ConnectorStatusChanged', callback);
      
      return () => {
        connection.off('ConnectorStatusChanged', callback);
      };
    }
    return () => {};
  }, [connection]);

  // Subscribe to session updates
  const onSessionUpdate = useCallback((callback: (notification: SessionUpdateNotification) => void) => {
    if (connection) {
      connection.on('SessionUpdate', callback);
      
      return () => {
        connection.off('SessionUpdate', callback);
      };
    }
    return () => {};
  }, [connection]);

  return {
    isConnected,
    connect,
    disconnect,
    onStationStatusChanged,
    onConnectorStatusChanged,
    onSessionUpdate
  };
};
