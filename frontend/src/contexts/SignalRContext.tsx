import React, { createContext, useContext, useEffect } from 'react';
import { useSignalR } from '../hooks/useSignalR';

interface SignalRContextType {
  isConnected: boolean;
  onStationStatusChanged: (callback: (notification: any) => void) => () => void;
  onConnectorStatusChanged: (callback: (notification: any) => void) => () => void;
  onSessionUpdate: (callback: (notification: any) => void) => () => void;
}

const SignalRContext = createContext<SignalRContextType | null>(null);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const signalR = useSignalR();

  useEffect(() => {
    // TEMPORÄR: Immer verbinden (auch ohne Token) für Debugging
    console.log('SignalRProvider: Initiating connection...');
    signalR.connect();

    // Disconnect when component unmounts
    return () => {
      console.log('SignalRProvider: Cleaning up...');
      signalR.disconnect();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Empty dependency array - only run once on mount

  return (
    <SignalRContext.Provider value={{
      isConnected: signalR.isConnected,
      onStationStatusChanged: signalR.onStationStatusChanged,
      onConnectorStatusChanged: signalR.onConnectorStatusChanged,
      onSessionUpdate: signalR.onSessionUpdate
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

