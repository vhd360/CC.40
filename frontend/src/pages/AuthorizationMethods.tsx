import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { CreditCard, Plus, Loader2, Edit, Trash2, Smartphone, QrCode as QrCodeIcon, Key, Car } from 'lucide-react';
import { api } from '../services/api';

interface AuthorizationMethod {
  id: string;
  userId: string;
  userName: string;
  email: string;
  type: string;
  identifier: string;
  friendlyName?: string;
  isActive: boolean;
  validFrom?: string;
  validUntil?: string;
  createdAt: string;
  lastUsedAt?: string;
}

const methodTypeMap: Record<string, { label: string; icon: any; color: string }> = {
  'RFID': { label: 'RFID-Karte', icon: CreditCard, color: 'text-blue-600' },
  'Autocharge': { label: 'Autocharge (Plug & Charge)', icon: Car, color: 'text-green-600' },
  'App': { label: 'Mobile App', icon: Smartphone, color: 'text-purple-600' },
  'QRCode': { label: 'QR-Code', icon: QrCodeIcon, color: 'text-orange-600' },
  'CreditCard': { label: 'Kreditkarte', icon: CreditCard, color: 'text-pink-600' },
  'PlugAndCharge': { label: 'Plug & Charge (ISO 15118)', icon: Key, color: 'text-indigo-600' }
};

export const AuthorizationMethods: React.FC = () => {
  const [methods, setMethods] = useState<AuthorizationMethod[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingMethod, setEditingMethod] = useState<AuthorizationMethod | null>(null);
  const [users, setUsers] = useState<any[]>([]);
  const [vehicles, setVehicles] = useState<any[]>([]);
  const [formData, setFormData] = useState({
    userId: '',
    type: 0,
    identifier: '',
    friendlyName: '',
    validFrom: '',
    validUntil: '',
    metadata: ''
  });

  const loadMethods = async () => {
    try {
      setLoading(true);
      const data = await api.getAuthorizationMethods();
      setMethods(data);
    } catch (error) {
      console.error('Failed to load authorization methods:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadUsers = async () => {
    try {
      const data = await api.getUsers();
      setUsers(data);
    } catch (error) {
      console.error('Failed to load users:', error);
    }
  };

  const loadVehicles = async () => {
    try {
      const data = await api.getVehicles();
      setVehicles(data);
    } catch (error) {
      console.error('Failed to load vehicles:', error);
    }
  };

  useEffect(() => {
    loadMethods();
    loadUsers();
    loadVehicles();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingMethod) {
        await api.updateAuthorizationMethod(editingMethod.id, {
          friendlyName: formData.friendlyName || null,
          isActive: true,
          validFrom: formData.validFrom || null,
          validUntil: formData.validUntil || null,
          metadata: formData.metadata || null
        });
      } else {
        await api.createAuthorizationMethod({
          userId: formData.userId,
          type: parseInt(formData.type.toString()),
          identifier: formData.identifier,
          friendlyName: formData.friendlyName || null,
          validFrom: formData.validFrom || null,
          validUntil: formData.validUntil || null,
          metadata: formData.metadata || null
        });
      }
      setShowForm(false);
      setEditingMethod(null);
      setFormData({ userId: '', type: 0, identifier: '', friendlyName: '', validFrom: '', validUntil: '', metadata: '' });
      loadMethods();
    } catch (error) {
      console.error('Failed to save authorization method:', error);
      alert('Fehler beim Speichern der Identifikationsmethode');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Möchten Sie diese Identifikationsmethode wirklich löschen?')) return;
    try {
      await api.deleteAuthorizationMethod(id);
      loadMethods();
    } catch (error) {
      console.error('Failed to delete authorization method:', error);
      alert('Fehler beim Löschen der Identifikationsmethode');
    }
  };

  const handleEdit = (method: AuthorizationMethod) => {
    setEditingMethod(method);
    setFormData({
      userId: method.userId,
      type: ['RFID', 'Autocharge', 'App', 'QRCode', 'CreditCard', 'PlugAndCharge'].indexOf(method.type),
      identifier: method.identifier,
      friendlyName: method.friendlyName || '',
      validFrom: method.validFrom ? method.validFrom.split('T')[0] : '',
      validUntil: method.validUntil ? method.validUntil.split('T')[0] : '',
      metadata: ''
    });
    setShowForm(true);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingMethod(null);
    setFormData({ userId: '', type: 0, identifier: '', friendlyName: '', validFrom: '', validUntil: '', metadata: '' });
  };

  const handleVehicleSelect = (vehicleId: string) => {
    const vehicle = vehicles.find(v => v.id === vehicleId);
    if (vehicle && vehicle.vin) {
      setFormData({ ...formData, identifier: vehicle.vin });
    }
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={handleCancel}>
          ← Zurück
        </Button>
        <div className="flex justify-center">
          <Card className="w-full max-w-2xl">
            <CardHeader>
              <CardTitle>
                {editingMethod ? 'Identifikationsmethode bearbeiten' : 'Neue Identifikationsmethode'}
              </CardTitle>
              <CardDescription>
                {editingMethod 
                  ? 'Aktualisieren Sie die Methoden-Daten' 
                  : 'Fügen Sie eine neue Autorisierungsmethode hinzu (RFID, Autocharge, etc.)'}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-4">
                {!editingMethod && (
                  <>
                    <div className="space-y-2">
                      <Label htmlFor="userId">Benutzer *</Label>
                      <select
                        id="userId"
                        value={formData.userId}
                        onChange={(e) => setFormData({ ...formData, userId: e.target.value })}
                        className="w-full rounded-md border border-input bg-background px-3 py-2"
                        required
                      >
                        <option value="">Bitte wählen...</option>
                        {users.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.firstName} {user.lastName} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="type">Typ *</Label>
                      <select
                        id="type"
                        value={formData.type}
                        onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) })}
                        className="w-full rounded-md border border-input bg-background px-3 py-2"
                        required
                      >
                        <option value="0">RFID-Karte</option>
                        <option value="1">Autocharge (Plug & Charge)</option>
                        <option value="2">Mobile App</option>
                        <option value="3">QR-Code</option>
                        <option value="4">Kreditkarte</option>
                        <option value="5">Plug & Charge (ISO 15118)</option>
                      </select>
                    </div>

                    {formData.type === 1 && (
                      <div className="space-y-2">
                        <Label htmlFor="vehicle">Fahrzeug (optional)</Label>
                        <select
                          id="vehicle"
                          onChange={(e) => handleVehicleSelect(e.target.value)}
                          className="w-full rounded-md border border-input bg-background px-3 py-2"
                        >
                          <option value="">Fahrzeug auswählen (VIN wird übernommen)...</option>
                          {vehicles.map((vehicle) => (
                            <option key={vehicle.id} value={vehicle.id}>
                              {vehicle.make} {vehicle.model} - {vehicle.licensePlate}
                              {vehicle.vin && ` (VIN: ${vehicle.vin})`}
                            </option>
                          ))}
                        </select>
                      </div>
                    )}

                    <div className="space-y-2">
                      <Label htmlFor="identifier">
                        {formData.type === 0 ? 'RFID-Tag-Nummer' : 
                         formData.type === 1 ? 'Fahrzeug-Identifikationsnummer (VIN)' : 
                         formData.type === 4 ? 'Kreditkarten-Token' : 
                         'Identifikator'} *
                      </Label>
                      <Input
                        id="identifier"
                        value={formData.identifier}
                        onChange={(e) => setFormData({ ...formData, identifier: e.target.value })}
                        required
                        placeholder={
                          formData.type === 0 ? 'z.B. 0123456789ABCDEF' : 
                          formData.type === 1 ? 'z.B. WVW1234567890123' : 
                          'Eindeutiger Identifikator'
                        }
                      />
                    </div>
                  </>
                )}

                <div className="space-y-2">
                  <Label htmlFor="friendlyName">Freundlicher Name</Label>
                  <Input
                    id="friendlyName"
                    value={formData.friendlyName}
                    onChange={(e) => setFormData({ ...formData, friendlyName: e.target.value })}
                    placeholder="z.B. Meine RFID-Karte, Tesla Model 3"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="validFrom">Gültig ab</Label>
                    <Input
                      id="validFrom"
                      type="date"
                      value={formData.validFrom}
                      onChange={(e) => setFormData({ ...formData, validFrom: e.target.value })}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="validUntil">Gültig bis</Label>
                    <Input
                      id="validUntil"
                      type="date"
                      value={formData.validUntil}
                      onChange={(e) => setFormData({ ...formData, validUntil: e.target.value })}
                    />
                  </div>
                </div>

                {!editingMethod && formData.type === 1 && (
                  <div className="space-y-2">
                    <Label htmlFor="metadata">Zusätzliche Informationen (JSON)</Label>
                    <textarea
                      id="metadata"
                      value={formData.metadata}
                      onChange={(e) => setFormData({ ...formData, metadata: e.target.value })}
                      className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2 font-mono text-sm"
                      placeholder='{"manufacturer": "Tesla", "model": "Model 3"}'
                    />
                  </div>
                )}

                <div className="flex justify-end space-x-2 pt-4">
                  <Button type="button" variant="outline" onClick={handleCancel}>
                    Abbrechen
                  </Button>
                  <Button type="submit">
                    {editingMethod ? 'Speichern' : 'Hinzufügen'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Identifikationsmethoden</h1>
          <p className="text-gray-600 mt-1">
            Verwalten Sie RFID-Karten, Autocharge, Mobile Apps und weitere Autorisierungsmethoden
          </p>
        </div>
        <Button onClick={() => setShowForm(true)} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neue Methode</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Lade Identifikationsmethoden...</span>
        </div>
      ) : methods.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {methods.map((method) => {
            const typeInfo = methodTypeMap[method.type] || { 
              label: method.type, 
              icon: Key, 
              color: 'text-gray-600' 
            };
            const Icon = typeInfo.icon;

            return (
              <Card key={method.id} className="hover:shadow-lg transition-shadow">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center">
                      <Icon className={`h-5 w-5 mr-2 ${typeInfo.color}`} />
                      {method.friendlyName || typeInfo.label}
                    </CardTitle>
                    <div className={`w-3 h-3 rounded-full ${
                      method.isActive ? 'bg-green-500' : 'bg-red-500'
                    }`} />
                  </div>
                  <CardDescription className="text-sm">
                    {method.userName} ({method.email})
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div>
                    <span className="text-xs text-gray-600">Typ</span>
                    <div className="text-sm font-medium">{typeInfo.label}</div>
                  </div>

                  <div>
                    <span className="text-xs text-gray-600">Identifikator</span>
                    <div className="text-sm font-mono bg-gray-100 px-2 py-1 rounded">
                      {method.identifier.length > 20 
                        ? `${method.identifier.substring(0, 20)}...` 
                        : method.identifier}
                    </div>
                  </div>

                  {(method.validFrom || method.validUntil) && (
                    <div>
                      <span className="text-xs text-gray-600">Gültigkeit</span>
                      <div className="text-sm">
                        {method.validFrom && `Ab ${new Date(method.validFrom).toLocaleDateString('de-DE')}`}
                        {method.validFrom && method.validUntil && ' - '}
                        {method.validUntil && `Bis ${new Date(method.validUntil).toLocaleDateString('de-DE')}`}
                      </div>
                    </div>
                  )}

                  {method.lastUsedAt && (
                    <div>
                      <span className="text-xs text-gray-600">Zuletzt verwendet</span>
                      <div className="text-sm">
                        {new Date(method.lastUsedAt).toLocaleString('de-DE')}
                      </div>
                    </div>
                  )}

                  <div className="text-xs text-gray-500">
                    Erstellt: {new Date(method.createdAt).toLocaleDateString('de-DE')}
                  </div>

                  <div className="flex space-x-2 pt-2">
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="flex-1"
                      onClick={() => handleEdit(method)}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Bearbeiten
                    </Button>
                    <Button 
                      variant="outline" 
                      size="sm"
                      onClick={() => handleDelete(method.id)}
                      className="text-red-600 hover:text-red-700"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Key className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Identifikationsmethoden vorhanden</h3>
            <p className="text-gray-600 mb-4">Fügen Sie RFID-Karten, Autocharge oder andere Methoden hinzu</p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Methode hinzufügen
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

