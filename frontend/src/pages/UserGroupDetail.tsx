import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
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
import { ArrowLeft, Users, Shield, Plus, Trash2, Loader2, CheckCircle2, QrCode, X, Copy, Check, Layers } from 'lucide-react';
import { api, Permission } from '../services/api';
import { QRCodeSVG } from 'qrcode.react';

interface UserGroupDetails {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  members: Array<{
    userId: string;
    userName: string;
    email: string;
    assignedAt: string;
  }>;
  permissions: Array<{
    permissionId: string;
    name: string;
    description?: string;
    assignedAt: string;
  }>;
}

export const UserGroupDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [group, setGroup] = useState<UserGroupDetails | null>(null);
  const [allPermissions, setAllPermissions] = useState<Permission[]>([]);
  const [allUsers, setAllUsers] = useState<any[]>([]);
  const [showAddPermission, setShowAddPermission] = useState(false);
  const [showAddUser, setShowAddUser] = useState(false);
  const [showQrCode, setShowQrCode] = useState(false);
  const [qrCodeData, setQrCodeData] = useState<{token: string; expiresAt: string; inviteUrl: string} | null>(null);
  const [copied, setCopied] = useState(false);
  
  // Charging Station Groups
  const [stationPermissions, setStationPermissions] = useState<any[]>([]);
  const [allStationGroups, setAllStationGroups] = useState<any[]>([]);
  const [showAddStationPermission, setShowAddStationPermission] = useState(false);

  useEffect(() => {
    if (id) {
      loadGroupDetails();
      loadAllPermissions();
      loadAllUsers();
      loadStationPermissions();
      loadAllStationGroups();
    }
  }, [id]);

  const loadGroupDetails = async () => {
    try {
      setLoading(true);
      const data = await api.getUserGroupDetails(id!);
      setGroup(data);
    } catch (error) {
      console.error('Failed to load group details:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadAllPermissions = async () => {
    try {
      const data = await api.getAllPermissions();
      setAllPermissions(data);
    } catch (error) {
      console.error('Failed to load permissions:', error);
    }
  };

  const loadAllUsers = async () => {
    try {
      const userStr = localStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const data = await api.getUsers(user.tenantId);
      setAllUsers(data);
    } catch (error) {
      console.error('Failed to load users:', error);
    }
  };

  const loadStationPermissions = async () => {
    try {
      const data = await api.getUserGroupStationPermissions(id!);
      setStationPermissions(data);
    } catch (error) {
      console.error('Failed to load station permissions:', error);
    }
  };

  const loadAllStationGroups = async () => {
    try {
      const data = await api.getChargingStationGroups();
      setAllStationGroups(data);
    } catch (error) {
      console.error('Failed to load station groups:', error);
    }
  };

  const handleAddPermission = async (permissionId: string) => {
    try {
      await api.addPermissionToGroup(id!, permissionId);
      await loadGroupDetails();
      setShowAddPermission(false);
    } catch (error) {
      console.error('Failed to add permission:', error);
      alert('Fehler beim Hinzufügen der Berechtigung');
    }
  };

  const handleRemovePermission = async (permissionId: string) => {
    if (!window.confirm('Möchten Sie diese Berechtigung wirklich entfernen?')) return;

    try {
      await api.removePermissionFromGroup(id!, permissionId);
      await loadGroupDetails();
    } catch (error) {
      console.error('Failed to remove permission:', error);
      alert('Fehler beim Entfernen der Berechtigung');
    }
  };

  const handleAddUser = async (userId: string) => {
    try {
      await api.addUserToGroup(id!, userId);
      await loadGroupDetails();
      setShowAddUser(false);
    } catch (error) {
      console.error('Failed to add user:', error);
      alert('Fehler beim Hinzufügen des Benutzers');
    }
  };

  const handleRemoveUser = async (userId: string) => {
    if (!window.confirm('Möchten Sie diesen Benutzer wirklich aus der Gruppe entfernen?')) return;

    try {
      await api.removeUserFromGroup(id!, userId);
      await loadGroupDetails();
    } catch (error) {
      console.error('Failed to remove user:', error);
      alert('Fehler beim Entfernen des Benutzers');
    }
  };

  const handleAddStationPermission = async (stationGroupId: string) => {
    try {
      await api.addStationPermissionToUserGroup(id!, stationGroupId);
      await loadStationPermissions();
      setShowAddStationPermission(false);
    } catch (error) {
      console.error('Failed to add station permission:', error);
      alert('Fehler beim Hinzufügen der Ladepunkt-Gruppe');
    }
  };

  const handleRemoveStationPermission = async (stationGroupId: string) => {
    if (!window.confirm('Möchten Sie diese Ladepunkt-Gruppe wirklich entfernen?')) return;

    try {
      await api.removeStationPermissionFromUserGroup(id!, stationGroupId);
      await loadStationPermissions();
    } catch (error) {
      console.error('Failed to remove station permission:', error);
      alert('Fehler beim Entfernen der Ladepunkt-Gruppe');
    }
  };

  const getAvailablePermissions = () => {
    if (!group) return [];
    const assignedIds = group.permissions.map(p => p.permissionId);
    return allPermissions.filter(p => !assignedIds.includes(p.id));
  };

  const getAvailableUsers = () => {
    if (!group) return [];
    const assignedUserIds = group.members.map(m => m.userId);
    return allUsers.filter(u => !assignedUserIds.includes(u.id));
  };

  const getAvailableStationGroups = () => {
    const assignedIds = stationPermissions.map(p => p.chargingStationGroupId);
    return allStationGroups.filter(g => !assignedIds.includes(g.id));
  };

  const groupByResource = (permissions: Permission[]) => {
    const grouped: Record<string, Permission[]> = {};
    permissions.forEach(p => {
      if (!grouped[p.resource]) {
        grouped[p.resource] = [];
      }
      grouped[p.resource].push(p);
    });
    return grouped;
  };

  const handleGenerateQrCode = async () => {
    try {
      const data = await api.generateGroupInvite(id!, 7); // 7 days expiry
      setQrCodeData(data);
      setShowQrCode(true);
    } catch (error) {
      console.error('Failed to generate invite:', error);
      alert('Fehler beim Generieren des Einladungslinks');
    }
  };

  const handleCopyLink = () => {
    if (qrCodeData) {
      const fullUrl = `${window.location.origin}${qrCodeData.inviteUrl}`;
      navigator.clipboard.writeText(fullUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleRevokeInvite = async () => {
    if (!window.confirm('Möchten Sie den Einladungslink wirklich widerrufen?')) return;
    
    try {
      await api.revokeGroupInvite(id!);
      setQrCodeData(null);
      setShowQrCode(false);
      alert('Einladungslink wurde widerrufen');
    } catch (error) {
      console.error('Failed to revoke invite:', error);
      alert('Fehler beim Widerrufen des Einladungslinks');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!group) {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-800">Nutzergruppe nicht gefunden</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="outline" size="sm" onClick={() => navigate('/user-groups')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Zurück
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{group.name}</h1>
            {group.description && (
              <p className="text-gray-600 mt-1">{group.description}</p>
            )}
          </div>
        </div>
        <div className="flex items-center space-x-2">
          <Button onClick={handleGenerateQrCode} variant="outline">
            <QrCode className="h-4 w-4 mr-2" />
            QR-Code Einladung
          </Button>
          <Badge variant={group.isActive ? "default" : "secondary"}>
            {group.isActive ? 'Aktiv' : 'Inaktiv'}
          </Badge>
        </div>
      </div>

      {/* QR Code Dialog */}
      {showQrCode && qrCodeData && (
        <Card className="border-2 border-blue-500">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="flex items-center">
                  <QrCode className="h-5 w-5 mr-2 text-blue-600" />
                  Einladungs-QR-Code
                </CardTitle>
                <CardDescription>
                  Teilen Sie diesen QR-Code oder Link mit Benutzern, damit sie der Gruppe beitreten können
                </CardDescription>
              </div>
              <Button variant="ghost" size="sm" onClick={() => setShowQrCode(false)}>
                <X className="h-4 w-4" />
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* QR Code */}
              <div className="flex flex-col items-center space-y-4">
                <div className="p-4 bg-white rounded-lg shadow">
                  <QRCodeSVG 
                    value={`${window.location.origin}${qrCodeData.inviteUrl}`}
                    size={200}
                    level="H"
                    includeMargin
                  />
                </div>
                <p className="text-xs text-gray-500 text-center">
                  Gültig bis: {new Date(qrCodeData.expiresAt).toLocaleString('de-DE')}
                </p>
              </div>

              {/* Link & Actions */}
              <div className="space-y-4">
                <div>
                  <Label>Einladungslink</Label>
                  <div className="flex items-center space-x-2 mt-2">
                    <Input
                      value={`${window.location.origin}${qrCodeData.inviteUrl}`}
                      readOnly
                      className="font-mono text-sm"
                    />
                    <Button onClick={handleCopyLink} variant="outline" size="sm">
                      {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                    </Button>
                  </div>
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <h4 className="font-semibold text-blue-900 mb-2">So funktioniert's:</h4>
                  <ul className="text-sm text-blue-800 space-y-1">
                    <li>• Scannen Sie den QR-Code mit einem Smartphone</li>
                    <li>• Oder kopieren Sie den Link und teilen Sie ihn</li>
                    <li>• Benutzer können sich selbst zur Gruppe hinzufügen</li>
                    <li>• Der Link ist 7 Tage gültig</li>
                  </ul>
                </div>

                <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
                  <h4 className="font-semibold text-amber-900 mb-2">⚠️ Wichtiger Hinweis:</h4>
                  <p className="text-sm text-amber-800">
                    Benutzer, die über den QR-Code beitreten, müssen anschließend eine 
                    <strong> Identifikationsmethode</strong> (z.B. RFID-Karte) hinzufügen, 
                    um sich an den Ladestationen authentifizieren zu können.
                  </p>
                </div>

                <Button 
                  onClick={handleRevokeInvite} 
                  variant="outline" 
                  className="w-full text-red-600 hover:text-red-700"
                >
                  Einladungslink widerrufen
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Mitglieder</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{group.members.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Berechtigungen</CardTitle>
            <Shield className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{group.permissions.length}</div>
          </CardContent>
        </Card>
      </div>

      {/* Berechtigungen */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Berechtigungen</CardTitle>
              <CardDescription>Verwalten Sie die Zugriffsrechte dieser Gruppe</CardDescription>
            </div>
            <Button onClick={() => setShowAddPermission(!showAddPermission)}>
              <Plus className="h-4 w-4 mr-2" />
              Berechtigung hinzufügen
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {showAddPermission && (
            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
              <h3 className="font-semibold mb-3">Verfügbare Berechtigungen</h3>
              {Object.entries(groupByResource(getAvailablePermissions())).map(([resource, perms]) => (
                <div key={resource} className="mb-4">
                  <h4 className="text-sm font-medium text-gray-700 mb-2 capitalize">{resource}</h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                    {perms.map(permission => (
                      <div
                        key={permission.id}
                        className="flex items-center justify-between p-2 bg-white rounded border hover:border-blue-500 cursor-pointer"
                        onClick={() => handleAddPermission(permission.id)}
                      >
                        <div>
                          <p className="text-sm font-medium">{permission.name}</p>
                          {permission.description && (
                            <p className="text-xs text-gray-500">{permission.description}</p>
                          )}
                        </div>
                        <Plus className="h-4 w-4 text-blue-600" />
                      </div>
                    ))}
                  </div>
                </div>
              ))}
              {getAvailablePermissions().length === 0 && (
                <div className="text-center py-4 text-gray-500">
                  <CheckCircle2 className="h-8 w-8 mx-auto mb-2 text-green-600" />
                  <p>Alle Berechtigungen wurden bereits zugewiesen</p>
                </div>
              )}
            </div>
          )}

          {group.permissions.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Berechtigung</TableHead>
                  <TableHead>Beschreibung</TableHead>
                  <TableHead>Zugewiesen am</TableHead>
                  <TableHead className="text-right">Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {group.permissions.map((permission) => (
                  <TableRow key={permission.permissionId}>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Shield className="h-4 w-4 text-blue-600" />
                        <span className="font-medium">{permission.name}</span>
                      </div>
                    </TableCell>
                    <TableCell className="text-gray-600">
                      {permission.description || '-'}
                    </TableCell>
                    <TableCell className="text-sm text-gray-500">
                      {new Date(permission.assignedAt).toLocaleDateString('de-DE')}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleRemovePermission(permission.permissionId)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Shield className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p>Keine Berechtigungen zugewiesen</p>
              <p className="text-sm mt-1">Klicken Sie auf "Berechtigung hinzufügen", um zu beginnen</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Mitglieder */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Mitglieder</CardTitle>
              <CardDescription>Verwalten Sie die Benutzer in dieser Gruppe</CardDescription>
            </div>
            <Button onClick={() => setShowAddUser(!showAddUser)}>
              <Plus className="h-4 w-4 mr-2" />
              Benutzer hinzufügen
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {showAddUser && (
            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
              <h3 className="font-semibold mb-3">Verfügbare Benutzer</h3>
              {getAvailableUsers().length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                  {getAvailableUsers().map((user: any) => (
                    <div
                      key={user.id}
                      className="flex items-center justify-between p-3 bg-white rounded border hover:border-blue-500 cursor-pointer"
                      onClick={() => handleAddUser(user.id)}
                    >
                      <div>
                        <p className="text-sm font-medium">{user.firstName} {user.lastName}</p>
                        <p className="text-xs text-gray-500">{user.email}</p>
                      </div>
                      <Plus className="h-4 w-4 text-blue-600" />
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-4 text-gray-500">
                  <CheckCircle2 className="h-8 w-8 mx-auto mb-2 text-green-600" />
                  <p>Alle Benutzer sind bereits in dieser Gruppe</p>
                </div>
              )}
            </div>
          )}

          {group.members.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>E-Mail</TableHead>
                  <TableHead>Hinzugefügt am</TableHead>
                  <TableHead className="text-right">Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {group.members.map((member) => (
                  <TableRow key={member.userId}>
                    <TableCell className="font-medium">{member.userName}</TableCell>
                    <TableCell>{member.email}</TableCell>
                    <TableCell className="text-sm text-gray-500">
                      {new Date(member.assignedAt).toLocaleDateString('de-DE')}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleRemoveUser(member.userId)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Users className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p>Keine Mitglieder in dieser Gruppe</p>
              <p className="text-sm mt-1">Klicken Sie auf "Benutzer hinzufügen", um zu beginnen</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Ladepunkt-Gruppen */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Ladepunkt-Gruppen</CardTitle>
              <CardDescription>Verwalten Sie den Zugriff auf Ladepunkt-Gruppen</CardDescription>
            </div>
            <Button onClick={() => setShowAddStationPermission(!showAddStationPermission)}>
              <Plus className="h-4 w-4 mr-2" />
              Ladepunkt-Gruppe hinzufügen
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {showAddStationPermission && (
            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
              <h3 className="font-semibold mb-3">Verfügbare Ladepunkt-Gruppen</h3>
              {getAvailableStationGroups().length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                  {getAvailableStationGroups().map((group: any) => (
                    <div
                      key={group.id}
                      className="flex items-center justify-between p-3 bg-white rounded border hover:border-blue-500 cursor-pointer"
                      onClick={() => handleAddStationPermission(group.id)}
                    >
                      <div>
                        <p className="text-sm font-medium">{group.name}</p>
                        <p className="text-xs text-gray-500">{group.description || 'Keine Beschreibung'}</p>
                      </div>
                      <Plus className="h-4 w-4 text-blue-600" />
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-4 text-gray-500">
                  <p>Alle verfügbaren Ladepunkt-Gruppen sind bereits zugewiesen</p>
                </div>
              )}
            </div>
          )}

          {stationPermissions.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ladepunkt-Gruppe</TableHead>
                  <TableHead>Zugewiesen am</TableHead>
                  <TableHead className="text-right">Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {stationPermissions.map((permission) => (
                  <TableRow key={permission.id}>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Layers className="h-4 w-4 text-indigo-600" />
                        <span className="font-medium">{permission.chargingStationGroupName}</span>
                      </div>
                    </TableCell>
                    <TableCell className="text-sm text-gray-500">
                      {new Date(permission.assignedAt).toLocaleDateString('de-DE')}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleRemoveStationPermission(permission.chargingStationGroupId)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Layers className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p>Keine Ladepunkt-Gruppen zugewiesen</p>
              <p className="text-sm mt-1">Klicken Sie auf "Ladepunkt-Gruppe hinzufügen", um zu beginnen</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};
