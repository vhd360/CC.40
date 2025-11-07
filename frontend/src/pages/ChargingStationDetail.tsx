import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Dialog, DialogContent } from '../components/ui/dialog';
import { ChargingPointForm, ChargingPointFormData } from '../components/ChargingPointForm';
import { ConnectorForm, ConnectorFormData } from '../components/ConnectorForm';
import { 
  Loader2, 
  ArrowLeft, 
  Edit, 
  Save, 
  X, 
  Zap, 
  MapPin, 
  Server, 
  Key,
  Layers,
  Plus,
  Trash2
} from 'lucide-react';
import { api } from '../services/api';

export const ChargingStationDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [station, setStation] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState<any>({});
  
  // Dialog states
  const [showChargingPointDialog, setShowChargingPointDialog] = useState(false);
  const [showConnectorDialog, setShowConnectorDialog] = useState(false);
  const [selectedChargingPoint, setSelectedChargingPoint] = useState<any>(null);
  const [editingChargingPoint, setEditingChargingPoint] = useState<any>(null);
  const [editingConnector, setEditingConnector] = useState<any>(null);

  const loadStation = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const response = await fetch(`http://localhost:5126/api/charging-stations/${id}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      if (!response.ok) throw new Error('Failed to fetch station');
      const data = await response.json();
      setStation(data);
      setFormData({
        stationId: data.stationId,
        name: data.name,
        vendor: data.vendor,
        model: data.model,
        type: data.type === 'AC' ? 0 : 1,
        maxPower: data.maxPower,
        numberOfConnectors: data.numberOfConnectors,
        status: ['Available', 'Occupied', 'OutOfOrder', 'Reserved', 'Unavailable'].indexOf(data.status),
        latitude: data.latitude || '',
        longitude: data.longitude || '',
        notes: data.notes || '',
        chargeBoxId: data.chargeBoxId || '',
        ocppPassword: data.ocppPassword || '',
        ocppProtocol: data.ocppProtocol || 'OCPP16',
        ocppEndpoint: data.ocppEndpoint || '',
        isActive: data.isActive
      });
    } catch (error) {
      console.error('Failed to load station:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStation();
  }, [id]);

  const handleSave = async () => {
    try {
      const token = localStorage.getItem('token');
      
      // Convert formData to match backend DTO expectations (PascalCase, Enum integers)
      const dto = {
        StationId: formData.stationId,
        Name: formData.name,
        Vendor: formData.vendor,
        Model: formData.model,
        Type: parseInt(formData.type), // 0 = AC, 1 = DC
        MaxPower: parseInt(formData.maxPower),
        NumberOfConnectors: parseInt(formData.numberOfConnectors),
        Status: parseInt(formData.status), // 0 = Available, 1 = Occupied, etc.
        Latitude: formData.latitude ? parseFloat(formData.latitude) : null,
        Longitude: formData.longitude ? parseFloat(formData.longitude) : null,
        Notes: formData.notes || null,
        ChargeBoxId: formData.chargeBoxId || null,
        OcppPassword: formData.ocppPassword || null,
        OcppProtocol: formData.ocppProtocol || null,
        OcppEndpoint: formData.ocppEndpoint || null,
        IsActive: formData.isActive
      };
      
      const response = await fetch(`http://localhost:5126/api/charging-stations/${id}`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(dto),
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error('Failed to update station');
      }
      
      setIsEditing(false);
      loadStation();
    } catch (error) {
      console.error('Failed to save station:', error);
      alert('Fehler beim Speichern der Ladestation');
    }
  };

  const handleAddToGroup = async (groupId: string) => {
    try {
      const response = await fetch(
        `http://localhost:5126/api/charging-station-groups/${groupId}/stations/${id}`,
        { method: 'POST' }
      );
      if (!response.ok) throw new Error('Failed to add station to group');
      loadStation();
    } catch (error) {
      console.error('Failed to add to group:', error);
      alert('Fehler beim Hinzufügen zur Gruppe');
    }
  };

  const handleRemoveFromGroup = async (groupId: string) => {
    if (!window.confirm('Möchten Sie diese Ladestation wirklich aus der Gruppe entfernen?')) return;
    try {
      const response = await fetch(
        `http://localhost:5126/api/charging-station-groups/${groupId}/stations/${id}`,
        { method: 'DELETE' }
      );
      if (!response.ok) throw new Error('Failed to remove station from group');
      loadStation();
    } catch (error) {
      console.error('Failed to remove from group:', error);
      alert('Fehler beim Entfernen aus der Gruppe');
    }
  };

  // ChargingPoint handlers
  const handleAddChargingPoint = () => {
    setEditingChargingPoint(null);
    setShowChargingPointDialog(true);
  };

  const handleEditChargingPoint = (point: any) => {
    setEditingChargingPoint(point);
    setShowChargingPointDialog(true);
  };

  const handleSubmitChargingPoint = async (data: ChargingPointFormData) => {
    try {
      const token = localStorage.getItem('token');
      const url = editingChargingPoint
        ? `http://localhost:5126/api/charging-points/${editingChargingPoint.id}`
        : `http://localhost:5126/api/charging-points`;
      
      const response = await fetch(url, {
        method: editingChargingPoint ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
      });

      if (!response.ok) throw new Error('Failed to save charging point');
      
      setShowChargingPointDialog(false);
      setEditingChargingPoint(null);
      loadStation();
    } catch (error) {
      console.error('Failed to save charging point:', error);
      alert('Fehler beim Speichern des Ladepunkts');
    }
  };

  const handleDeleteChargingPoint = async (pointId: string) => {
    if (!window.confirm('Möchten Sie diesen Ladepunkt wirklich löschen?')) return;
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`http://localhost:5126/api/charging-points/${pointId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!response.ok) throw new Error('Failed to delete charging point');
      loadStation();
    } catch (error) {
      console.error('Failed to delete charging point:', error);
      alert('Fehler beim Löschen des Ladepunkts');
    }
  };

  // Connector handlers
  const handleAddConnector = (chargingPoint: any) => {
    setSelectedChargingPoint(chargingPoint);
    setEditingConnector(null);
    setShowConnectorDialog(true);
  };

  const handleEditConnector = (chargingPoint: any, connector: any) => {
    setSelectedChargingPoint(chargingPoint);
    setEditingConnector(connector);
    setShowConnectorDialog(true);
  };

  const handleSubmitConnector = async (data: ConnectorFormData) => {
    try {
      const token = localStorage.getItem('token');
      const url = editingConnector
        ? `http://localhost:5126/api/connectors/${editingConnector.id}`
        : `http://localhost:5126/api/connectors`;
      
      const response = await fetch(url, {
        method: editingConnector ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
      });

      if (!response.ok) throw new Error('Failed to save connector');
      
      setShowConnectorDialog(false);
      setEditingConnector(null);
      setSelectedChargingPoint(null);
      loadStation();
    } catch (error) {
      console.error('Failed to save connector:', error);
      alert('Fehler beim Speichern des Steckers');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Ladestation...</span>
      </div>
    );
  }

  if (!station) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900">Ladestation nicht gefunden</h2>
        <Button onClick={() => navigate('/charging-stations')} className="mt-4">
          Zurück zur Übersicht
        </Button>
      </div>
    );
  }

  const statusColors: Record<string, string> = {
    'Available': 'bg-green-100 text-green-800',
    'Occupied': 'bg-yellow-100 text-yellow-800',
    'OutOfOrder': 'bg-red-100 text-red-800',
    'Reserved': 'bg-blue-100 text-blue-800',
    'Unavailable': 'bg-gray-100 text-gray-800'
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => navigate('/charging-stations')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Zurück
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{station.name}</h1>
            <p className="text-gray-600 mt-1">ID: {station.stationId}</p>
          </div>
        </div>
        {!isEditing ? (
          <Button onClick={() => setIsEditing(true)}>
            <Edit className="h-4 w-4 mr-2" />
            Bearbeiten
          </Button>
        ) : (
          <div className="flex space-x-2">
            <Button onClick={handleSave}>
              <Save className="h-4 w-4 mr-2" />
              Speichern
            </Button>
            <Button variant="outline" onClick={() => {
              setIsEditing(false);
              loadStation();
            }}>
              <X className="h-4 w-4 mr-2" />
              Abbrechen
            </Button>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Grunddaten */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Zap className="h-5 w-5 mr-2 text-blue-600" />
              Grunddaten
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {isEditing ? (
              <>
                <div className="space-y-2">
                  <Label htmlFor="stationId">Stations-ID *</Label>
                  <Input
                    id="stationId"
                    value={formData.stationId}
                    onChange={(e) => setFormData({ ...formData, stationId: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">Name *</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="vendor">Hersteller *</Label>
                  <Input
                    id="vendor"
                    value={formData.vendor}
                    onChange={(e) => setFormData({ ...formData, vendor: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="model">Modell *</Label>
                  <Input
                    id="model"
                    value={formData.model}
                    onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="type">Typ *</Label>
                  <select
                    id="type"
                    value={formData.type}
                    onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) })}
                    className="w-full rounded-md border border-input bg-background px-3 py-2"
                  >
                    <option value="0">AC (Wechselstrom)</option>
                    <option value="1">DC (Gleichstrom)</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="maxPower">Maximale Leistung (kW) *</Label>
                  <Input
                    id="maxPower"
                    type="number"
                    value={formData.maxPower}
                    onChange={(e) => setFormData({ ...formData, maxPower: parseInt(e.target.value) })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <select
                    id="status"
                    value={formData.status}
                    onChange={(e) => setFormData({ ...formData, status: parseInt(e.target.value) })}
                    className="w-full rounded-md border border-input bg-background px-3 py-2"
                  >
                    <option value="0">Verfügbar</option>
                    <option value="1">Belegt</option>
                    <option value="2">Außer Betrieb</option>
                    <option value="3">Reserviert</option>
                    <option value="4">Nicht verfügbar</option>
                  </select>
                </div>
              </>
            ) : (
              <>
                <div>
                  <span className="text-sm text-gray-600">Status</span>
                  <div className="mt-1">
                    <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${statusColors[station.status]}`}>
                      {station.status}
                    </span>
                  </div>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Hersteller</span>
                  <div className="text-sm font-medium mt-1">{station.vendor}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Modell</span>
                  <div className="text-sm font-medium mt-1">{station.model}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Typ</span>
                  <div className="text-sm font-medium mt-1">{station.type}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Maximale Leistung</span>
                  <div className="text-sm font-medium mt-1">{station.maxPower} kW</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Anzahl Anschlüsse</span>
                  <div className="text-sm font-medium mt-1">{station.numberOfConnectors}</div>
                </div>
                {station.lastHeartbeat && (
                  <div>
                    <span className="text-sm text-gray-600">Letzter Heartbeat</span>
                    <div className="text-sm font-medium mt-1">
                      {new Date(station.lastHeartbeat).toLocaleString('de-DE')}
                    </div>
                  </div>
                )}
              </>
            )}
          </CardContent>
        </Card>

        {/* OCPP-Konfiguration */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Server className="h-5 w-5 mr-2 text-purple-600" />
              OCPP-Konfiguration
            </CardTitle>
            <CardDescription>
              Konfiguration für die Verbindung mit dem OCPP-Server
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {isEditing ? (
              <>
                <div className="space-y-2">
                  <Label htmlFor="chargeBoxId">
                    <Key className="h-4 w-4 inline mr-1" />
                    ChargeBox-ID
                  </Label>
                  <Input
                    id="chargeBoxId"
                    value={formData.chargeBoxId}
                    onChange={(e) => setFormData({ ...formData, chargeBoxId: e.target.value })}
                    placeholder="z.B. CP001, STATION-001"
                  />
                  <p className="text-xs text-gray-500">
                    Eindeutige ID für OCPP-Authentifizierung
                  </p>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="ocppPassword">OCPP-Passwort</Label>
                  <Input
                    id="ocppPassword"
                    type="password"
                    value={formData.ocppPassword}
                    onChange={(e) => setFormData({ ...formData, ocppPassword: e.target.value })}
                    placeholder="Sicheres Passwort"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="ocppProtocol">OCPP-Protokoll</Label>
                  <select
                    id="ocppProtocol"
                    value={formData.ocppProtocol}
                    onChange={(e) => setFormData({ ...formData, ocppProtocol: e.target.value })}
                    className="w-full rounded-md border border-input bg-background px-3 py-2"
                  >
                    <option value="OCPP15">OCPP 1.5</option>
                    <option value="OCPP16">OCPP 1.6</option>
                    <option value="OCPP20">OCPP 2.0</option>
                    <option value="OCPP201">OCPP 2.0.1</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="ocppEndpoint">OCPP-Server Endpoint</Label>
                  <Input
                    id="ocppEndpoint"
                    value={formData.ocppEndpoint}
                    onChange={(e) => setFormData({ ...formData, ocppEndpoint: e.target.value })}
                    placeholder="ws://localhost:8080/ocpp oder wss://..."
                  />
                  <p className="text-xs text-gray-500">
                    WebSocket-URL des OCPP-Servers
                  </p>
                </div>
              </>
            ) : (
              <>
                {station.chargeBoxId ? (
                  <>
                    <div>
                      <span className="text-sm text-gray-600">ChargeBox-ID</span>
                      <div className="text-sm font-mono bg-gray-100 px-2 py-1 rounded mt-1">
                        {station.chargeBoxId}
                      </div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600">Passwort</span>
                      <div className="text-sm font-medium mt-1">
                        {station.ocppPassword ? '••••••••' : 'Nicht gesetzt'}
                      </div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600">Protokoll</span>
                      <div className="text-sm font-medium mt-1">{station.ocppProtocol || 'Nicht konfiguriert'}</div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600">Server-Endpoint</span>
                      <div className="text-sm font-mono bg-gray-100 px-2 py-1 rounded mt-1 break-all">
                        {station.ocppEndpoint || 'Nicht konfiguriert'}
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="text-center py-6 bg-yellow-50 rounded-lg">
                    <Key className="h-12 w-12 text-yellow-600 mx-auto mb-2" />
                    <p className="text-sm text-gray-600">OCPP noch nicht konfiguriert</p>
                    <Button 
                      size="sm" 
                      className="mt-2"
                      onClick={() => setIsEditing(true)}
                    >
                      Jetzt konfigurieren
                    </Button>
                  </div>
                )}
              </>
            )}
          </CardContent>
        </Card>

        {/* Standort */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <MapPin className="h-5 w-5 mr-2 text-red-600" />
              Standort
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <span className="text-sm text-gray-600">Ladepark</span>
              <div className="text-sm font-medium mt-1">
                {station.chargingPark.name}
              </div>
              <div className="text-xs text-gray-500 mt-1">
                {station.chargingPark.address}, {station.chargingPark.city}
              </div>
            </div>
            {isEditing ? (
              <>
                <div className="space-y-2">
                  <Label htmlFor="latitude">Breitengrad</Label>
                  <Input
                    id="latitude"
                    type="number"
                    step="0.000001"
                    value={formData.latitude}
                    onChange={(e) => setFormData({ ...formData, latitude: parseFloat(e.target.value) })}
                    placeholder="z.B. 52.520008"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="longitude">Längengrad</Label>
                  <Input
                    id="longitude"
                    type="number"
                    step="0.000001"
                    value={formData.longitude}
                    onChange={(e) => setFormData({ ...formData, longitude: parseFloat(e.target.value) })}
                    placeholder="z.B. 13.404954"
                  />
                </div>
              </>
            ) : (
              <>
                {station.latitude && station.longitude && (
                  <div>
                    <span className="text-sm text-gray-600">Koordinaten</span>
                    <div className="text-sm font-mono bg-gray-100 px-2 py-1 rounded mt-1">
                      {station.latitude}, {station.longitude}
                    </div>
                  </div>
                )}
              </>
            )}
          </CardContent>
        </Card>

        {/* Ladepunkt-Gruppen */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Layers className="h-5 w-5 mr-2 text-indigo-600" />
              Ladepunkt-Gruppen
            </CardTitle>
            <CardDescription>
              Gruppenzuordnungen für Berechtigungen
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {station.groups && station.groups.length > 0 ? (
              <div className="space-y-2">
                {station.groups.map((group: any) => (
                  <div key={group.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                    <div>
                      <div className="font-medium">{group.name}</div>
                      {group.description && (
                        <div className="text-xs text-gray-500">{group.description}</div>
                      )}
                      <div className="text-xs text-gray-400 mt-1">
                        Zugeordnet: {new Date(group.assignedAt).toLocaleDateString('de-DE')}
                      </div>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleRemoveFromGroup(group.id)}
                      className="text-red-600 hover:text-red-700"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-gray-500">Keiner Gruppe zugeordnet</p>
            )}

            {station.availableGroups && station.availableGroups.length > 0 && (
              <div className="pt-4 border-t">
                <Label>Zu Gruppe hinzufügen</Label>
                <div className="flex gap-2 mt-2">
                  <select
                    id="groupSelect"
                    className="flex-1 rounded-md border border-input bg-background px-3 py-2"
                  >
                    <option value="">Gruppe wählen...</option>
                    {station.availableGroups.map((group: any) => (
                      <option key={group.id} value={group.id}>
                        {group.name}
                      </option>
                    ))}
                  </select>
                  <Button
                    size="sm"
                    onClick={() => {
                      const select = document.getElementById('groupSelect') as HTMLSelectElement;
                      if (select.value) {
                        handleAddToGroup(select.value);
                        select.value = '';
                      }
                    }}
                  >
                    <Plus className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Ladepunkte (EVSE) */}
      {station.chargingPoints && station.chargingPoints.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Ladepunkte (EVSE) ({station.chargingPoints.length})</CardTitle>
                <CardDescription className="mt-1">
                  Jeder Ladepunkt kann mehrere Stecker haben
                </CardDescription>
              </div>
              <Button size="sm" onClick={handleAddChargingPoint}>
                <Plus className="h-4 w-4 mr-2" />
                Ladepunkt hinzufügen
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-6">
              {station.chargingPoints.map((point: any) => (
                <div key={point.id} className="border rounded-lg p-4 bg-gray-50">
                  <div className="flex items-center justify-between mb-4">
                    <div>
                      <div className="font-medium text-lg">{point.name}</div>
                      <div className="flex items-center space-x-4 mt-1">
                        <span className="text-sm text-gray-600">
                          EVSE-ID: <span className="font-mono font-medium">{point.evseId}</span>
                        </span>
                        {point.evseIdExternal && (
                          <span className="text-sm text-gray-600">
                            Externe ID: <span className="font-mono">{point.evseIdExternal}</span>
                          </span>
                        )}
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[point.status]}`}>
                          {point.status}
                        </span>
                      </div>
                    </div>
                    <div className="flex space-x-2">
                      <Button variant="outline" size="sm" onClick={() => handleEditChargingPoint(point)}>
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button 
                        variant="outline" 
                        size="sm" 
                        className="text-red-600 hover:text-red-700"
                        onClick={() => handleDeleteChargingPoint(point.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>

                  <div className="grid grid-cols-4 gap-4 mb-4 text-sm">
                    <div>
                      <span className="text-gray-600">Max. Leistung</span>
                      <div className="font-medium">{point.maxPower} kW</div>
                    </div>
                    <div>
                      <span className="text-gray-600">Smart Charging</span>
                      <div className="font-medium">{point.supportsSmartCharging ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                    <div>
                      <span className="text-gray-600">Remote Start/Stop</span>
                      <div className="font-medium">{point.supportsRemoteStartStop ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                    <div>
                      <span className="text-gray-600">Reservierung</span>
                      <div className="font-medium">{point.supportsReservation ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                  </div>

                  {point.publicKey && (
                    <div className="mb-4 p-3 bg-blue-50 rounded-lg">
                      <div className="flex items-center text-sm text-blue-800">
                        <Key className="h-4 w-4 mr-2" />
                        <span className="font-medium">Plug & Charge aktiviert</span>
                      </div>
                      <div className="text-xs text-blue-600 mt-1">
                        ISO 15118 Zertifikat konfiguriert
                      </div>
                    </div>
                  )}

                  {/* Connectors innerhalb des ChargingPoints */}
                  {point.connectors && point.connectors.length > 0 && (
                    <div>
                      <div className="text-sm font-medium mb-2 flex items-center justify-between">
                        <span>Stecker ({point.connectors.length})</span>
                        <Button variant="outline" size="sm" onClick={() => handleAddConnector(point)}>
                          <Plus className="h-3 w-3 mr-1" />
                          Stecker hinzufügen
                        </Button>
                      </div>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {point.connectors.map((connector: any) => (
                          <div key={connector.id} className="p-3 border rounded-lg bg-white">
                            <div className="flex items-center justify-between">
                              <div className="font-medium">Stecker #{connector.connectorId}</div>
                              <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[connector.status]}`}>
                                {connector.status}
                              </span>
                            </div>
                            <div className="text-sm text-gray-600 mt-2 space-y-1">
                              <div>Typ: <span className="font-medium">{connector.connectorType}</span></div>
                              {connector.powerType && (
                                <div>Strom: <span className="font-medium">{connector.powerType}</span></div>
                              )}
                              <div>Leistung: <span className="font-medium">{connector.maxPower} kW</span></div>
                              <div>
                                <span className="font-medium">{connector.maxCurrent}A @ {connector.maxVoltage}V</span>
                              </div>
                              {connector.physicalReference && (
                                <div className="text-xs text-gray-500 mt-1">
                                  Ref: {connector.physicalReference}
                                </div>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* ChargingPoint Dialog */}
      <Dialog open={showChargingPointDialog} onOpenChange={setShowChargingPointDialog}>
        <DialogContent>
          <ChargingPointForm
            chargingStationId={id!}
            chargingPoint={editingChargingPoint}
            onSubmit={handleSubmitChargingPoint}
            onCancel={() => {
              setShowChargingPointDialog(false);
              setEditingChargingPoint(null);
            }}
          />
        </DialogContent>
      </Dialog>

      {/* Connector Dialog */}
      <Dialog open={showConnectorDialog} onOpenChange={setShowConnectorDialog}>
        <DialogContent>
          {selectedChargingPoint && (
            <ConnectorForm
              chargingPointId={selectedChargingPoint.id}
              connector={editingConnector}
              onSubmit={handleSubmitConnector}
              onCancel={() => {
                setShowConnectorDialog(false);
                setEditingConnector(null);
                setSelectedChargingPoint(null);
              }}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

