import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { ArrowLeft, Plus, Trash2, Loader2, Zap } from 'lucide-react';
import { api } from '../services/api';

interface GroupDetails {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  stations: Array<{
    chargingStationId: string;
    stationName: string;
    stationId: string;
    status: string;
    addedAt: string;
  }>;
}

export const ChargingStationGroupDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [group, setGroup] = useState<GroupDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [showAddStation, setShowAddStation] = useState(false);
  const [availableStations, setAvailableStations] = useState<any[]>([]);
  const [selectedStationId, setSelectedStationId] = useState('');

  const loadGroupDetails = async () => {
    if (!id) return;
    try {
      setLoading(true);
      const data = await api.getChargingStationGroupDetails(id);
      setGroup(data);
    } catch (error) {
      console.error('Failed to load group details:', error);
      alert('Fehler beim Laden der Gruppendetails');
    } finally {
      setLoading(false);
    }
  };

  const loadAvailableStations = async () => {
    try {
      const stations = await api.getChargingStations();
      // Filter out stations that are already in the group
      const filtered = stations.filter(
        (s: any) => !group?.stations.some(gs => gs.chargingStationId === s.id)
      );
      setAvailableStations(filtered);
    } catch (error) {
      console.error('Failed to load stations:', error);
    }
  };

  useEffect(() => {
    loadGroupDetails();
  }, [id]);

  useEffect(() => {
    if (showAddStation) {
      loadAvailableStations();
    }
  }, [showAddStation, group]);

  const handleAddStation = async () => {
    if (!id || !selectedStationId) return;
    try {
      await api.addStationToGroup(id, selectedStationId);
      setShowAddStation(false);
      setSelectedStationId('');
      loadGroupDetails();
    } catch (error) {
      console.error('Failed to add station:', error);
      alert('Fehler beim Hinzufügen der Ladestation');
    }
  };

  const handleRemoveStation = async (stationId: string) => {
    if (!id) return;
    if (!window.confirm('Möchten Sie diese Ladestation aus der Gruppe entfernen?')) return;
    try {
      await api.removeStationFromGroup(id, stationId);
      loadGroupDetails();
    } catch (error) {
      console.error('Failed to remove station:', error);
      alert('Fehler beim Entfernen der Ladestation');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Gruppendetails...</span>
      </div>
    );
  }

  if (!group) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">Gruppe nicht gefunden</p>
        <Button onClick={() => navigate('/charging-station-groups')} className="mt-4">
          Zurück zur Übersicht
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="outline" onClick={() => navigate('/charging-station-groups')}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Zurück
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl flex items-center">
                <Zap className="h-6 w-6 mr-2 text-yellow-600" />
                {group.name}
              </CardTitle>
              {group.description && (
                <CardDescription className="mt-2">{group.description}</CardDescription>
              )}
            </div>
            <div className={`px-3 py-1 rounded-full text-sm font-medium ${
              group.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
            }`}>
              {group.isActive ? 'Aktiv' : 'Inaktiv'}
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="text-sm text-gray-600">
            Erstellt am: {new Date(group.createdAt).toLocaleDateString('de-DE', {
              day: '2-digit',
              month: '2-digit',
              year: 'numeric',
              hour: '2-digit',
              minute: '2-digit'
            })}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Ladestationen ({group.stations.length})</CardTitle>
            <Button onClick={() => setShowAddStation(!showAddStation)} size="sm">
              <Plus className="h-4 w-4 mr-2" />
              Station hinzufügen
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {showAddStation && (
            <div className="mb-6 p-4 border rounded-lg bg-gray-50">
              <h3 className="font-medium mb-3">Ladestation hinzufügen</h3>
              <div className="flex space-x-2">
                <select
                  value={selectedStationId}
                  onChange={(e) => setSelectedStationId(e.target.value)}
                  className="flex-1 rounded-md border border-input bg-background px-3 py-2"
                >
                  <option value="">Bitte wählen...</option>
                  {availableStations.map((station) => (
                    <option key={station.id} value={station.id}>
                      {station.name} ({station.stationId})
                    </option>
                  ))}
                </select>
                <Button onClick={handleAddStation} disabled={!selectedStationId}>
                  Hinzufügen
                </Button>
                <Button variant="outline" onClick={() => setShowAddStation(false)}>
                  Abbrechen
                </Button>
              </div>
            </div>
          )}

          {group.stations.length > 0 ? (
            <div className="space-y-3">
              {group.stations.map((station) => (
                <div
                  key={station.chargingStationId}
                  className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50"
                >
                  <div className="flex-1">
                    <div className="font-medium">{station.stationName}</div>
                    <div className="text-sm text-gray-600">
                      ID: {station.stationId} • Status: 
                      <span className={`ml-1 font-medium ${
                        station.status === 'available' ? 'text-green-600' : 'text-gray-600'
                      }`}>
                        {station.status}
                      </span>
                    </div>
                    <div className="text-xs text-gray-500 mt-1">
                      Hinzugefügt: {new Date(station.addedAt).toLocaleDateString('de-DE')}
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleRemoveStation(station.chargingStationId)}
                    className="text-red-600 hover:text-red-700"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Zap className="h-12 w-12 mx-auto mb-3 text-gray-300" />
              <p>Keine Ladestationen in dieser Gruppe</p>
              <p className="text-sm mt-1">Fügen Sie Ladestationen hinzu, um zu beginnen</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

