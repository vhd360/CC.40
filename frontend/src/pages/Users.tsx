import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import { Badge } from '../components/ui/badge';
import { ArrowLeft, Plus, Edit, Trash2, Loader2, Users as UsersIcon, Mail, Phone } from 'lucide-react';
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
  isGuest?: boolean;
  groupMemberships?: string[];
}

export const Users: React.FC = () => {
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
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const userStr = localStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const usersData = await api.getUsers(user.tenantId);
      setUsers(usersData);
    } catch (error) {
      console.error('Failed to load users:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUser = async (userId: string) => {
    const user = users.find(u => u.id === userId);
    if (!user) return;

    const isGuest = user.isGuest || false;
    const confirmMessage = isGuest
      ? 'Möchten Sie diesen eingeladenen Benutzer wirklich aus allen Nutzergruppen entfernen?'
      : 'Möchten Sie diesen Benutzer wirklich deaktivieren?';

    if (!window.confirm(confirmMessage)) return;
    
    try {
      if (isGuest) {
        const result = await api.removeGuestUserFromTenant(userId);
        console.log('Removed from groups:', result);
        alert(`Benutzer wurde aus ${result.removedMemberships} Gruppe(n) entfernt: ${result.groups.join(', ')}`);
      } else {
        await api.deleteUser(userId);
      }
      setUsers(users.filter(u => u.id !== userId));
    } catch (error: any) {
      console.error('Failed to delete user:', error);
      alert(error.message || 'Fehler beim Löschen des Benutzers');
    }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const userStr = localStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      
      // Map role string to enum value
      const roleValue = formData.role === 'TenantAdmin' ? 1 : 0; // 0 = User, 1 = TenantAdmin
      
      const newUser = await api.createUser({
        tenantId: user.tenantId,
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

  // Show create user form
  if (showUserForm) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={() => setShowUserForm(false)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Neuer Benutzer</h1>
            <p className="text-gray-600 dark:text-gray-400 mt-1">Fügen Sie einen neuen Benutzer hinzu</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Benutzer erstellen</CardTitle>
            <CardDescription>
              Erstellen Sie einen neuen Benutzer für Ihren Tenant
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
                <p className="text-xs text-gray-500 dark:text-gray-400">
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
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Benutzer</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Verwalten Sie die Benutzer Ihres Tenants</p>
        </div>
        <Button onClick={() => setShowUserForm(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Neuer Benutzer
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Benutzer...</span>
        </div>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Benutzerübersicht</CardTitle>
            <CardDescription>Alle Benutzer Ihres Tenants</CardDescription>
          </CardHeader>
          <CardContent>
            {users.length > 0 ? (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>E-Mail</TableHead>
                    <TableHead>Typ</TableHead>
                    <TableHead>Rolle</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Letzter Login</TableHead>
                    <TableHead className="text-right">Aktionen</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id} className={user.isGuest ? 'bg-indigo-50' : ''}>
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
                        {user.isGuest ? (
                          <div>
                            <Badge variant="outline" className="bg-indigo-100 text-indigo-700 border-indigo-300">
                              Eingeladen
                            </Badge>
                            {user.groupMemberships && user.groupMemberships.length > 0 && (
                              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                                Gruppen: {user.groupMemberships.join(', ')}
                              </div>
                            )}
                          </div>
                        ) : (
                          <Badge variant="secondary">
                            Eigener Benutzer
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge variant={user.role === 'TenantAdmin' ? "default" : "secondary"}>
                          {user.role === 'TenantAdmin' ? 'Tenant Admin' : user.role || 'User'}
                        </Badge>
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
                          {!user.isGuest && (
                            <Button variant="outline" size="sm">
                              <Edit className="h-4 w-4" />
                            </Button>
                          )}
                          <Button 
                            variant="outline" 
                            size="sm"
                            onClick={() => handleDeleteUser(user.id)}
                            className="text-red-600 hover:text-red-700"
                            title={user.isGuest ? 'Aus Nutzergruppen entfernen' : 'Benutzer deaktivieren'}
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
                <UsersIcon className="h-16 w-16 text-gray-300 mb-4" />
                <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">Keine Benutzer vorhanden</h3>
                <p className="text-gray-600 dark:text-gray-400 mb-4">Legen Sie den ersten Benutzer für Ihren Tenant an</p>
                <Button onClick={() => setShowUserForm(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  Benutzer anlegen
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
};

