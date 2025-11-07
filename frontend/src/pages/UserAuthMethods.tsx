import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Badge } from '../components/ui/badge';
import { CreditCard, Plus, Trash2, Loader2, CheckCircle, AlertCircle, ArrowLeft } from 'lucide-react';
import { api } from '../services/api';

export const UserAuthMethods: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [methods, setMethods] = useState<any[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    type: 'RFID',
    identifier: '',
    friendlyName: ''
  });

  useEffect(() => {
    loadMethods();
  }, []);

  const loadMethods = async () => {
    try {
      setLoading(true);
      const userStr = localStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const data = await api.getAuthorizationMethodsByUser(user.id);
      setMethods(data);
    } catch (error) {
      console.error('Failed to load methods:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateMethod = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const userStr = localStorage.getItem('user');
      if (!userStr) {
        alert('Benutzerinformationen nicht gefunden.');
        return;
      }
      
      const user = JSON.parse(userStr);
      
      // Map type string to enum value
      const typeMap: Record<string, number> = {
        'RFID': 0,
        'Autocharge': 1,
        'App': 2,
        'QRCode': 3,
        'CreditCard': 4,
        'PlugAndCharge': 5
      };
      
      const newMethod = await api.createAuthorizationMethod({
        userId: user.id,
        type: typeMap[formData.type],
        identifier: formData.identifier,
        friendlyName: formData.friendlyName || undefined
      });

      setMethods([...methods, newMethod]);
      setShowForm(false);
      setFormData({
        type: 'RFID',
        identifier: '',
        friendlyName: ''
      });
      alert('Identifikationsmethode erfolgreich hinzugefügt!');
    } catch (error: any) {
      console.error('Failed to create method:', error);
      alert(error.message || 'Fehler beim Hinzufügen der Identifikationsmethode');
    }
  };

  const handleDeleteMethod = async (id: string) => {
    if (!window.confirm('Möchten Sie diese Identifikationsmethode wirklich löschen?')) return;

    try {
      await api.deleteAuthorizationMethod(id);
      setMethods(methods.filter(m => m.id !== id));
    } catch (error) {
      console.error('Failed to delete method:', error);
      alert('Fehler beim Löschen der Identifikationsmethode');
    }
  };

  const getTypeBadgeColor = (type: string) => {
    const colors: Record<string, string> = {
      'RFID': 'bg-blue-100 text-blue-800',
      'Autocharge': 'bg-green-100 text-green-800',
      'App': 'bg-purple-100 text-purple-800',
      'QRCode': 'bg-yellow-100 text-yellow-800',
      'CreditCard': 'bg-orange-100 text-orange-800',
      'PlugAndCharge': 'bg-pink-100 text-pink-800'
    };
    return colors[type] || 'bg-gray-100 text-gray-800';
  };

  const getTypeIcon = (type: string) => {
    return <CreditCard className="h-4 w-4" />;
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => setShowForm(false)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Neue Identifikationsmethode</h1>
            <p className="text-gray-600 mt-1">Fügen Sie eine neue Methode zum Laden hinzu</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Identifikationsmethode hinzufügen</CardTitle>
            <CardDescription>
              Fügen Sie eine RFID-Karte, Autocharge oder andere Methode hinzu
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleCreateMethod} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="type">Typ *</Label>
                <select
                  id="type"
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  required
                >
                  <option value="RFID">RFID-Karte</option>
                  <option value="Autocharge">Autocharge (Plug & Charge)</option>
                  <option value="App">Mobile App</option>
                  <option value="QRCode">QR-Code</option>
                  <option value="CreditCard">Kreditkarte</option>
                </select>
                <p className="text-xs text-gray-500">
                  Wählen Sie die Art der Identifikationsmethode
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="identifier">Identifikator *</Label>
                <Input
                  id="identifier"
                  value={formData.identifier}
                  onChange={(e) => setFormData({ ...formData, identifier: e.target.value })}
                  placeholder="z.B. RFID-Tag-Nummer, VIN, Kartennummer"
                  required
                />
                <p className="text-xs text-gray-500">
                  Die eindeutige Kennung (z.B. RFID-Tag-Nummer, VIN bei Autocharge)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="friendlyName">Anzeigename (optional)</Label>
                <Input
                  id="friendlyName"
                  value={formData.friendlyName}
                  onChange={(e) => setFormData({ ...formData, friendlyName: e.target.value })}
                  placeholder="z.B. Meine RFID-Karte"
                />
                <p className="text-xs text-gray-500">
                  Ein freundlicher Name zur besseren Identifizierung
                </p>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h4 className="font-semibold text-blue-900 mb-2 flex items-center">
                  <CheckCircle className="h-4 w-4 mr-2" />
                  Hinweis
                </h4>
                <p className="text-sm text-blue-800">
                  Nach dem Hinzufügen können Sie diese Methode an allen Ladestationen verwenden, 
                  zu denen Sie über Ihre Nutzergruppen Zugriff haben.
                </p>
              </div>

              <div className="flex justify-end space-x-2 pt-4">
                <Button type="button" variant="outline" onClick={() => setShowForm(false)}>
                  Abbrechen
                </Button>
                <Button type="submit">
                  <Plus className="h-4 w-4 mr-2" />
                  Methode hinzufügen
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Identifikationsmethoden...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Meine Identifikationsmethoden</h1>
          <p className="text-gray-600 mt-1">Verwalten Sie Ihre RFID-Karten, Autocharge und mehr</p>
        </div>
        <Button onClick={() => setShowForm(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Neue Methode
        </Button>
      </div>

      {/* Info Card */}
      <Card className="bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
        <CardContent className="pt-6">
          <div className="flex items-start space-x-4">
            <div className="p-3 bg-blue-100 rounded-lg">
              <CreditCard className="h-6 w-6 text-blue-600" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold text-gray-900 mb-1">Was sind Identifikationsmethoden?</h3>
              <p className="text-sm text-gray-700">
                Identifikationsmethoden ermöglichen es Ihnen, sich an Ladestationen zu authentifizieren. 
                Dies kann eine RFID-Karte, Autocharge (Plug & Charge), eine mobile App oder andere Methoden sein.
              </p>
              <p className="text-sm text-gray-700 mt-2">
                <strong>Wichtig:</strong> Sie benötigen mindestens eine aktive Identifikationsmethode, 
                um an Ladestationen laden zu können!
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Methods Table */}
      <Card>
        <CardHeader>
          <CardTitle>Ihre Identifikationsmethoden</CardTitle>
          <CardDescription>
            {methods.length} Methode{methods.length !== 1 ? 'n' : ''} registriert
          </CardDescription>
        </CardHeader>
        <CardContent>
          {methods.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Typ</TableHead>
                  <TableHead>Anzeigename</TableHead>
                  <TableHead>Identifikator</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Erstellt</TableHead>
                  <TableHead>Zuletzt verwendet</TableHead>
                  <TableHead className="text-right">Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {methods.map((method) => (
                  <TableRow key={method.id}>
                    <TableCell>
                      <Badge className={getTypeBadgeColor(method.type)}>
                        {method.type}
                      </Badge>
                    </TableCell>
                    <TableCell className="font-medium">
                      {method.friendlyName || '-'}
                    </TableCell>
                    <TableCell className="font-mono text-sm">
                      {method.identifier}
                    </TableCell>
                    <TableCell>
                      {method.isActive ? (
                        <Badge className="bg-green-100 text-green-800">
                          <CheckCircle className="h-3 w-3 mr-1" />
                          Aktiv
                        </Badge>
                      ) : (
                        <Badge className="bg-gray-100 text-gray-800">
                          <AlertCircle className="h-3 w-3 mr-1" />
                          Inaktiv
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {new Date(method.createdAt).toLocaleDateString('de-DE')}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {method.lastUsedAt 
                        ? new Date(method.lastUsedAt).toLocaleDateString('de-DE')
                        : 'Nie'}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleDeleteMethod(method.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-12">
              <CreditCard className="h-16 w-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Noch keine Identifikationsmethoden
              </h3>
              <p className="text-gray-600 mb-4">
                Fügen Sie Ihre erste Identifikationsmethode hinzu, um an Ladestationen laden zu können
              </p>
              <Button onClick={() => setShowForm(true)}>
                <Plus className="h-4 w-4 mr-2" />
                Erste Methode hinzufügen
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};


