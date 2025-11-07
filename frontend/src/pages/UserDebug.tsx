import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Loader2, RefreshCw } from 'lucide-react';

export const UserDebug: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [debugData, setDebugData] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  const loadDebugData = async () => {
    try {
      setLoading(true);
      setError(null);
      const token = localStorage.getItem('token');
      
      const response = await fetch('http://localhost:5126/api/user-portal/debug-access', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      setDebugData(data);
    } catch (err: any) {
      console.error('Failed to load debug data:', err);
      setError(err.message || 'Fehler beim Laden der Debug-Daten');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDebugData();
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Debug-Daten...</span>
      </div>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-red-600">Fehler</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-red-600">{error}</p>
          <Button onClick={loadDebugData} className="mt-4">
            <RefreshCw className="h-4 w-4 mr-2" />
            Erneut versuchen
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Debug-Informationen</h1>
          <p className="text-gray-600 mt-1">Zugriffsberechtigungen und Gruppenmitgliedschaften</p>
        </div>
        <Button onClick={loadDebugData} variant="outline">
          <RefreshCw className="h-4 w-4 mr-2" />
          Aktualisieren
        </Button>
      </div>

      {/* Summary */}
      <Card>
        <CardHeader>
          <CardTitle>Zusammenfassung</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4">
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <div className="text-3xl font-bold text-blue-600">
                {debugData?.summary?.userGroupCount || 0}
              </div>
              <div className="text-sm text-gray-600 mt-1">Nutzergruppen</div>
            </div>
            <div className="text-center p-4 bg-green-50 rounded-lg">
              <div className="text-3xl font-bold text-green-600">
                {debugData?.summary?.permissionCount || 0}
              </div>
              <div className="text-sm text-gray-600 mt-1">Berechtigungen</div>
            </div>
            <div className="text-center p-4 bg-purple-50 rounded-lg">
              <div className="text-3xl font-bold text-purple-600">
                {debugData?.summary?.accessibleStationCount || 0}
              </div>
              <div className="text-sm text-gray-600 mt-1">Ladestationen</div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* User Groups */}
      <Card>
        <CardHeader>
          <CardTitle>Meine Nutzergruppen</CardTitle>
          <CardDescription>Gruppen, in denen Sie Mitglied sind</CardDescription>
        </CardHeader>
        <CardContent>
          {debugData?.userGroups?.length > 0 ? (
            <div className="space-y-2">
              {debugData.userGroups.map((group: any) => (
                <div key={group.userGroupId} className="p-3 bg-gray-50 rounded-lg">
                  <div className="font-medium">{group.userGroupName}</div>
                  <div className="text-xs text-gray-500">
                    Group ID: {group.userGroupId}
                  </div>
                  <div className="text-xs text-gray-500">
                    Tenant ID: {group.tenantId}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-500">Keine Nutzergruppen gefunden</p>
          )}
        </CardContent>
      </Card>

      {/* Permissions */}
      <Card>
        <CardHeader>
          <CardTitle>Ladepunkt-Gruppen-Berechtigungen</CardTitle>
          <CardDescription>Zugriff auf Ladepunkt-Gruppen über Nutzergruppen</CardDescription>
        </CardHeader>
        <CardContent>
          {debugData?.permissions?.length > 0 ? (
            <div className="space-y-2">
              {debugData.permissions.map((perm: any, index: number) => (
                <div key={index} className="p-3 bg-green-50 rounded-lg border border-green-200">
                  <div className="font-medium text-green-900">
                    {perm.chargingStationGroupName}
                  </div>
                  <div className="text-xs text-green-700 mt-1">
                    Ladepunkt-Gruppe ID: {perm.chargingStationGroupId}
                  </div>
                  <div className="text-xs text-green-600">
                    Über Nutzergruppe: {perm.userGroupId}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-red-600 font-medium mb-2">⚠️ Keine Berechtigungen gefunden</p>
              <p className="text-sm text-gray-600">
                Sie haben keinen Zugriff auf Ladepunkt-Gruppen. Ein TenantAdmin muss Ihrer 
                Nutzergruppe Berechtigungen für Ladepunkt-Gruppen zuweisen.
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Station Memberships */}
      <Card>
        <CardHeader>
          <CardTitle>Ladestationen in Ladepunkt-Gruppen</CardTitle>
          <CardDescription>Ladestationen, die Sie über Ihre Berechtigungen nutzen können</CardDescription>
        </CardHeader>
        <CardContent>
          {debugData?.stationMemberships?.length > 0 ? (
            <div className="space-y-2">
              {debugData.stationMemberships.map((station: any, index: number) => (
                <div key={index} className="p-3 bg-purple-50 rounded-lg border border-purple-200">
                  <div className="font-medium text-purple-900">
                    {station.chargingStationName}
                  </div>
                  <div className="text-xs text-purple-700 mt-1">
                    Station ID: {station.chargingStationId}
                  </div>
                  <div className="text-xs text-purple-600">
                    In Ladepunkt-Gruppe: {station.chargingStationGroupId}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-red-600 font-medium mb-2">⚠️ Keine Ladestationen gefunden</p>
              <p className="text-sm text-gray-600">
                Die Ladepunkt-Gruppen, auf die Sie Zugriff haben, enthalten keine Ladestationen. 
                Ein TenantAdmin muss Ladestationen zu den Ladepunkt-Gruppen hinzufügen.
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Raw JSON */}
      <Card>
        <CardHeader>
          <CardTitle>Raw JSON (für Entwickler)</CardTitle>
        </CardHeader>
        <CardContent>
          <pre className="bg-gray-100 p-4 rounded-lg overflow-auto text-xs">
            {JSON.stringify(debugData, null, 2)}
          </pre>
        </CardContent>
      </Card>
    </div>
  );
};


