import React, { createContext, useContext, useEffect, useState } from 'react';
import { useSignalR } from '../hooks/useSignalR';

interface SignalRContextType {
  isConnected: boolean;
  onStationStatusChanged: (callback: (notification: any) => void) => () => void;
  onConnectorStatusChanged: (callback: (notification: any) => void) => () => void;
  onSessionUpdate: (callback: (notification: any) => void) => () => void;
  reconnect: () => void;
}

const SignalRContext = createContext<SignalRContextType | null>(null);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const signalR = useSignalR();
  const [shouldConnect, setShouldConnect] = useState(false);

  useEffect(() => {
    // Prüfe initial, ob ein Token vorhanden ist
    const token = localStorage.getItem('token');
    setShouldConnect(!!token);
  }, []);

  useEffect(() => {
    if (shouldConnect) {
      console.log('SignalRProvider: Connecting with authentication...');
      signalR.connect();
    } else {
      console.log('SignalRProvider: Not authenticated, skipping connection');
      signalR.disconnect();
    }

    // Disconnect when component unmounts
    return () => {
      signalR.disconnect();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shouldConnect]);

  // Storage Event Listener für Login/Logout in anderen Tabs
  useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'token') {
        setShouldConnect(!!e.newValue);
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const reconnect = () => {
    const token = localStorage.getItem('token');
    if (token) {
      setShouldConnect(true);
    }
  };

  return (
    <SignalRContext.Provider value={{
      isConnected: signalR.isConnected,
      onStationStatusChanged: signalR.onStationStatusChanged,
      onConnectorStatusChanged: signalR.onConnectorStatusChanged,
      onSessionUpdate: signalR.onSessionUpdate,
      reconnect
    }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalRContext = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalRContext must be used within SignalRProvider');
  }
  return context;
};

