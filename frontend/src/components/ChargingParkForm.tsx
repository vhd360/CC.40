import React, { useState, useEffect } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Dialog, DialogContent } from './ui/dialog';
import { ChargingStationForm, ChargingStationFormData } from './ChargingStationForm';
import { api } from '../services/api';
import { useToast } from './ui/toast';
import { Plus, X, Zap, Trash2 } from 'lucide-react';

interface ChargingParkFormProps {
  park?: any;
  onSubmit: (data: ChargingParkFormData) => void;
  onCancel: () => void;
}

export interface ChargingParkFormData {
  name: string;
  description?: string;
  address: string;
  postalCode: string;
  city: string;
  country: string;
  latitude?: number;
  longitude?: number;
}

export const ChargingParkForm: React.FC<ChargingParkFormProps> = ({ park, onSubmit, onCancel }) => {
  const { showToast } = useToast();
  const [formData, setFormData] = useState<ChargingParkFormData>({
    name: park?.name || '',
    description: park?.description || '',
    address: park?.address || '',
    postalCode: park?.postalCode || '',
    city: park?.city || '',
    country: park?.country || 'Deutschland',
    latitude: park?.latitude || undefined,
    longitude: park?.longitude || undefined
  });

  const [stations, setStations] = useState<any[]>([]);
  const [unassignedStations, setUnassignedStations] = useState<any[]>([]);
  const [selectedUnassignedStations, setSelectedUnassignedStations] = useState<string[]>([]);
  const [showAddStationDialog, setShowAddStationDialog] = useState(false);
  const [showSelectStationsDialog, setShowSelectStationsDialog] = useState(false);
  const [loadingStations, setLoadingStations] = useState(false);

  useEffect(() => {
    if (park?.id) {
      loadParkStations();
      loadUnassignedStations();
    }
  }, [park?.id]);

  const loadParkStations = async () => {
    if (!park?.id) return;
    try {
      setLoadingStations(true);
      const parkData = await api.getChargingPark(park.id);
      setStations(parkData.stations || []);
    } catch (error) {
      console.error('Failed to load park stations:', error);
    } finally {
      setLoadingStations(false);
    }
  };

  const loadUnassignedStations = async () => {
    try {
      const data = await api.getUnassignedChargingStations();
      setUnassignedStations(data);
    } catch (error) {
      console.error('Failed to load unassigned stations:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Wenn Park bearbeitet wird und Stationen ausgewählt wurden, diese zuordnen
    if (park?.id && selectedUnassignedStations.length > 0) {
      try {
        await api.assignStationsToPark(park.id, selectedUnassignedStations);
        showToast(`${selectedUnassignedStations.length} Station(en) erfolgreich zugeordnet`, 'success');
        setSelectedUnassignedStations([]);
        await loadParkStations();
        await loadUnassignedStations();
      } catch (error) {
        console.error('Failed to assign stations:', error);
        showToast('Fehler beim Zuordnen der Stationen', 'error');
      }
    }
    
    onSubmit(formData);
  };

  const handleAddStation = async (stationData: ChargingStationFormData) => {
    if (!park?.id) {
      showToast('Bitte speichern Sie zuerst den Ladepark', 'error');
      return;
    }

    try {
      await api.createChargingStation({
        chargingParkId: park.id,
        stationId: stationData.stationId,
        name: stationData.name,
        vendor: stationData.vendor || '',
        model: stationData.model || '',
        type: stationData.type === 'AC' ? 0 : stationData.type === 'DC' ? 1 : 0,
        maxPower: stationData.maxPower,
        numberOfConnectors: stationData.numberOfConnectors,
        latitude: stationData.latitude,
        longitude: stationData.longitude,
        notes: stationData.notes
      });
      showToast('Ladestation erfolgreich erstellt', 'success');
      setShowAddStationDialog(false);
      await loadParkStations();
      await loadUnassignedStations();
    } catch (error) {
      console.error('Failed to create station:', error);
      showToast('Fehler beim Erstellen der Ladestation', 'error');
    }
  };

  const handleSelectStations = () => {
    if (selectedUnassignedStations.length === 0) {
      showToast('Bitte wählen Sie mindestens eine Station aus', 'error');
      return;
    }
    setShowSelectStationsDialog(false);
    // Die Zuordnung erfolgt beim Speichern des Parks
  };

  const handleRemoveStation = async (stationId: string) => {
    // Hier könnte man die Station aus dem Park entfernen
    // Da ChargingParkId required ist, müsste man die Station löschen oder einem anderen Park zuordnen
    showToast('Station kann nicht entfernt werden, da sie einem Park zugeordnet sein muss', 'error');
  };

  return (
    <>
      <Card className="w-full max-w-3xl">
        <CardHeader>
          <CardTitle>{park ? 'Ladepark bearbeiten' : 'Neuer Ladepark anlegen'}</CardTitle>
          <CardDescription>
            {park ? 'Aktualisieren Sie die Ladepark-Daten' : 'Geben Sie die Details für den neuen Ladepark ein'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                placeholder="z.B. Ladepark München Süd"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Beschreibung</Label>
              <textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
                placeholder="Zusätzliche Informationen zum Ladepark..."
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="col-span-2 space-y-2">
                <Label htmlFor="address">Adresse *</Label>
                <Input
                  id="address"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  required
                  placeholder="Straße und Hausnummer"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="postalCode">PLZ *</Label>
                <Input
                  id="postalCode"
                  value={formData.postalCode}
                  onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                  required
                  placeholder="80331"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="city">Stadt *</Label>
                <Input
                  id="city"
                  value={formData.city}
                  onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                  required
                  placeholder="München"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="country">Land *</Label>
              <Input
                id="country"
                value={formData.country}
                onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="latitude">Breitengrad</Label>
                <Input
                  id="latitude"
                  type="number"
                  step="0.000001"
                  value={formData.latitude || ''}
                  onChange={(e) => setFormData({ ...formData, latitude: e.target.value ? parseFloat(e.target.value) : undefined })}
                  placeholder="48.1351"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="longitude">Längengrad</Label>
                <Input
                  id="longitude"
                  type="number"
                  step="0.000001"
                  value={formData.longitude || ''}
                  onChange={(e) => setFormData({ ...formData, longitude: e.target.value ? parseFloat(e.target.value) : undefined })}
                  placeholder="11.5820"
                />
              </div>
            </div>

            {/* Ladestationen-Verwaltung (nur beim Bearbeiten) */}
            {park?.id && (
              <div className="border-t pt-4 mt-4">
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h3 className="text-lg font-medium">Ladestationen</h3>
                    <p className="text-sm text-gray-500">Verwalten Sie die Ladestationen dieses Ladeparks</p>
                  </div>
                  <div className="flex space-x-2">
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setShowSelectStationsDialog(true)}
                      disabled={unassignedStations.length === 0}
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Bestehende zuordnen ({unassignedStations.length})
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setShowAddStationDialog(true)}
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Neue erstellen
                    </Button>
                  </div>
                </div>

                {loadingStations ? (
                  <div className="text-center py-4 text-gray-500">Lade Stationen...</div>
                ) : stations.length === 0 ? (
                  <div className="text-center py-4 text-gray-500">
                    Noch keine Ladestationen zugeordnet
                  </div>
                ) : (
                  <div className="space-y-2">
                    {stations.map((station: any) => (
                      <div
                        key={station.id}
                        className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800"
                      >
                        <div className="flex items-center space-x-3">
                          <Zap className="h-5 w-5 text-blue-600" />
                          <div>
                            <div className="font-medium">{station.name}</div>
                            <div className="text-sm text-gray-500">
                              {station.stationId} • {station.vendor} {station.model}
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2">
                          <span className={`px-2 py-1 rounded-full text-xs ${
                            station.status === 'Available' ? 'bg-green-100 text-green-800' :
                            station.status === 'Offline' ? 'bg-gray-100 text-gray-600' :
                            'bg-yellow-100 text-yellow-800'
                          }`}>
                            {station.status}
                          </span>
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => handleRemoveStation(station.id)}
                            className="text-red-600 hover:text-red-700"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}

            <div className="flex justify-end space-x-2 pt-4">
              <Button type="button" variant="outline" onClick={onCancel}>
                Abbrechen
              </Button>
              <Button type="submit">
                {park ? 'Speichern' : 'Ladepark anlegen'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {/* Dialog: Neue Station erstellen */}
      <Dialog open={showAddStationDialog} onOpenChange={setShowAddStationDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <ChargingStationForm
            station={undefined}
            chargingParkId={park?.id}
            onSubmit={handleAddStation}
            onCancel={() => setShowAddStationDialog(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Dialog: Bestehende Stationen auswählen */}
      <Dialog open={showSelectStationsDialog} onOpenChange={setShowSelectStationsDialog}>
        <DialogContent className="max-w-2xl">
          <div className="space-y-4">
            <div>
              <h3 className="text-lg font-medium mb-2">Bestehende Ladestationen zuordnen</h3>
              <p className="text-sm text-gray-500">
                Wählen Sie Ladestationen aus, die diesem Ladepark zugeordnet werden sollen
              </p>
            </div>

            {unassignedStations.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                Keine unzugeordneten Ladestationen verfügbar
              </div>
            ) : (
              <div className="space-y-2 max-h-96 overflow-y-auto">
                {unassignedStations.map((station: any) => (
                  <label
                    key={station.id}
                    className="flex items-center space-x-3 p-3 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedUnassignedStations.includes(station.id)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedUnassignedStations([...selectedUnassignedStations, station.id]);
                        } else {
                          setSelectedUnassignedStations(selectedUnassignedStations.filter(id => id !== station.id));
                        }
                      }}
                      className="rounded border-gray-300"
                    />
                    <Zap className="h-5 w-5 text-blue-600" />
                    <div className="flex-1">
                      <div className="font-medium">{station.name}</div>
                      <div className="text-sm text-gray-500">
                        {station.stationId} • {station.vendor} {station.model} • {station.maxPower}kW
                      </div>
                    </div>
                    <span className={`px-2 py-1 rounded-full text-xs ${
                      station.status === 'Available' ? 'bg-green-100 text-green-800' :
                      station.status === 'Offline' ? 'bg-gray-100 text-gray-600' :
                      'bg-yellow-100 text-yellow-800'
                    }`}>
                      {station.status}
                    </span>
                  </label>
                ))}
              </div>
            )}

            <div className="flex justify-end space-x-2 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setShowSelectStationsDialog(false);
                  setSelectedUnassignedStations([]);
                }}
              >
                Abbrechen
              </Button>
              <Button
                type="button"
                onClick={handleSelectStations}
                disabled={selectedUnassignedStations.length === 0}
              >
                {selectedUnassignedStations.length} Station(en) zuordnen
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
