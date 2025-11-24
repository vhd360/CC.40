import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Dialog, DialogContent } from '../components/ui/dialog';
import { ChargingPointForm, ChargingPointFormData } from '../components/ChargingPointForm';
import { useToast } from '../components/ui/toast';
import { ConfirmDialog } from '../components/ConfirmDialog';
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
  Trash2,
  Settings,
  FileText,
  History,
  Download
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
  const [editingChargingPoint, setEditingChargingPoint] = useState<any>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ open: boolean; type: 'station' | 'chargingPoint' | 'group'; id: string | null; name?: string }>({
    open: false,
    type: 'station',
    id: null
  });
  const { showToast } = useToast();
  
  // Tab state
  const [activeTab, setActiveTab] = useState<'overview' | 'configuration' | 'firmware' | 'diagnostics'>('overview');
  
  // Configuration state
  const [configuration, setConfiguration] = useState<any[]>([]);
  const [loadingConfiguration, setLoadingConfiguration] = useState(false);
  const [configKey, setConfigKey] = useState('');
  const [configValue, setConfigValue] = useState('');
  
  // Firmware state
  const [firmwareHistory, setFirmwareHistory] = useState<any[]>([]);
  const [loadingFirmware, setLoadingFirmware] = useState(false);
  
  // Diagnostics state
  const [diagnosticsHistory, setDiagnosticsHistory] = useState<any[]>([]);
  const [loadingDiagnostics, setLoadingDiagnostics] = useState(false);
  const [diagnosticsLocation, setDiagnosticsLocation] = useState('https://example.com/upload');
  const [diagnosticsStartTime, setDiagnosticsStartTime] = useState('');
  const [diagnosticsStopTime, setDiagnosticsStopTime] = useState('');

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
        status: ['Available', 'Occupied', 'OutOfOrder', 'Reserved', 'Unavailable', 'Offline'].indexOf(data.status),
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

  useEffect(() => {
    if (activeTab === 'configuration' && id) {
      loadConfiguration();
    } else if (activeTab === 'firmware' && id) {
      loadFirmwareHistory();
    } else if (activeTab === 'diagnostics' && id) {
      loadDiagnosticsHistory();
    }
  }, [activeTab, id]);

  const loadConfiguration = async () => {
    if (!id) return;
    try {
      setLoadingConfiguration(true);
      const config = await api.getStationConfiguration(id);
      setConfiguration(config.configurationKey || []);
      
      // Only show warning if configuration is empty AND station appears to be offline
      if (config.configurationKey && config.configurationKey.length === 0) {
        const isStationOnline = station?.lastHeartbeat && 
          (new Date().getTime() - new Date(station.lastHeartbeat).getTime()) < 10 * 60 * 1000; // 10 minutes
        
        if (!isStationOnline) {
          showToast('Keine Konfiguration verfügbar. Die Station ist möglicherweise nicht verbunden.', 'warning');
        } else {
          // Station is online but no configuration - might be normal for some stations
          showToast('Keine Konfiguration verfügbar. Die Station hat möglicherweise keine Konfigurationsparameter.', 'info');
        }
      } else if (config.configurationKey && config.configurationKey.length > 0) {
        showToast(`Konfiguration erfolgreich geladen: ${config.configurationKey.length} Parameter`, 'success');
      }
    } catch (error: any) {
      console.error('Failed to load configuration:', error);
      const errorMessage = error.message || 'Fehler beim Laden der Konfiguration';
      showToast(errorMessage, 'error');
      // Set empty configuration on error to prevent UI issues
      setConfiguration([]);
    } finally {
      setLoadingConfiguration(false);
    }
  };

  const handleChangeConfiguration = async () => {
    if (!id || !configKey || !configValue) {
      showToast('Bitte füllen Sie alle Felder aus', 'error');
      return;
    }
    try {
      await api.changeStationConfiguration(id, configKey, configValue);
      showToast('Konfiguration erfolgreich geändert', 'success');
      setConfigKey('');
      setConfigValue('');
      await loadConfiguration();
    } catch (error: any) {
      console.error('Failed to change configuration:', error);
      showToast(error.message || 'Fehler beim Ändern der Konfiguration', 'error');
    }
  };

  const loadFirmwareHistory = async () => {
    if (!id) return;
    try {
      setLoadingFirmware(true);
      const history = await api.getStationFirmwareHistory(id);
      setFirmwareHistory(history);
    } catch (error: any) {
      console.error('Failed to load firmware history:', error);
      showToast(error.message || 'Fehler beim Laden der Firmware-Historie', 'error');
    } finally {
      setLoadingFirmware(false);
    }
  };

  const loadDiagnosticsHistory = async () => {
    if (!id) return;
    try {
      setLoadingDiagnostics(true);
      const history = await api.getStationDiagnosticsHistory(id);
      setDiagnosticsHistory(history);
    } catch (error: any) {
      console.error('Failed to load diagnostics history:', error);
      showToast(error.message || 'Fehler beim Laden der Diagnose-Historie', 'error');
    } finally {
      setLoadingDiagnostics(false);
    }
  };

  const handleRequestDiagnostics = async () => {
    if (!id || !diagnosticsLocation) {
      showToast('Bitte geben Sie eine Upload-URL an', 'error');
      return;
    }
    try {
      await api.requestStationDiagnostics(
        id,
        diagnosticsLocation,
        diagnosticsStartTime || undefined,
        diagnosticsStopTime || undefined
      );
      showToast('Diagnose-Anfrage erfolgreich gesendet', 'success');
      setDiagnosticsLocation('https://example.com/upload');
      setDiagnosticsStartTime('');
      setDiagnosticsStopTime('');
      await loadDiagnosticsHistory();
    } catch (error: any) {
      console.error('Failed to request diagnostics:', error);
      showToast(error.message || 'Fehler beim Anfordern der Diagnose', 'error');
    }
  };

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
      showToast('Ladestation erfolgreich gespeichert', 'success');
    } catch (error) {
      console.error('Failed to save station:', error);
      showToast('Fehler beim Speichern der Ladestation', 'error');
    }
  };

  const handleAddToGroup = async (groupId: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(
        `http://localhost:5126/api/charging-station-groups/${groupId}/stations/${id}`,
        { 
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error(`Failed to add station to group: ${errorText}`);
      }
      loadStation();
      showToast('Ladestation erfolgreich zur Gruppe hinzugefügt', 'success');
    } catch (error) {
      console.error('Failed to add to group:', error);
      showToast('Fehler beim Hinzufügen zur Gruppe', 'error');
    }
  };

  const handleRemoveFromGroup = (groupId: string) => {
    const group = station.groups?.find((g: any) => g.id === groupId);
    setDeleteConfirm({
      open: true,
      type: 'group',
      id: groupId,
      name: group?.name
    });
  };

  const handleRemoveFromGroupConfirm = async () => {
    if (!deleteConfirm.id || deleteConfirm.type !== 'group') return;
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(
        `http://localhost:5126/api/charging-station-groups/${deleteConfirm.id}/stations/${id}`,
        { 
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error(`Failed to remove station from group: ${errorText}`);
      }
      loadStation();
      showToast('Ladestation erfolgreich aus der Gruppe entfernt', 'success');
    } catch (error) {
      console.error('Failed to remove from group:', error);
      showToast('Fehler beim Entfernen aus der Gruppe', 'error');
    } finally {
      setDeleteConfirm({ open: false, type: 'station', id: null });
    }
  };

  // ChargingPoint handlers
  const handleAddChargingPoint = () => {
    setEditingChargingPoint(null);
    setShowChargingPointDialog(true);
  };

  const handleEditChargingPoint = (point: any) => {
    // Convert status string to number for the form
    const statusMap: Record<string, number> = {
      'Available': 0,
      'Occupied': 1,
      'Charging': 2,
      'Reserved': 3,
      'Faulted': 4,
      'Unavailable': 5,
      'Preparing': 6,
      'Finishing': 7,
      'Offline': 8
    };
    
    const editData = {
      ...point,
      status: typeof point.status === 'string' ? statusMap[point.status] || 0 : point.status
    };
    
    setEditingChargingPoint(editData);
    setShowChargingPointDialog(true);
  };

  const handleSubmitChargingPoint = async (data: ChargingPointFormData) => {
    try {
      const token = localStorage.getItem('token');
      const url = editingChargingPoint
        ? `http://localhost:5126/api/charging-points/${editingChargingPoint.id}`
        : `http://localhost:5126/api/charging-points`;
      
      // Convert camelCase to PascalCase for backend
      const dto = {
        ChargingStationId: data.chargingStationId,
        EvseId: data.evseId,
        EvseIdExternal: data.evseIdExternal || null,
        Name: data.name,
        Description: data.description || null,
        MaxPower: data.maxPower,
        Status: data.status,
        // Connector-Eigenschaften
        ConnectorType: data.connectorType || 'Type2',
        ConnectorFormat: data.connectorFormat || null,
        PowerType: data.powerType || null,
        MaxCurrent: data.maxCurrent || 32,
        MaxVoltage: data.maxVoltage || 230,
        PhysicalReference: data.physicalReference || null,
        // Funktionen
        PublicKey: data.publicKey || null,
        CertificateChain: data.certificateChain || null,
        SupportsSmartCharging: data.supportsSmartCharging,
        SupportsRemoteStartStop: data.supportsRemoteStartStop,
        SupportsReservation: data.supportsReservation,
        TariffInfo: data.tariffInfo || null,
        Notes: data.notes || null
      };
      
      const response = await fetch(url, {
        method: editingChargingPoint ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(dto)
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error('Failed to save charging point');
      }
      
      setShowChargingPointDialog(false);
      setEditingChargingPoint(null);
      loadStation();
      showToast(editingChargingPoint ? 'Ladepunkt erfolgreich aktualisiert' : 'Ladepunkt erfolgreich erstellt', 'success');
    } catch (error) {
      console.error('Failed to save charging point:', error);
      showToast('Fehler beim Speichern des Ladepunkts', 'error');
    }
  };

  const handleDeleteChargingPoint = (pointId: string) => {
    const point = station.chargingPoints?.find((p: any) => p.id === pointId);
    setDeleteConfirm({
      open: true,
      type: 'chargingPoint',
      id: pointId,
      name: point?.name
    });
  };

  const handleDeleteChargingPointConfirm = async () => {
    if (!deleteConfirm.id || deleteConfirm.type !== 'chargingPoint') return;
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`http://localhost:5126/api/charging-points/${deleteConfirm.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error('Failed to delete charging point');
      }
      loadStation();
      showToast('Ladepunkt erfolgreich gelöscht', 'success');
    } catch (error) {
      console.error('Failed to delete charging point:', error);
      showToast('Fehler beim Löschen des Ladepunkts', 'error');
    } finally {
      setDeleteConfirm({ open: false, type: 'station', id: null });
    }
  };


  const handleDeleteStation = () => {
    setDeleteConfirm({
      open: true,
      type: 'station',
      id: id || null,
      name: station?.name
    });
  };

  const handleDeleteStationConfirm = async () => {
    if (!deleteConfirm.id || deleteConfirm.type !== 'station') return;
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`http://localhost:5126/api/charging-stations/${deleteConfirm.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Server error:', errorText);
        throw new Error('Failed to delete charging station');
      }
      showToast('Ladestation erfolgreich gelöscht', 'success');
      navigate('/charging-stations');
    } catch (error) {
      console.error('Failed to delete charging station:', error);
      showToast('Fehler beim Löschen der Ladestation', 'error');
    } finally {
      setDeleteConfirm({ open: false, type: 'station', id: null });
    }
  };


  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Ladestation...</span>
      </div>
    );
  }

  if (!station) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Ladestation nicht gefunden</h2>
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
    'Reserved': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'Unavailable': 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200',
    'Offline': 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400'
  };

  // Konvertiere Status-Zahl zurück zu String für Anzeige (ChargingStation)
  const getStatusDisplay = (status: number | string | undefined | null): string => {
    if (status === null || status === undefined) return 'Unknown';
    if (typeof status === 'string') return status;
    if (typeof status === 'number') {
      const statusNames = ['Available', 'Occupied', 'OutOfOrder', 'Reserved', 'Unavailable', 'Offline'];
      return statusNames[status] || 'Unknown';
    }
    return 'Unknown';
  };

  // Konvertiere Status-Zahl zurück zu String für Anzeige (ChargingPoint)
  const getPointStatusDisplay = (status: number | string | undefined | null): string => {
    if (status === null || status === undefined) return 'Unknown';
    if (typeof status === 'string') return status;
    if (typeof status === 'number') {
      const statusNames = ['Available', 'Occupied', 'Charging', 'Reserved', 'Faulted', 'Unavailable', 'Preparing', 'Finishing', 'Offline'];
      return statusNames[status] || 'Unknown';
    }
    return 'Unknown';
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
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{station.name}</h1>
            <p className="text-gray-600 dark:text-gray-400 mt-1">ID: {station.stationId}</p>
          </div>
        </div>
        {!isEditing ? (
          <div className="flex space-x-2">
            <Button onClick={() => setIsEditing(true)}>
              <Edit className="h-4 w-4 mr-2" />
              Bearbeiten
            </Button>
            <Button 
              variant="outline" 
              className="text-red-600 hover:text-red-700 hover:bg-red-50"
              onClick={handleDeleteStation}
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Löschen
            </Button>
          </div>
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

      {/* Tabs */}
      <div className="border-b border-gray-200 dark:border-gray-800">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('overview')}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'overview'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            Übersicht
          </button>
          <button
            onClick={() => setActiveTab('configuration')}
            className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
              activeTab === 'configuration'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <Settings className="h-4 w-4 mr-2" />
            Konfiguration
          </button>
          <button
            onClick={() => setActiveTab('firmware')}
            className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
              activeTab === 'firmware'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <History className="h-4 w-4 mr-2" />
            Firmware
          </button>
          <button
            onClick={() => setActiveTab('diagnostics')}
            className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
              activeTab === 'diagnostics'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            }`}
          >
            <FileText className="h-4 w-4 mr-2" />
            Diagnosen
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <div className="space-y-6">
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
                    <option value="5">Offline</option>
                  </select>
                </div>
              </>
            ) : (
              <>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Status</span>
                  <div className="mt-1">
                    <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${statusColors[getStatusDisplay(station.status)]}`}>
                      {getStatusDisplay(station.status)}
                    </span>
                  </div>
                </div>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Hersteller</span>
                  <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.vendor}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Modell</span>
                  <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.model}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Typ</span>
                  <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.type}</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Maximale Leistung</span>
                  <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.maxPower} kW</div>
                </div>
                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Anzahl Anschlüsse</span>
                  <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.numberOfConnectors}</div>
                </div>
                {station.lastHeartbeat && (
                  <div>
                    <span className="text-sm text-gray-600 dark:text-gray-400">Letzter Heartbeat</span>
                    <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">
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
                  <p className="text-xs text-gray-500 dark:text-gray-400">
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
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    WebSocket-URL des OCPP-Servers
                  </p>
                </div>
              </>
            ) : (
              <>
                {station.chargeBoxId ? (
                  <>
                    <div>
                      <span className="text-sm text-gray-600 dark:text-gray-400">ChargeBox-ID</span>
                      <div className="text-sm font-mono bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-gray-100 px-2 py-1 rounded mt-1">
                        {station.chargeBoxId}
                      </div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600 dark:text-gray-400">Passwort</span>
                      <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">
                        {station.ocppPassword ? '••••••••' : 'Nicht gesetzt'}
                      </div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600 dark:text-gray-400">Protokoll</span>
                      <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">{station.ocppProtocol || 'Nicht konfiguriert'}</div>
                    </div>
                    <div>
                      <span className="text-sm text-gray-600 dark:text-gray-400">Server-Endpoint</span>
                      <div className="text-sm font-mono bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-gray-100 px-2 py-1 rounded mt-1 break-all">
                        {station.ocppEndpoint || 'Nicht konfiguriert'}
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="text-center py-6 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg border border-yellow-200 dark:border-yellow-800">
                    <Key className="h-12 w-12 text-yellow-600 dark:text-yellow-400 mx-auto mb-2" />
                    <p className="text-sm text-gray-600 dark:text-gray-400">OCPP noch nicht konfiguriert</p>
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
              <span className="text-sm text-gray-600 dark:text-gray-400">Ladepark</span>
              <div className="text-sm font-medium mt-1 text-gray-900 dark:text-gray-100">
                {station.chargingPark.name}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
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
                    <span className="text-sm text-gray-600 dark:text-gray-400">Koordinaten</span>
                    <div className="text-sm font-mono bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-gray-100 px-2 py-1 rounded mt-1">
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
                  <div key={group.id} className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
                    <div>
                      <div className="font-medium text-gray-900 dark:text-gray-100">{group.name}</div>
                      {group.description && (
                        <div className="text-xs text-gray-500 dark:text-gray-400">{group.description}</div>
                      )}
                      <div className="text-xs text-gray-400 dark:text-gray-500 mt-1">
                        Zugeordnet: {new Date(group.assignedAt).toLocaleDateString('de-DE')}
                      </div>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleRemoveFromGroup(group.id)}
                      className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-gray-500 dark:text-gray-400">Keiner Gruppe zugeordnet</p>
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
          <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Ladepunkte (EVSE) ({station.chargingPoints?.length || 0})</CardTitle>
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
          {station.chargingPoints && station.chargingPoints.length > 0 ? (
            <div className="space-y-6">
              {station.chargingPoints.map((point: any) => (
                <div key={point.id} className="border rounded-lg p-4 bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700">
                  <div className="flex items-center justify-between mb-4">
                    <div>
                      <div className="font-medium text-lg text-gray-900 dark:text-gray-100">{point.name}</div>
                      <div className="flex items-center space-x-4 mt-1">
                        <span className="text-sm text-gray-600 dark:text-gray-400">
                          EVSE-ID: <span className="font-mono font-medium text-gray-900 dark:text-gray-100">{point.evseId}</span>
                        </span>
                        {point.evseIdExternal && (
                          <span className="text-sm text-gray-600 dark:text-gray-400">
                            Externe ID: <span className="font-mono text-gray-900 dark:text-gray-100">{point.evseIdExternal}</span>
                          </span>
                        )}
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[getPointStatusDisplay(point.status)]}`}>
                          {getPointStatusDisplay(point.status)}
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
                      <span className="text-gray-600 dark:text-gray-400">Max. Leistung</span>
                      <div className="font-medium text-gray-900 dark:text-gray-100">{point.maxPower} kW</div>
                    </div>
                    <div>
                      <span className="text-gray-600 dark:text-gray-400">Smart Charging</span>
                      <div className="font-medium text-gray-900 dark:text-gray-100">{point.supportsSmartCharging ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                    <div>
                      <span className="text-gray-600 dark:text-gray-400">Remote Start/Stop</span>
                      <div className="font-medium text-gray-900 dark:text-gray-100">{point.supportsRemoteStartStop ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                    <div>
                      <span className="text-gray-600 dark:text-gray-400">Reservierung</span>
                      <div className="font-medium text-gray-900 dark:text-gray-100">{point.supportsReservation ? '✓ Ja' : '✗ Nein'}</div>
                    </div>
                  </div>

                  {point.publicKey && (
                    <div className="mb-4 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
                      <div className="flex items-center text-sm text-blue-800 dark:text-blue-200">
                        <Key className="h-4 w-4 mr-2" />
                        <span className="font-medium">Plug & Charge aktiviert</span>
                      </div>
                      <div className="text-xs text-blue-600 dark:text-blue-300 mt-1">
                        ISO 15118 Zertifikat konfiguriert
                      </div>
                    </div>
                  )}

                  {/* Connector-Eigenschaften (jetzt Teil des ChargingPoints) */}
                  {point.connectorType && (
                    <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                      <div className="text-sm font-medium mb-3 text-gray-900 dark:text-gray-100">Stecker-Eigenschaften</div>
                      <div className="grid grid-cols-2 gap-3 text-sm">
                        <div>
                          <span className="text-gray-600 dark:text-gray-400">Typ:</span>{' '}
                          <span className="font-medium text-gray-900 dark:text-gray-100">{point.connectorType}</span>
                        </div>
                        {point.connectorFormat && (
                          <div>
                            <span className="text-gray-600 dark:text-gray-400">Format:</span>{' '}
                            <span className="font-medium text-gray-900 dark:text-gray-100">{point.connectorFormat}</span>
                          </div>
                        )}
                        {point.powerType && (
                          <div>
                            <span className="text-gray-600 dark:text-gray-400">Stromart:</span>{' '}
                            <span className="font-medium text-gray-900 dark:text-gray-100">{point.powerType}</span>
                          </div>
                        )}
                        {point.maxCurrent && point.maxVoltage && (
                          <div>
                            <span className="text-gray-600 dark:text-gray-400">Strom/Spannung:</span>{' '}
                            <span className="font-medium text-gray-900 dark:text-gray-100">{point.maxCurrent}A @ {point.maxVoltage}V</span>
                          </div>
                        )}
                        {point.physicalReference && (
                          <div className="col-span-2">
                            <span className="text-gray-600 dark:text-gray-400">Physische Referenz:</span>{' '}
                            <span className="font-medium text-gray-900 dark:text-gray-100">{point.physicalReference}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12 bg-gray-50 rounded-lg">
              <Zap className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Noch keine Ladepunkte vorhanden
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                Legen Sie einen ersten Ladepunkt (EVSE) für diese Station an.<br />
                Ein Ladepunkt entspricht einem physischen Stecker.
              </p>
              <Button onClick={handleAddChargingPoint}>
                <Plus className="h-4 w-4 mr-2" />
                Ersten Ladepunkt anlegen
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

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
        </div>
      )}

      {/* Configuration Tab */}

      {/* Configuration Tab */}
      {activeTab === 'configuration' && (
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Settings className="h-5 w-5 mr-2 text-blue-600" />
                Konfiguration
              </CardTitle>
              <CardDescription>
                Konfigurationsparameter der Ladestation abrufen und ändern
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between items-center">
                <Button onClick={loadConfiguration} disabled={loadingConfiguration}>
                  {loadingConfiguration ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Lade...
                    </>
                  ) : (
                    <>
                      <Download className="h-4 w-4 mr-2" />
                      Konfiguration abrufen
                    </>
                  )}
                </Button>
              </div>

              {configuration.length > 0 && (
                <div className="space-y-2">
                  <h3 className="font-medium text-gray-900 dark:text-gray-100">Aktuelle Konfiguration</h3>
                  <div className="border rounded-lg overflow-hidden">
                    <table className="w-full">
                      <thead className="bg-gray-50 dark:bg-gray-800">
                        <tr>
                          <th className="px-4 py-2 text-left text-sm font-medium text-gray-900 dark:text-gray-100">Key</th>
                          <th className="px-4 py-2 text-left text-sm font-medium text-gray-900 dark:text-gray-100">Value</th>
                          <th className="px-4 py-2 text-left text-sm font-medium text-gray-900 dark:text-gray-100">Readonly</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                        {configuration.map((config: any, index: number) => (
                          <tr key={index} className="hover:bg-gray-50 dark:hover:bg-gray-800">
                            <td className="px-4 py-2 text-sm font-mono text-gray-900 dark:text-gray-100">{config.key}</td>
                            <td className="px-4 py-2 text-sm text-gray-700 dark:text-gray-300">{config.value || '-'}</td>
                            <td className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400">
                              {config.readonly ? '✓' : '-'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}

              <div className="border-t pt-4">
                <h3 className="font-medium text-gray-900 dark:text-gray-100 mb-4">Konfiguration ändern</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <Label htmlFor="configKey">Key</Label>
                    <Input
                      id="configKey"
                      value={configKey}
                      onChange={(e) => setConfigKey(e.target.value)}
                      placeholder="z.B. HeartbeatInterval"
                    />
                  </div>
                  <div>
                    <Label htmlFor="configValue">Value</Label>
                    <Input
                      id="configValue"
                      value={configValue}
                      onChange={(e) => setConfigValue(e.target.value)}
                      placeholder="z.B. 300"
                    />
                  </div>
                  <div className="flex items-end">
                    <Button onClick={handleChangeConfiguration} className="w-full">
                      Ändern
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Firmware Tab */}
      {activeTab === 'firmware' && (
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <History className="h-5 w-5 mr-2 text-blue-600" />
                Firmware-Historie
              </CardTitle>
              <CardDescription>
                Verlauf aller Firmware-Updates und Statusänderungen
              </CardDescription>
            </CardHeader>
            <CardContent>
              {loadingFirmware ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
                  <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Firmware-Historie...</span>
                </div>
              ) : firmwareHistory.length === 0 ? (
                <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                  Keine Firmware-Historie verfügbar
                </div>
              ) : (
                <div className="space-y-4">
                  {firmwareHistory.map((entry: any) => (
                    <div key={entry.id} className="border rounded-lg p-4">
                      <div className="flex justify-between items-start">
                        <div>
                          <div className="font-medium text-gray-900 dark:text-gray-100">
                            Version: {entry.firmwareVersion}
                          </div>
                          <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                            Status: <span className="font-medium">{entry.status}</span>
                          </div>
                          {entry.info && (
                            <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                              {entry.info}
                            </div>
                          )}
                        </div>
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          {new Date(entry.timestamp).toLocaleString('de-DE')}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {/* Diagnostics Tab */}
      {activeTab === 'diagnostics' && (
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <FileText className="h-5 w-5 mr-2 text-blue-600" />
                Diagnoseinformationen
              </CardTitle>
              <CardDescription>
                Diagnoseinformationen anfordern und Historie einsehen
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="border rounded-lg p-4">
                <h3 className="font-medium text-gray-900 dark:text-gray-100 mb-4">Neue Diagnose anfordern</h3>
                <div className="space-y-4">
                  <div>
                    <Label htmlFor="diagnosticsLocation">Upload-URL *</Label>
                    <Input
                      id="diagnosticsLocation"
                      value={diagnosticsLocation}
                      onChange={(e) => setDiagnosticsLocation(e.target.value)}
                      placeholder="https://example.com/upload"
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label htmlFor="diagnosticsStartTime">Startzeit (optional)</Label>
                      <Input
                        id="diagnosticsStartTime"
                        type="datetime-local"
                        value={diagnosticsStartTime}
                        onChange={(e) => setDiagnosticsStartTime(e.target.value)}
                      />
                    </div>
                    <div>
                      <Label htmlFor="diagnosticsStopTime">Endzeit (optional)</Label>
                      <Input
                        id="diagnosticsStopTime"
                        type="datetime-local"
                        value={diagnosticsStopTime}
                        onChange={(e) => setDiagnosticsStopTime(e.target.value)}
                      />
                    </div>
                  </div>
                  <Button onClick={handleRequestDiagnostics}>
                    <Download className="h-4 w-4 mr-2" />
                    Diagnose anfordern
                  </Button>
                </div>
              </div>

              <div>
                <h3 className="font-medium text-gray-900 dark:text-gray-100 mb-4">Diagnose-Historie</h3>
                {loadingDiagnostics ? (
                  <div className="flex items-center justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
                    <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Diagnose-Historie...</span>
                  </div>
                ) : diagnosticsHistory.length === 0 ? (
                  <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                    Keine Diagnose-Anfragen vorhanden
                  </div>
                ) : (
                  <div className="space-y-4">
                    {diagnosticsHistory.map((entry: any) => (
                      <div key={entry.id} className="border rounded-lg p-4">
                        <div className="flex justify-between items-start">
                          <div>
                            <div className="font-medium text-gray-900 dark:text-gray-100">
                              Status: <span className={entry.status === 'Completed' ? 'text-green-600' : entry.status === 'Failed' ? 'text-red-600' : 'text-yellow-600'}>{entry.status}</span>
                            </div>
                            {entry.fileName && (
                              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                                Datei: {entry.fileName}
                              </div>
                            )}
                            {entry.diagnosticsUrl && (
                              <div className="text-sm text-blue-600 dark:text-blue-400 mt-1">
                                <a href={entry.diagnosticsUrl} target="_blank" rel="noopener noreferrer">
                                  {entry.diagnosticsUrl}
                                </a>
                              </div>
                            )}
                            {entry.errorMessage && (
                              <div className="text-sm text-red-600 dark:text-red-400 mt-1">
                                Fehler: {entry.errorMessage}
                              </div>
                            )}
                          </div>
                          <div className="text-sm text-gray-500 dark:text-gray-400">
                            <div>Angefordert: {new Date(entry.requestedAt).toLocaleString('de-DE')}</div>
                            {entry.completedAt && (
                              <div>Abgeschlossen: {new Date(entry.completedAt).toLocaleString('de-DE')}</div>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Delete Confirmation Dialogs */}
      <ConfirmDialog
        open={deleteConfirm.open && deleteConfirm.type === 'station'}
        onOpenChange={(open) => setDeleteConfirm({ ...deleteConfirm, open })}
        title="Ladestation löschen"
        message={`Möchten Sie die Ladestation "${deleteConfirm.name}" wirklich löschen? Alle Ladepunkte werden ebenfalls deaktiviert. Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteStationConfirm}
      />

      <ConfirmDialog
        open={deleteConfirm.open && deleteConfirm.type === 'chargingPoint'}
        onOpenChange={(open) => setDeleteConfirm({ ...deleteConfirm, open })}
        title="Ladepunkt löschen"
        message={`Möchten Sie den Ladepunkt "${deleteConfirm.name}" wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteChargingPointConfirm}
      />

      <ConfirmDialog
        open={deleteConfirm.open && deleteConfirm.type === 'group'}
        onOpenChange={(open) => setDeleteConfirm({ ...deleteConfirm, open })}
        title="Aus Gruppe entfernen"
        message={`Möchten Sie diese Ladestation wirklich aus der Gruppe "${deleteConfirm.name}" entfernen?`}
        confirmText="Entfernen"
        cancelText="Abbrechen"
        variant="default"
        onConfirm={handleRemoveFromGroupConfirm}
      />
    </div>
  );
};

