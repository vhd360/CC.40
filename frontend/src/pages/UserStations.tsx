import React, { useState, useEffect, useCallback } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Loader2, MapPin, Zap, Battery, Building2, Navigation, Play, AlertCircle, CheckCircle, QrCode } from 'lucide-react';
import { api } from '../services/api';
import { useSignalRContext } from '../contexts/SignalRContext';
import { QRScanner } from '../components/QRScanner';

export const UserStations: React.FC = () => {
  const { isConnected, onStationStatusChanged, onConnectorStatusChanged } = useSignalRContext();
  const [loading, setLoading] = useState(true);
  const [stations, setStations] = useState<any[]>([]);
  const [selectedStation, setSelectedStation] = useState<any | null>(null);
  const [showStartDialog, setShowStartDialog] = useState(false);
  const [chargingPoints, setChargingPoints] = useState<any[]>([]);
  const [myVehicles, setMyVehicles] = useState<any[]>([]);
  const [selectedChargingPoint, setSelectedChargingPoint] = useState<string>('');
  const [selectedVehicle, setSelectedVehicle] = useState<string>('');
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showQRScanner, setShowQRScanner] = useState(false);
  const [scannedVehicle, setScannedVehicle] = useState<any | null>(null);
  const [stationAvailability, setStationAvailability] = useState<Record<string, { hasAvailablePoints: boolean; isLoading: boolean }>>({});

  const loadStations = useCallback(async () => {
    try {
      setLoading(true);
      const data = await api.getUserAvailableStations();
      setStations(data);
      
      // Prüfe für jede Station, ob verfügbare Ladepunkte vorhanden sind
      const availability: Record<string, { hasAvailablePoints: boolean; isLoading: boolean }> = {};
      
      for (const station of data) {
        // Station ist verfügbar wenn Status "Available" ist, unabhängig von lastHeartbeat
        // lastHeartbeat wird nur als zusätzliche Info verwendet, nicht als harte Bedingung
        const isStationOnline = station.lastHeartbeat && 
          (new Date().getTime() - new Date(station.lastHeartbeat).getTime()) < 10 * 60 * 1000;
        
        // Station ist verfügbar wenn Status "Available" ist und nicht Offline/Unavailable/OutOfOrder
        const isStationAvailable = station.status === 'Available' && 
                                   station.status !== 'Unavailable' && 
                                   station.status !== 'OutOfOrder' &&
                                   station.status !== 'Offline' &&
                                   station.chargeBoxId;
        
        if (isStationAvailable) {
          availability[station.id] = { hasAvailablePoints: false, isLoading: true };
          
          // Lade Ladepunkte für diese Station
          try {
            const chargingPointsData = await api.getStationConnectors(station.id);
            const availablePoints = chargingPointsData.filter((cp: any) => cp.isAvailable);
            availability[station.id] = { 
              hasAvailablePoints: availablePoints.length > 0, 
              isLoading: false 
            };
          } catch (err) {
            console.error(`Failed to load charging points for station ${station.id}:`, err);
            availability[station.id] = { hasAvailablePoints: false, isLoading: false };
          }
        } else {
          availability[station.id] = { hasAvailablePoints: false, isLoading: false };
        }
      }
      
      setStationAvailability(availability);
    } catch (error) {
      console.error('Failed to load stations:', error);
      setError('Fehler beim Laden der Ladestationen');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStations();
    loadMyVehicles();
  }, [loadStations]);

  // SignalR: Station Status Updates
  useEffect(() => {
    if (!isConnected) {
      console.log('⚠️ SignalR nicht verbunden, warte auf Verbindung...');
      return;
    }

    console.log('✅ SignalR verbunden, registriere Station Status Handler...');

    const handleStationUpdate = async (notification: any) => {
      console.log('📡 Station Status Update received:', notification);
      console.log('🔍 StationId from notification:', notification.StationId, typeof notification.StationId);
      console.log('🔍 Status:', notification.Status);
      
      const isUnavailable = notification.Status === 'Unavailable' || notification.Status === 'OutOfOrder';
      const isAvailable = notification.Status === 'Available';
      
      // Aktualisiere Station-Status im State
      setStations(prevStations => {
        console.log('🔍 Current stations:', prevStations.map(s => ({ id: s.id, idType: typeof s.id, status: s.status, lastHeartbeat: s.lastHeartbeat })));
        
        const updated = prevStations.map(station => {
          // Vergleiche IDs als Strings, da sie möglicherweise unterschiedliche Typen haben
          const stationIdStr = String(station.id);
          const notificationIdStr = String(notification.StationId);
          const matches = stationIdStr === notificationIdStr || station.id === notification.StationId;
          
          if (matches) {
            console.log('✅ Station gefunden und aktualisiert:', stationIdStr, '->', notification.Status);
            // Wenn Station wieder online kommt, aktualisiere auch lastHeartbeat
            const updatedStation: any = { ...station, status: notification.Status };
            if (isAvailable) {
              // Setze lastHeartbeat auf jetzt, wenn Station wieder online kommt
              updatedStation.lastHeartbeat = new Date().toISOString();
              console.log('🔄 LastHeartbeat aktualisiert für Station:', stationIdStr);
            } else if (isUnavailable) {
              // Entferne lastHeartbeat, wenn Station offline geht
              updatedStation.lastHeartbeat = null;
            }
            return updatedStation;
          }
          return station;
        });
        
        // Prüfe, ob eine Station aktualisiert wurde
        const wasUpdated = updated.some((s, idx) => s.status !== prevStations[idx].status);
        if (!wasUpdated) {
          console.warn('⚠️ Keine Station mit ID', notification.StationId, 'gefunden!');
        }
        
        return updated;
      });
      
      // Wenn Station wieder verfügbar wird, aktualisiere auch Verfügbarkeitsprüfung
      if (isAvailable) {
        const stationIdForAvailability = notification.StationId;
        console.log('🔄 Prüfe Verfügbarkeit für Station:', stationIdForAvailability);
        
        // Setze Verfügbarkeit auf "wird geprüft"
        setStationAvailability(prev => ({
          ...prev,
          [stationIdForAvailability]: { hasAvailablePoints: false, isLoading: true }
        }));
        
        // Lade Ladepunkte für diese Station asynchron
        api.getStationConnectors(stationIdForAvailability)
          .then(chargingPointsData => {
            const availablePoints = chargingPointsData.filter((cp: any) => cp.isAvailable);
            setStationAvailability(prev => ({
              ...prev,
              [stationIdForAvailability]: { 
                hasAvailablePoints: availablePoints.length > 0, 
                isLoading: false 
              }
            }));
            console.log('✅ Verfügbarkeit aktualisiert für Station:', stationIdForAvailability, '- Verfügbare Punkte:', availablePoints.length);
          })
          .catch(err => {
            console.error(`❌ Fehler beim Laden der Ladepunkte für Station ${stationIdForAvailability}:`, err);
            setStationAvailability(prev => ({
              ...prev,
              [stationIdForAvailability]: { hasAvailablePoints: false, isLoading: false }
            }));
          });
        
        // Lade auch die vollständige Station-Liste neu, um sicherzustellen, dass alle Daten aktuell sind
        console.log('🔄 Lade Station-Liste neu für vollständige Daten...');
        await loadStations();
      }
      
      // Wenn Station offline geht, aktualisiere Verfügbarkeit
      if (isUnavailable) {
        const stationIdForAvailability = notification.StationId;
        setStationAvailability(prev => ({
          ...prev,
          [stationIdForAvailability]: { hasAvailablePoints: false, isLoading: false }
        }));
      }

      // Wenn der Dialog für diese Station geöffnet ist, Ladepunkte neu laden
      // Vergleiche IDs als Strings für konsistente Vergleichbarkeit
      const selectedStationIdStr = selectedStation?.id ? String(selectedStation.id).toLowerCase() : null;
      const notificationIdStrForDialog = String(notification.StationId).toLowerCase();
      const isSelectedStation = selectedStationIdStr === notificationIdStrForDialog || selectedStation?.id === notification.StationId;
      
      if (isSelectedStation) {
        if (isUnavailable) {
          // Station ist nicht mehr verfügbar - alle Ladepunkte als nicht verfügbar markieren
          setChargingPoints(prevPoints =>
            prevPoints.map(point => ({
              ...point,
              isAvailable: false
            }))
          );
        } else {
          // Station ist wieder verfügbar - Ladepunkte neu laden
          console.log('🔄 Station wieder verfügbar, lade Ladepunkte neu...');
          try {
            const chargingPointsData = await api.getStationConnectors(notification.StationId);
            console.log('✅ Ladepunkte neu geladen:', chargingPointsData);
            setChargingPoints(chargingPointsData);
          } catch (err) {
            console.error('❌ Fehler beim Neuladen der Ladepunkte:', err);
          }
        }
      }
      
      // Wenn Station wieder verfügbar wird, Station-Liste aktualisieren (für LastHeartbeat etc.)
      if (!isUnavailable && notification.Status === 'Available') {
        console.log('🔄 Station wieder verfügbar, aktualisiere Station-Liste und Verfügbarkeit...');
        // Station-Liste neu laden, um aktuelle Daten zu erhalten (inkl. lastHeartbeat)
        await loadStations();
      }
    };

    const unsubscribe = onStationStatusChanged(handleStationUpdate);
    return () => unsubscribe();
  }, [isConnected, onStationStatusChanged, selectedStation?.id, loadStations]);

  // SignalR: ChargingPoint Status Updates (Connector Status wird jetzt für ChargingPoints verwendet)
  useEffect(() => {
    if (!isConnected || chargingPoints.length === 0) return;

    const handleChargingPointUpdate = (notification: any) => {
      console.log('📡 ChargingPoint Status Update received:', notification);
      
      // Update charging points in dialog
      // WICHTIG: isAvailable sollte auch von der Station-Verbindung abhängen, nicht nur vom Status
      setChargingPoints(prevPoints =>
        prevPoints.map(point => {
          if (point.id === notification.ConnectorId || point.id === notification.ChargingPointId) {
            // Prüfe auch den Status der Station aus dem State
            setStations(currentStations => {
              const stationStatus = currentStations.find(s => s.id === selectedStation?.id)?.status;
              const isStationAvailable = stationStatus !== 'Unavailable' && stationStatus !== 'OutOfOrder';
              
              return { 
                ...point, 
                status: notification.Status, 
                isAvailable: notification.Status === 'Available' && isStationAvailable
              };
            });
            
            // Fallback: Verwende den aktuellen Station-Status aus dem State
            const stationStatus = stations.find(s => s.id === selectedStation?.id)?.status;
            const isStationAvailable = stationStatus !== 'Unavailable' && stationStatus !== 'OutOfOrder';
            
            return { 
              ...point, 
              status: notification.Status, 
              isAvailable: notification.Status === 'Available' && isStationAvailable
            };
          }
          return point;
        })
      );
    };

    const unsubscribe = onConnectorStatusChanged(handleChargingPointUpdate);
    return () => unsubscribe();
  }, [isConnected, onConnectorStatusChanged, chargingPoints.length, stations, selectedStation?.id]);

  const loadMyVehicles = async () => {
    try {
      const vehicles = await api.getMyVehicles();
      setMyVehicles(vehicles);
    } catch (error) {
      console.error('Failed to load vehicles:', error);
    }
  };

  const handleStartClick = async (station: any) => {
    setError(null);
    
    // Prüfe, ob Station verfügbar ist
    if (station.status === 'Unavailable' || station.status === 'OutOfOrder') {
      setError('Diese Ladestation ist derzeit nicht verfügbar');
      return;
    }
    
    setSelectedStation(station);
    try {
      const chargingPointsData = await api.getStationConnectors(station.id);
      console.log('📡 ChargingPoints geladen:', chargingPointsData);
      console.log('📊 Anzahl ChargingPoints gesamt:', chargingPointsData.length);
      
      const availablePoints = chargingPointsData.filter((cp: any) => cp.isAvailable);
      console.log('✅ Verfügbare ChargingPoints:', availablePoints.length);
      console.log('🔍 ChargingPoint Details:', chargingPointsData.map((cp: any) => ({
        id: cp.id,
        evseId: cp.evseId,
        connectorId: cp.connectorId,
        status: cp.status,
        isAvailable: cp.isAvailable
      })));
      
      // Prüfe, ob überhaupt verfügbare Ladepunkte vorhanden sind
      if (availablePoints.length === 0) {
        setError('Keine verfügbaren Ladepunkte an dieser Station');
        return;
      }
      
      setChargingPoints(chargingPointsData);
      setShowStartDialog(true);
      setSelectedChargingPoint('');
      setSelectedVehicle('');
      setShowQRScanner(false);
      setScannedVehicle(null);
    } catch (err: any) {
      console.error('❌ Fehler beim Laden der ChargingPoints:', err);
      setError(err.message || 'Fehler beim Laden der Ladepunkte');
    }
  };

  const handleQRScan = async (qrData: string) => {
    console.log('QR-Code gescannt:', qrData);
    setShowQRScanner(false);
    
    try {
      // Versuche Fahrzeug anhand QR-Code zu finden
      const vehicles = await api.getVehicles();
      const foundVehicle = vehicles.find(v => 
        v.qrCode === qrData || 
        `VEHICLE-${v.id}` === qrData ||
        v.id === qrData
      );
      
      if (foundVehicle) {
        setScannedVehicle(foundVehicle);
        setSelectedVehicle(foundVehicle.id);
        setSuccess(`✅ Fahrzeug erkannt: ${foundVehicle.make} ${foundVehicle.model} (${foundVehicle.licensePlate})`);
        setTimeout(() => setSuccess(null), 5000);
      } else {
        setError('Fahrzeug nicht gefunden. Bitte manuell auswählen.');
        setTimeout(() => setError(null), 5000);
      }
    } catch (err: any) {
      setError('Fehler beim Suchen des Fahrzeugs');
      setTimeout(() => setError(null), 5000);
    }
  };

  const handleStartCharging = async () => {
    if (!selectedChargingPoint) {
      setError('Bitte wählen Sie einen Ladepunkt');
      return;
    }

    try {
      setStarting(true);
      setError(null);
      await api.startChargingSession(selectedChargingPoint, selectedVehicle || undefined);
      setSuccess(`Ladevorgang erfolgreich gestartet an ${selectedStation?.name}`);
      setShowStartDialog(false);
      setScannedVehicle(null);
      setTimeout(() => {
        setSuccess(null);
      }, 5000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Starten des Ladevorgangs');
    } finally {
      setStarting(false);
    }
  };

  // Hilfsfunktion: Bestimmt den anzuzeigenden Status-Text
  const getDisplayStatus = (status: string, lastHeartbeat?: string): string => {
    // Wenn Status explizit gesetzt ist, verwende diesen (hat Vorrang)
    if (status && status !== 'Unknown') {
      return status;
    }
    
    // Fallback: Prüfe lastHeartbeat nur wenn Status nicht explizit gesetzt ist
    const isOffline = !lastHeartbeat || 
      (new Date().getTime() - new Date(lastHeartbeat).getTime()) >= 10 * 60 * 1000;
    
    return isOffline ? 'Offline' : 'Unknown';
  };

  const getStatusColor = (status: string, lastHeartbeat?: string, chargeBoxId?: string) => {
    // Status-Farben basierend auf dem tatsächlichen Status aus dem Payload
    const colors: Record<string, string> = {
      'Available': 'bg-green-500',
      'Occupied': 'bg-yellow-500',
      'OutOfOrder': 'bg-red-500',
      'Reserved': 'bg-blue-500',
      'Unavailable': 'bg-gray-500',
      'Offline': 'bg-gray-500'
    };
    
    // Wenn Status explizit gesetzt ist, verwende diesen
    if (status && colors[status]) {
      return colors[status];
    }
    
    // Fallback: Prüfe lastHeartbeat nur wenn Status nicht explizit gesetzt ist
    const isOffline = !lastHeartbeat || 
      (new Date().getTime() - new Date(lastHeartbeat).getTime()) >= 10 * 60 * 1000 ||
      !chargeBoxId;
    
    if (isOffline) {
      return 'bg-gray-500';
    }
    
    return colors[status] || 'bg-gray-500';
  };

  const getConnectorStatusBadge = (status: string) => {
    const variants: Record<string, { color: string; label: string }> = {
      'Available': { color: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200', label: 'Verfügbar' },
      'Occupied': { color: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200', label: 'Belegt' },
      'Faulted': { color: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200', label: 'Fehler' },
      'Unavailable': { color: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200', label: 'Nicht verfügbar' }
    };
    const config = variants[status] || { color: 'bg-gray-100 text-gray-800', label: status };
    return <Badge className={config.color}>{config.label}</Badge>;
  };

  const openInMaps = (lat: number, lng: number, name: string) => {
    const url = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
    window.open(url, '_blank');
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Ladestationen...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Verfügbare Ladestationen</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            {stations.length} Ladestation{stations.length !== 1 ? 'en' : ''} stehen Ihnen zur Verfügung
          </p>
        </div>
        {isConnected && (
          <Badge className="bg-green-500 hover:bg-green-600 flex items-center gap-2">
            <span className="w-2 h-2 bg-white rounded-full animate-pulse" />
            Live-Updates aktiv
          </Badge>
        )}
      </div>

      {/* Success/Error Messages */}
      {error && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {success && (
        <Alert className="border-green-500 bg-green-50 dark:bg-green-950">
          <CheckCircle className="h-4 w-4 text-green-600 dark:text-green-400" />
          <AlertDescription className="text-green-600 dark:text-green-400">{success}</AlertDescription>
        </Alert>
      )}

      {/* Stations Grid */}
      {stations.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {stations.map((station) => (
            <Card key={station.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-2">
                    <Zap className="h-5 w-5 text-blue-600" />
                    <CardTitle className="text-lg text-gray-900 dark:text-gray-100">{station.name}</CardTitle>
                  </div>
                  <div className={`w-3 h-3 rounded-full ${getStatusColor(station.status, station.lastHeartbeat, station.chargeBoxId)}`} />
                </div>
                <CardDescription className="text-xs">
                  ID: {station.stationId}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Location */}
                <div className="bg-gray-50 dark:bg-gray-800 p-3 rounded-lg space-y-2">
                  <div className="flex items-start space-x-2">
                    <Building2 className="h-4 w-4 text-gray-600 dark:text-gray-400 mt-0.5 flex-shrink-0" />
                    <div className="text-sm">
                      <div className="font-medium text-gray-900 dark:text-gray-100">{station.chargingPark.name}</div>
                      <div className="text-gray-500 dark:text-gray-400 text-xs">
                        {station.chargingPark.address}, {station.chargingPark.city}
                      </div>
                      <Badge variant="outline" className="mt-1 text-xs">
                        {station.chargingPark.tenant.name}
                      </Badge>
                    </div>
                  </div>
                </div>

                {/* Specs */}
                <div className="grid grid-cols-2 gap-3 text-sm">
                  <div className="flex items-center space-x-2">
                    <Battery className="h-4 w-4 text-green-600" />
                    <span className="font-medium text-gray-900 dark:text-gray-100">{station.maxPower} kW</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Zap className="h-4 w-4 text-blue-600" />
                    <span className="text-gray-900 dark:text-gray-100">{station.numberOfConnectors} Stecker</span>
                  </div>
                  <div className="text-xs text-gray-600 dark:text-gray-400">
                    Hersteller: <span className="font-medium">{station.vendor}</span>
                  </div>
                  <div className="text-xs text-gray-600 dark:text-gray-400">
                    Typ: <span className="font-medium">{station.type}</span>
                  </div>
                </div>

                {/* Groups */}
                {station.groups && station.groups.length > 0 && (
                  <div className="flex flex-wrap gap-1">
                    {station.groups.map((group: any) => (
                      <Badge key={group.id} variant="secondary" className="text-xs">
                        {group.name}
                      </Badge>
                    ))}
                  </div>
                )}

                {/* Action Buttons */}
                <div className="flex gap-2">
                  {(() => {
                    const isStationUnavailable = station.status === 'Unavailable' || station.status === 'OutOfOrder';
                    // Station ist offline nur wenn Status explizit "Offline" ist, nicht basierend auf lastHeartbeat
                    const isStationOffline = station.status === 'Offline';
                    const hasNoChargeBoxId = !station.chargeBoxId;
                    const availability = stationAvailability[station.id];
                    const hasNoAvailablePoints = availability && !availability.hasAvailablePoints && !availability.isLoading;
                    const isLoadingAvailability = availability?.isLoading;
                    
                    // Prüfe auch, ob Station wirklich verbunden ist (für Button-Deaktivierung)
                    const isNotConnected = !station.lastHeartbeat || 
                      (new Date().getTime() - new Date(station.lastHeartbeat).getTime()) >= 10 * 60 * 1000;
                    
                    const isDisabled = isStationUnavailable || isStationOffline || hasNoChargeBoxId || hasNoAvailablePoints || isLoadingAvailability || (isNotConnected && station.status !== 'Available');
                    
                    let disabledTitle = '';
                    if (isStationUnavailable) {
                      disabledTitle = 'Ladestation ist nicht verfügbar';
                    } else if (isStationOffline) {
                      disabledTitle = 'Ladestation ist offline';
                    } else if (hasNoChargeBoxId) {
                      disabledTitle = 'Ladestation ist nicht mit OCPP verbunden';
                    } else if (isLoadingAvailability) {
                      disabledTitle = 'Prüfe Verfügbarkeit...';
                    } else if (hasNoAvailablePoints) {
                      disabledTitle = 'Keine verfügbaren Ladepunkte';
                    } else if (isNotConnected && station.status !== 'Available') {
                      disabledTitle = 'Ladestation ist nicht verbunden';
                    }
                    
                    return (
                      <Button
                        onClick={() => handleStartClick(station)}
                        className="flex-1 bg-primary hover:bg-primary/90"
                        disabled={isDisabled}
                        title={disabledTitle}
                      >
                        <Play className="h-4 w-4 mr-2" />
                        {isLoadingAvailability ? 'Prüfe...' : 'Laden starten'}
                      </Button>
                    );
                  })()}
                  {station.latitude && station.longitude && (
                    <Button
                      variant="outline"
                      size="icon"
                      onClick={() => openInMaps(station.latitude, station.longitude, station.name)}
                    >
                      <Navigation className="h-4 w-4" />
                    </Button>
                  )}
                </div>

                {/* Last Heartbeat */}
                {station.lastHeartbeat && (
                  <div className="text-xs text-gray-500 dark:text-gray-400 text-center">
                    Letzte Aktivität: {new Date(station.lastHeartbeat).toLocaleString('de-DE')}
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <MapPin className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">Keine Ladestationen verfügbar</h3>
            <p className="text-gray-600 dark:text-gray-400 text-center max-w-md">
              Sie haben derzeit keinen Zugriff auf Ladestationen. Treten Sie einer Nutzergruppe bei, 
              um Zugriff auf Ladestationen zu erhalten.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Start Charging Dialog */}
      <Dialog open={showStartDialog} onOpenChange={setShowStartDialog}>
        <DialogContent className="sm:max-w-[500px] p-5">
          <DialogHeader>
            <DialogTitle>Ladevorgang starten</DialogTitle>
            <DialogDescription>
              Station: {selectedStation?.name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            {error && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <div className="space-y-2">
              <Label htmlFor="chargingPoint">Ladepunkt auswählen*</Label>
              <Select value={selectedChargingPoint} onValueChange={setSelectedChargingPoint}>
                <SelectTrigger>
                  <SelectValue placeholder="Bitte Ladepunkt wählen" />
                </SelectTrigger>
                <SelectContent>
                  {chargingPoints.length === 0 ? (
                    <SelectItem value="none" disabled>Keine Ladepunkte vorhanden</SelectItem>
                  ) : (
                    chargingPoints.map((point) => (
                      <SelectItem 
                        key={point.id} 
                        value={point.id}
                        disabled={!point.isAvailable}
                      >
                        EVSE {point.evseId} - {point.pointName || `Ladepunkt ${point.evseId}`} ({point.type}, {point.maxPower}kW)
                        {!point.isAvailable && ' - Nicht verfügbar'}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>

            {myVehicles.length > 0 && !showQRScanner && (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="vehicle">Fahrzeug (optional)</Label>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setShowQRScanner(true)}
                    className="text-xs"
                  >
                    <QrCode className="h-3 w-3 mr-1" />
                    QR-Code scannen
                  </Button>
                </div>
                
                {scannedVehicle && (
                  <div className="bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800 rounded-lg p-3 mb-2">
                    <p className="text-sm font-medium text-green-900 dark:text-green-100">
                      ✅ Fahrzeug gescannt:
                    </p>
                    <p className="text-sm text-green-700 dark:text-green-300">
                      {scannedVehicle.make} {scannedVehicle.model} ({scannedVehicle.licensePlate})
                    </p>
                  </div>
                )}
                
                <Select value={selectedVehicle || undefined} onValueChange={(value: string) => setSelectedVehicle(value === 'none' ? '' : value)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Kein Fahrzeug auswählen" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Kein Fahrzeug</SelectItem>
                    {myVehicles.map((assignment) => (
                      <SelectItem key={assignment.vehicle.id} value={assignment.vehicle.id}>
                        {assignment.vehicle.make} {assignment.vehicle.model} ({assignment.vehicle.licensePlate})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  Wählen Sie ein Fahrzeug für eine korrekte Kostenzuordnung
                </p>
              </div>
            )}

            {showQRScanner && (
              <div className="space-y-2">
                <Label>QR-Code am Fahrzeug scannen</Label>
                <QRScanner
                  onScan={handleQRScan}
                  onClose={() => setShowQRScanner(false)}
                />
              </div>
            )}

            {chargingPoints.length > 0 && (
              <div className="bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg p-3">
                <h4 className="font-semibold text-sm text-blue-900 dark:text-blue-100 mb-2">
                  Ladepunkte ({chargingPoints.filter((cp: any) => cp.isAvailable).length} verfügbar von {chargingPoints.length}):
                </h4>
                <div className="space-y-2">
                  {chargingPoints.map((point) => (
                    <div key={point.id} className={`flex justify-between items-center text-sm ${!point.isAvailable ? 'opacity-50' : ''}`}>
                      <span className={`${point.isAvailable ? 'text-blue-800 dark:text-blue-200' : 'text-gray-500 dark:text-gray-400'}`}>
                        EVSE {point.evseId} - {point.pointName || `Ladepunkt ${point.evseId}`} ({point.type})
                        {!point.isAvailable && ` - Nicht verfügbar (${point.status})`}
                      </span>
                      {getConnectorStatusBadge(point.status)}
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowStartDialog(false)} disabled={starting}>
              Abbrechen
            </Button>
            <Button
              onClick={handleStartCharging}
              disabled={!selectedChargingPoint || starting || chargingPoints.filter((cp: any) => cp.isAvailable).length === 0}
              title={chargingPoints.filter((cp: any) => cp.isAvailable).length === 0 ? 'Keine verfügbaren Ladepunkte' : !selectedChargingPoint ? 'Bitte wählen Sie einen Ladepunkt' : ''}
            >
              {starting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Wird gestartet...
                </>
              ) : (
                <>
                  <Play className="mr-2 h-4 w-4" />
                  Starten
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};
