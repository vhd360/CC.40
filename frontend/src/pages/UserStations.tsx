import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Loader2, MapPin, Zap, Battery, Building2, Navigation, Play, AlertCircle, CheckCircle, Camera, QrCode } from 'lucide-react';
import { api } from '../services/api';
import { useSignalRContext } from '../contexts/SignalRContext';
import { QRScanner } from '../components/QRScanner';

export const UserStations: React.FC = () => {
  const { isConnected, onStationStatusChanged, onConnectorStatusChanged } = useSignalRContext();
  const [loading, setLoading] = useState(true);
  const [stations, setStations] = useState<any[]>([]);
  const [selectedStation, setSelectedStation] = useState<any | null>(null);
  const [showStartDialog, setShowStartDialog] = useState(false);
  const [connectors, setConnectors] = useState<any[]>([]);
  const [myVehicles, setMyVehicles] = useState<any[]>([]);
  const [selectedConnector, setSelectedConnector] = useState<string>('');
  const [selectedVehicle, setSelectedVehicle] = useState<string>('');
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showQRScanner, setShowQRScanner] = useState(false);
  const [scannedVehicle, setScannedVehicle] = useState<any | null>(null);

  useEffect(() => {
    loadStations();
    loadMyVehicles();
  }, []);

  // SignalR: Station Status Updates
  useEffect(() => {
    if (!isConnected) return;

    const handleStationUpdate = (notification: any) => {
      console.log('📡 Station Status Update received:', notification);
      
      setStations(prevStations => 
        prevStations.map(station => 
          station.id === notification.StationId 
            ? { ...station, status: notification.Status }
            : station
        )
      );
    };

    const unsubscribe = onStationStatusChanged(handleStationUpdate);
    return () => unsubscribe();
  }, [isConnected, onStationStatusChanged]);

  // SignalR: Connector Status Updates
  useEffect(() => {
    if (!isConnected || connectors.length === 0) return;

    const handleConnectorUpdate = (notification: any) => {
      console.log('📡 Connector Status Update received:', notification);
      
      // Update connectors in dialog
      setConnectors(prevConnectors =>
        prevConnectors.map(connector =>
          connector.id === notification.ConnectorId
            ? { ...connector, status: notification.Status, isAvailable: notification.Status === 'Available' }
            : connector
        )
      );
    };

    const unsubscribe = onConnectorStatusChanged(handleConnectorUpdate);
    return () => unsubscribe();
  }, [isConnected, onConnectorStatusChanged, connectors.length]);

  const loadStations = async () => {
    try {
      setLoading(true);
      const data = await api.getUserAvailableStations();
      setStations(data);
    } catch (error) {
      console.error('Failed to load stations:', error);
      setError('Fehler beim Laden der Ladestationen');
    } finally {
      setLoading(false);
    }
  };

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
    setSelectedStation(station);
    try {
      const connectorsData = await api.getStationConnectors(station.id);
      setConnectors(connectorsData);
      setShowStartDialog(true);
      setSelectedConnector('');
      setSelectedVehicle('');
      setShowQRScanner(false);
      setScannedVehicle(null);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Laden der Connectoren');
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
    if (!selectedConnector) {
      setError('Bitte wählen Sie einen Connector');
      return;
    }

    try {
      setStarting(true);
      setError(null);
      await api.startChargingSession(selectedConnector, selectedVehicle || undefined);
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

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'Available': 'bg-green-500',
      'Occupied': 'bg-yellow-500',
      'OutOfOrder': 'bg-red-500',
      'Reserved': 'bg-blue-500',
      'Unavailable': 'bg-gray-500'
    };
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
                  <div className={`w-3 h-3 rounded-full ${getStatusColor(station.status)}`} />
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
                  <Button
                    onClick={() => handleStartClick(station)}
                    className="flex-1 bg-primary hover:bg-primary/90"
                  >
                    <Play className="h-4 w-4 mr-2" />
                    Laden starten
                  </Button>
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
        <DialogContent className="sm:max-w-[500px]">
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
              <Label htmlFor="connector">Connector auswählen*</Label>
              <Select value={selectedConnector} onValueChange={setSelectedConnector}>
                <SelectTrigger>
                  <SelectValue placeholder="Bitte Connector wählen" />
                </SelectTrigger>
                <SelectContent>
                  {connectors.filter(c => c.isAvailable).length === 0 ? (
                    <SelectItem value="none" disabled>Keine verfügbaren Connectoren</SelectItem>
                  ) : (
                    connectors.filter(c => c.isAvailable).map((connector) => (
                      <SelectItem key={connector.id} value={connector.id}>
                        EVSE {connector.evseId} - Connector {connector.connectorId} ({connector.type}, {connector.maxPower}kW)
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

            {connectors.length > 0 && (
              <div className="bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg p-3">
                <h4 className="font-semibold text-sm text-blue-900 dark:text-blue-100 mb-2">Verfügbare Connectoren:</h4>
                <div className="space-y-2">
                  {connectors.map((connector) => (
                    <div key={connector.id} className="flex justify-between items-center text-sm">
                      <span className="text-blue-800 dark:text-blue-200">
                        EVSE {connector.evseId} - Connector {connector.connectorId} ({connector.type})
                      </span>
                      {getConnectorStatusBadge(connector.status)}
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
              disabled={!selectedConnector || starting}
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
