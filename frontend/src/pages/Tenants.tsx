import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Users, Building2, Loader2, Plus, Eye, X } from 'lucide-react';

export const Tenants: React.FC = () => {
  const [tenants, setTenants] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    subdomain: '',
    description: ''
  });
  const navigate = useNavigate();

  const loadTenants = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');
      
      const response = await fetch('http://localhost:5126/api/tenants', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      if (!response.ok) throw new Error('Failed to fetch tenants');
      const data = await response.json();
      
      // Filter out the current user's own tenant
      if (userStr) {
        const user = JSON.parse(userStr);
        const filteredTenants = data.filter((tenant: any) => tenant.id !== user.tenantId);
        setTenants(filteredTenants);
      } else {
        setTenants(data);
      }
    } catch (error) {
      console.error('Failed to load tenants:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTenants();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:5126/api/tenants', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const error = await response.json();
        alert(error.error || 'Fehler beim Erstellen des Tenants');
        return;
      }

      setShowForm(false);
      setFormData({ name: '', subdomain: '', description: '' });
      loadTenants();
      alert('Tenant erfolgreich erstellt!');
    } catch (error) {
      console.error('Failed to create tenant:', error);
      alert('Fehler beim Erstellen des Tenants');
    }
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={() => setShowForm(false)}>
          ← Zurück
        </Button>
        <div className="flex justify-center">
          <Card className="w-full max-w-2xl">
            <CardHeader>
              <CardTitle>Neuer Sub-Tenant</CardTitle>
              <CardDescription>
                Erstellen Sie einen neuen Sub-Tenant unter Ihrem Mandanten
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Firmenname *</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    placeholder="z.B. ACME GmbH"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="subdomain">Subdomain *</Label>
                  <div className="flex items-center space-x-2">
                    <Input
                      id="subdomain"
                      value={formData.subdomain}
                      onChange={(e) => setFormData({ ...formData, subdomain: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') })}
                      placeholder="acme"
                      pattern="[a-z0-9-]+"
                      required
                    />
                    <span className="text-sm text-gray-500">.chargingcontrol.com</span>
                  </div>
                  <p className="text-xs text-gray-500">
                    Nur Kleinbuchstaben, Zahlen und Bindestriche. Dies wird die URL des Tenants.
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Beschreibung</Label>
                  <textarea
                    id="description"
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
                    placeholder="Optionale Beschreibung des Tenants..."
                  />
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <p className="text-sm text-blue-800">
                    <strong>Info:</strong> Der Sub-Tenant wird unter Ihrem aktuellen Tenant erstellt 
                    und kann eigene Benutzer, Ladeparks und Fahrzeuge verwalten.
                  </p>
                </div>

                <div className="flex justify-end space-x-2 pt-4">
                  <Button type="button" variant="outline" onClick={() => setShowForm(false)}>
                    Abbrechen
                  </Button>
                  <Button type="submit">
                    <Plus className="h-4 w-4 mr-2" />
                    Sub-Tenant erstellen
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
          <h1 className="text-3xl font-bold text-gray-900">Tenants</h1>
          <p className="text-gray-600 mt-1">Verwalten Sie Ihre Mandanten und Sub-Tenants</p>
        </div>
        <Button onClick={() => setShowForm(true)} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neuer Sub-Tenant</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Lade Tenants...</span>
        </div>
      ) : tenants.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {tenants.map((tenant) => (
            <Card key={tenant.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">{tenant.name}</CardTitle>
                  <div className={`w-3 h-3 rounded-full ${
                    tenant.isActive ? 'bg-green-500' : 'bg-red-500'
                  }`} />
                </div>
                <CardDescription>
                  <span className="font-mono text-blue-600">{tenant.subdomain}</span>
                  .chargingcontrol.com
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {tenant.description && (
                  <p className="text-sm text-gray-600">{tenant.description}</p>
                )}

                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div className="flex items-center space-x-2">
                    <Users className="h-4 w-4 text-gray-500" />
                    <span className="text-gray-700">{tenant.userCount} Benutzer</span>
                  </div>
                  
                  {tenant.subTenantCount > 0 && (
                    <div className="flex items-center space-x-2">
                      <Building2 className="h-4 w-4 text-gray-500" />
                      <span className="text-gray-700">{tenant.subTenantCount} Sub-Tenants</span>
                    </div>
                  )}
                </div>

                <div className="text-xs text-gray-500">
                  Erstellt: {new Date(tenant.createdAt).toLocaleDateString('de-DE')}
                </div>

                <div className="flex space-x-2 pt-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => navigate(`/tenants/${tenant.id}`)}
                  >
                    <Eye className="h-4 w-4 mr-1" />
                    Details
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Building2 className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Sub-Tenants vorhanden</h3>
            <p className="text-gray-600 mb-4">Erstellen Sie Ihren ersten Sub-Tenant</p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Sub-Tenant erstellen
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
};
