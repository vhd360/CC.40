import { useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5126';

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

    const token = localStorage.getItem('token');
    console.log('SignalR: Token status:', token ? `Available (${token.length} chars)` : 'Not available');

    isConnectingRef.current = true;
    console.log('SignalR: Attempting connection to:', `${API_BASE_URL}/hubs/notifications`);

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notifications`, {
        // Token nur senden wenn vorhanden
        ...(token && { accessTokenFactory: () => token }),
        transport: signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => 5000 // Retry every 5 seconds
      })
      .configureLogging(signalR.LogLevel.Information) // Mehr Logging für Debugging
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
    } catch (err) {
      console.error('❌ SignalR: Connection Error:', err);
      setIsConnected(false);
      isConnectingRef.current = false;
      
      // Retry connection after 10 seconds
      reconnectTimeoutRef.current = setTimeout(() => {
        console.log('SignalR: Retrying connection...');
        connect();
      }, 10000);
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
    newConnection.onclose((error) => {
      console.log('SignalR: Connection Closed', error?.message);
      setIsConnected(false);
      isConnectingRef.current = false;
      
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
      connection.on('StationStatusChanged', callback);
      
      return () => {
        connection.off('StationStatusChanged', callback);
      };
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

