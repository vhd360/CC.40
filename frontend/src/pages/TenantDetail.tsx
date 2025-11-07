import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import { Badge } from '../components/ui/badge';
import { ArrowLeft, Plus, Edit, Trash2, Loader2, Users, Mail, Phone, X } from 'lucide-react';
import { api } from '../services/api';

interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  role?: string;
  isActive: boolean;
  isEmailConfirmed: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

interface Tenant {
  id: string;
  name: string;
  subdomain: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  userCount: number;
}

export const TenantDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tenant, setTenant] = useState<Tenant | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [showUserForm, setShowUserForm] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    role: 'User'
  });

  useEffect(() => {
    const loadData = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const [tenantData, usersData] = await Promise.all([
          api.getTenantById(id),
          api.getUsers(id)
        ]);
        setTenant(tenantData);
        setUsers(usersData);
      } catch (error) {
        console.error('Failed to load data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [id]);

  const handleDeleteUser = async (userId: string) => {
    if (!window.confirm('Möchten Sie diesen Benutzer wirklich löschen?')) return;
    
    try {
      await api.deleteUser(userId);
      setUsers(users.filter(u => u.id !== userId));
    } catch (error) {
      console.error('Failed to delete user:', error);
      alert('Fehler beim Löschen des Benutzers');
    }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    try {
      // Map role string to enum value
      const roleValue = formData.role === 'TenantAdmin' ? 1 : 0; // 0 = User, 1 = TenantAdmin
      
      const newUser = await api.createUser({
        tenantId: id,
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phoneNumber: formData.phoneNumber || undefined,
        password: formData.password,
        role: roleValue.toString()
      });

      setUsers([...users, newUser]);
      setShowUserForm(false);
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        phoneNumber: '',
        password: '',
        role: 'User'
      });
      alert('Benutzer erfolgreich erstellt!');
    } catch (error: any) {
      console.error('Failed to create user:', error);
      alert(error.message || 'Fehler beim Erstellen des Benutzers');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Daten...</span>
      </div>
    );
  }

  if (!tenant) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Tenant nicht gefunden</h3>
        <Button onClick={() => navigate('/tenants')} className="mt-4">
          Zurück zur Übersicht
        </Button>
      </div>
    );
  }

  // Show the create user form if requested
  if (showUserForm) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => setShowUserForm(false)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Neuer Benutzer</h1>
            <p className="text-gray-600 mt-1">Für: {tenant.name}</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Benutzer erstellen</CardTitle>
            <CardDescription>
              Erstellen Sie einen neuen Benutzer für {tenant.name}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleCreateUser} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">Vorname *</Label>
                  <Input
                    id="firstName"
                    value={formData.firstName}
                    onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                    placeholder="Max"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Nachname *</Label>
                  <Input
                    id="lastName"
                    value={formData.lastName}
                    onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                    placeholder="Mustermann"
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">E-Mail *</Label>
                <Input
                  id="email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  placeholder="max@beispiel.de"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="phoneNumber">Telefonnummer</Label>
                <Input
                  id="phoneNumber"
                  type="tel"
                  value={formData.phoneNumber}
                  onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                  placeholder="+49 123 456789"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="password">Passwort *</Label>
                <Input
                  id="password"
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  placeholder="Mindestens 6 Zeichen"
                  required
                  minLength={6}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="role">Rolle *</Label>
                <select
                  id="role"
                  value={formData.role}
                  onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  required
                >
                  <option value="User">Benutzer (User)</option>
                  <option value="TenantAdmin">Tenant Administrator</option>
                </select>
                <p className="text-xs text-gray-500">
                  <strong>Benutzer:</strong> Kann Fahrzeuge und Ladevorgänge verwalten.<br />
                  <strong>Tenant Administrator:</strong> Kann zusätzlich Benutzer und Sub-Tenants verwalten.
                </p>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <p className="text-sm text-blue-800">
                  <strong>Hinweis:</strong> Der Benutzer kann sich mit der angegebenen E-Mail-Adresse und dem Passwort anmelden.
                </p>
              </div>

              <div className="flex justify-end space-x-2 pt-4">
                <Button type="button" variant="outline" onClick={() => setShowUserForm(false)}>
                  Abbrechen
                </Button>
                <Button type="submit">
                  <Plus className="h-4 w-4 mr-2" />
                  Benutzer erstellen
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
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => navigate('/tenants')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{tenant.name}</h1>
            <p className="text-gray-600 mt-1">Subdomain: {tenant.subdomain}</p>
          </div>
        </div>
        <Badge variant={tenant.isActive ? "default" : "secondary"}>
          {tenant.isActive ? 'Aktiv' : 'Inaktiv'}
        </Badge>
      </div>

      {/* Tenant Info */}
      <Card>
        <CardHeader>
          <CardTitle>Tenant-Informationen</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <span className="text-sm text-gray-600">Name</span>
              <div className="font-medium">{tenant.name}</div>
            </div>
            <div>
              <span className="text-sm text-gray-600">Subdomain</span>
              <div className="font-medium">{tenant.subdomain}</div>
            </div>
            {tenant.description && (
              <div className="col-span-2">
                <span className="text-sm text-gray-600">Beschreibung</span>
                <div className="font-medium">{tenant.description}</div>
              </div>
            )}
            <div>
              <span className="text-sm text-gray-600">Erstellt am</span>
              <div className="font-medium">{new Date(tenant.createdAt).toLocaleDateString('de-DE')}</div>
            </div>
            <div>
              <span className="text-sm text-gray-600">Anzahl Benutzer</span>
              <div className="font-medium">{users.length}</div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Users Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Benutzer</CardTitle>
              <CardDescription>Verwalten Sie die Benutzer dieses Tenants</CardDescription>
            </div>
            <Button onClick={() => setShowUserForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Neuer Benutzer
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {users.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>E-Mail</TableHead>
                  <TableHead>Rolle</TableHead>
                  <TableHead>Telefon</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Letzter Login</TableHead>
                  <TableHead className="text-right">Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell className="font-medium">
                      {user.firstName} {user.lastName}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center">
                        <Mail className="h-4 w-4 mr-2 text-gray-400" />
                        {user.email}
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.role === 'TenantAdmin' ? "default" : "secondary"}>
                        {user.role === 'TenantAdmin' ? 'Tenant Admin' : user.role || 'User'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {user.phoneNumber ? (
                        <div className="flex items-center">
                          <Phone className="h-4 w-4 mr-2 text-gray-400" />
                          {user.phoneNumber}
                        </div>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.isActive ? "default" : "secondary"}>
                        {user.isActive ? 'Aktiv' : 'Inaktiv'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {user.lastLoginAt
                        ? new Date(user.lastLoginAt).toLocaleDateString('de-DE')
                        : <span className="text-gray-400">Nie</span>
                      }
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end space-x-2">
                        <Button variant="outline" size="sm">
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => handleDeleteUser(user.id)}
                          className="text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="flex flex-col items-center justify-center py-12">
              <Users className="h-16 w-16 text-gray-300 mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Benutzer vorhanden</h3>
              <p className="text-gray-600 mb-4">Legen Sie den ersten Benutzer für diesen Tenant an</p>
              <Button onClick={() => setShowUserForm(true)}>
                <Plus className="h-4 w-4 mr-2" />
                Benutzer anlegen
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

