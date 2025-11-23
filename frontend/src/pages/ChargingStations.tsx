import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Zap, Battery, Clock, Loader2, Eye, Building2, Layers, ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { api } from '../services/api';
import { useSignalRContext } from '../contexts/SignalRContext';
import { useToast } from '../components/ui/toast';
import { ConfirmDialog } from '../components/ConfirmDialog';

export const ChargingStations: React.FC = () => {
  const { isConnected, onStationStatusChanged } = useSignalRContext();
  const { showToast } = useToast();
  const [stations, setStations] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [chargingParks, setChargingParks] = useState<any[]>([]);
  const [deleteConfirm, setDeleteConfirm] = useState<{ open: boolean; stationId: string | null; stationName: string }>({
    open: false,
    stationId: null,
    stationName: ''
  });
  const [formData, setFormData] = useState({
    chargingParkId: '',
    stationId: '',
    name: '',
    vendor: '',
    model: '',
    type: '0', // 0 = AC, 1 = DC
    maxPower: '',
    numberOfConnectors: '1'
  });
  const navigate = useNavigate();

  useEffect(() => {
    loadStations();
    loadChargingParks();
    
    // Aktualisiere Stationsdaten alle 30 Sekunden
    const interval = setInterval(() => {
      loadStations();
    }, 30000);
    
    return () => clearInterval(interval);
  }, []);

  // SignalR: Station Status Updates
  useEffect(() => {
    if (!isConnected) return;

    const handleStationUpdate = (notification: any) => {
      console.log('📡 [Admin] Station Status Update received:', notification);
      
      setStations(prevStations => 
        prevStations.map(station => 
          station.id === notification.StationId 
            ? { 
                ...station, 
                status: notification.Status,
                lastHeartbeat: new Date().toISOString() // Aktualisiere lastHeartbeat bei Status-Updates
              }
            : station
        )
      );
    };

    const unsubscribe = onStationStatusChanged(handleStationUpdate);
    return () => unsubscribe();
  }, [isConnected, onStationStatusChanged]);

  const loadStations = async () => {
    try {
      setLoading(true);
      const data = await api.getChargingStations();
      setStations(data);
    } catch (error) {
      console.error('Failed to load charging stations:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadChargingParks = async () => {
    try {
      const parks = await api.getChargingParks();
      setChargingParks(parks);
    } catch (error) {
      console.error('Failed to load charging parks:', error);
    }
  };

  const handleCreateStation = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await api.createChargingStation({
        chargingParkId: formData.chargingParkId,
        stationId: formData.stationId,
        name: formData.name,
        vendor: formData.vendor,
        model: formData.model,
        type: parseInt(formData.type),
        maxPower: parseInt(formData.maxPower),
        numberOfConnectors: parseInt(formData.numberOfConnectors)
      });

      showToast('Ladestation erfolgreich erstellt!', 'success');
      setShowForm(false);
      setFormData({
        chargingParkId: '',
        stationId: '',
        name: '',
        vendor: '',
        model: '',
        type: '0',
        maxPower: '',
        numberOfConnectors: '1'
      });
      loadStations();
    } catch (error: any) {
      console.error('Failed to create charging station:', error);
      showToast(error.message || 'Fehler beim Erstellen der Ladestation', 'error');
    }
  };

  const handleDeleteClick = (id: string, name: string) => {
    setDeleteConfirm({
      open: true,
      stationId: id,
      stationName: name
    });
  };

  const handleDeleteConfirm = async () => {
    if (!deleteConfirm.stationId) return;
    
    try {
      await api.deleteChargingStation(deleteConfirm.stationId);
      showToast('Ladestation erfolgreich gelöscht', 'success');
      loadStations();
    } catch (error: any) {
      console.error('Failed to delete charging station:', error);
      showToast(error.message || 'Fehler beim Löschen der Ladestation', 'error');
    } finally {
      setDeleteConfirm({ open: false, stationId: null, stationName: '' });
    }
  };

  const statusColors: Record<string, string> = {
    'Available': 'bg-green-500',
    'Occupied': 'bg-yellow-500',
    'OutOfOrder': 'bg-red-500',
    'Reserved': 'bg-blue-500',
    'Unavailable': 'bg-gray-500',
    'Offline': 'bg-gray-400'
  };

  // Hilfsfunktion: Prüft ob Station online ist (Heartbeat innerhalb der letzten 10 Minuten)
  const isStationOnline = (station: any): boolean => {
    if (!station.lastHeartbeat) {
      console.log(`Station ${station.name}: Kein lastHeartbeat`);
      return false;
    }
    
    const now = new Date();
    const lastHeartbeat = new Date(station.lastHeartbeat);
    const timeSinceHeartbeat = now.getTime() - lastHeartbeat.getTime();
    const minutesSince = timeSinceHeartbeat / (60 * 1000);
    
    console.log(`Station ${station.name}:`, {
      now: now.toISOString(),
      lastHeartbeat: lastHeartbeat.toISOString(),
      minutesSince: minutesSince.toFixed(2),
      isOnline: timeSinceHeartbeat < 10 * 60 * 1000
    });
    
    return timeSinceHeartbeat < 10 * 60 * 1000; // 10 Minuten
  };

  // Hilfsfunktion: Bestimmt den tatsächlichen anzuzeigenden Status
  const getDisplayStatus = (station: any): string => {
    if (!isStationOnline(station)) return 'Offline';
    return station.status;
  };

  // Show create form
  if (showForm) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => setShowForm(false)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Neue Ladestation</h1>
            <p className="text-gray-600 dark:text-gray-400 mt-1">Fügen Sie eine neue Ladestation hinzu</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Ladestation erstellen</CardTitle>
            <CardDescription>
              Geben Sie die Details der neuen Ladestation ein
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleCreateStation} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="chargingParkId">Ladepark *</Label>
                <select
                  id="chargingParkId"
                  value={formData.chargingParkId}
                  onChange={(e) => setFormData({ ...formData, chargingParkId: e.target.value })}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  required
                >
                  <option value="">Bitte wählen...</option>
                  {chargingParks.map((park) => (
                    <option key={park.id} value={park.id}>
                      {park.name}
                    </option>
                  ))}
                </select>
                {chargingParks.length === 0 && (
                  <p className="text-xs text-red-500">
                    Bitte erstellen Sie zuerst einen Ladepark!
                  </p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="stationId">Stations-ID *</Label>
                  <Input
                    id="stationId"
                    value={formData.stationId}
                    onChange={(e) => setFormData({ ...formData, stationId: e.target.value })}
                    placeholder="z.B. ST-001"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">Name *</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    placeholder="z.B. Ladestation 1"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="vendor">Hersteller *</Label>
                  <Input
                    id="vendor"
                    value={formData.vendor}
                    onChange={(e) => setFormData({ ...formData, vendor: e.target.value })}
                    placeholder="z.B. ABB, Alfen, ..."
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="model">Modell *</Label>
                  <Input
                    id="model"
                    value={formData.model}
                    onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                    placeholder="z.B. Terra AC"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="type">Typ *</Label>
                  <select
                    id="type"
                    value={formData.type}
                    onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                    className="w-full rounded-md border border-input bg-background px-3 py-2"
                    required
                  >
                    <option value="0">AC</option>
                    <option value="1">DC</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="maxPower">Max. Leistung (kW) *</Label>
                  <Input
                    id="maxPower"
                    type="number"
                    value={formData.maxPower}
                    onChange={(e) => setFormData({ ...formData, maxPower: e.target.value })}
                    placeholder="z.B. 22"
                    required
                    min="1"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="numberOfConnectors">Anzahl Stecker *</Label>
                  <Input
                    id="numberOfConnectors"
                    type="number"
                    value={formData.numberOfConnectors}
                    onChange={(e) => setFormData({ ...formData, numberOfConnectors: e.target.value })}
                    required
                    min="1"
                  />
                </div>
              </div>

              <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
                <p className="text-sm text-blue-800 dark:text-blue-300">
                  <strong>Hinweis:</strong> Weitere Einstellungen (OCPP, Standort, etc.) können später in den Details bearbeitet werden.
                </p>
              </div>

              <div className="flex justify-end space-x-2 pt-4">
                <Button type="button" variant="outline" onClick={() => setShowForm(false)}>
                  Abbrechen
                </Button>
                <Button type="submit" disabled={chargingParks.length === 0}>
                  <Plus className="h-4 w-4 mr-2" />
                  Ladestation erstellen
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Ladestationen</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Verwalten Sie Ihre Ladestationen und deren Konfiguration</p>
        </div>
        <Button className="flex items-center space-x-2" onClick={() => setShowForm(true)}>
          <Zap className="h-4 w-4" />
          <span>Neue Ladestation</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600 dark:text-blue-400" />
          <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Ladestationen...</span>
        </div>
      ) : stations.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {stations.map((station) => (
            <Card key={station.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">{station.name}</CardTitle>
                  <div className="flex items-center space-x-2">
                    <div className={`w-3 h-3 rounded-full ${statusColors[getDisplayStatus(station)] || 'bg-gray-500'}`} />
                    <span className="text-xs text-gray-500 dark:text-gray-400">{getDisplayStatus(station)}</span>
                  </div>
                </div>
                <CardDescription className="text-sm">
                  ID: {station.stationId}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Offline Warning */}
                {!isStationOnline(station) && (
                  <div className="flex items-center space-x-2 p-2 bg-gray-100 dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded">
                    <div className="flex items-center space-x-2 text-gray-700 dark:text-gray-300">
                      <span className="text-sm font-medium">⚠️ Offline</span>
                      {station.lastHeartbeat ? (
                        <span className="text-xs">
                          (Letzter Heartbeat: {new Date(station.lastHeartbeat).toLocaleString('de-DE')})
                        </span>
                      ) : (
                        <span className="text-xs">(Kein Heartbeat)</span>
                      )}
                    </div>
                  </div>
                )}

                {/* Ladepark Info */}
                <div className="flex items-center space-x-2 p-2 bg-blue-50 dark:bg-blue-900/20 rounded">
                  <Building2 className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                  <div className="text-sm">
                    <span className="font-medium dark:text-gray-200">{station.chargingPark.name}</span>
                  </div>
                </div>

                {/* Ladepunkt-Gruppen */}
                {station.groups && station.groups.length > 0 && (
                  <div className="flex items-start space-x-2 p-2 bg-indigo-50 dark:bg-indigo-900/20 rounded">
                    <Layers className="h-4 w-4 text-indigo-600 dark:text-indigo-400 mt-0.5" />
                    <div className="text-xs">
                      <div className="font-medium text-indigo-900 dark:text-indigo-300 mb-1">Gruppen:</div>
                      {station.groups.map((group: any, idx: number) => (
                        <span key={group.id} className="text-indigo-700 dark:text-indigo-400">
                          {group.name}{idx < station.groups.length - 1 ? ', ' : ''}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                <div className="grid grid-cols-2 gap-4">
                  <div className="flex items-center space-x-2">
                    <Battery className="h-4 w-4 text-blue-500" />
                    <span className="text-sm">{station.maxPower}kW</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Clock className="h-4 w-4 text-green-500" />
                    <span className="text-sm">{station.numberOfConnectors} Stecker</span>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-gray-600 dark:text-gray-400">Hersteller:</span>
                    <div className="font-medium dark:text-gray-200">{station.vendor}</div>
                  </div>
                  <div>
                    <span className="text-gray-600 dark:text-gray-400">Typ:</span>
                    <div className="font-medium dark:text-gray-200">{station.type}</div>
                  </div>
                </div>

                {/* OCPP Info */}
                {station.chargeBoxId && (
                  <div className="text-xs bg-gray-50 dark:bg-gray-800 p-2 rounded">
                    <span className="text-gray-600 dark:text-gray-400">ChargeBox-ID: </span>
                    <span className="font-mono font-medium dark:text-gray-200">{station.chargeBoxId}</span>
                    {station.ocppProtocol && (
                      <span className="ml-2 text-gray-500 dark:text-gray-400">({station.ocppProtocol})</span>
                    )}
                  </div>
                )}

                {station.lastHeartbeat && (
                  <div className="text-xs text-gray-500 dark:text-gray-400">
                    Letzte Aktivität: {new Date(station.lastHeartbeat).toLocaleString('de-DE')}
                  </div>
                )}

                <div className="flex space-x-2 pt-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => navigate(`/charging-stations/${station.id}`)}
                  >
                    <Eye className="h-4 w-4 mr-1" />
                    Details
                  </Button>
                  <Button 
                    variant="outline" 
                    size="sm"
                    onClick={() => handleDeleteClick(station.id, station.name)}
                    className="text-red-600 hover:text-red-700 hover:bg-red-50"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Zap className="h-16 w-16 text-gray-300 dark:text-gray-600 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">Keine Ladestationen vorhanden</h3>
            <p className="text-gray-600 dark:text-gray-400 mb-4">Fügen Sie Ihre erste Ladestation hinzu</p>
            <Button onClick={() => setShowForm(true)}>
              <Zap className="h-4 w-4 mr-2" />
              Ladestation hinzufügen
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={deleteConfirm.open}
        onOpenChange={(open) => setDeleteConfirm({ ...deleteConfirm, open })}
        title="Ladestation löschen"
        message={`Möchten Sie die Ladestation "${deleteConfirm.stationName}" wirklich löschen? Alle Ladepunkte werden ebenfalls deaktiviert. Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteConfirm}
      />
    </div>
  );
};
