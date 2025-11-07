import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Loader2, Zap, Clock, Battery, MapPin, Car, Eye } from 'lucide-react';
import { api } from '../services/api';

export const UserSessions: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [sessions, setSessions] = useState<any[]>([]);

  useEffect(() => {
    loadSessions();
  }, []);

  const loadSessions = async () => {
    try {
      setLoading(true);
      const data = await api.getUserChargingSessions();
      setSessions(data);
    } catch (error) {
      console.error('Failed to load sessions:', error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      'Completed': 'bg-green-100 text-green-800',
      'Active': 'bg-blue-100 text-blue-800',
      'Failed': 'bg-red-100 text-red-800',
      'Cancelled': 'bg-gray-100 text-gray-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Ladevorgänge...</span>
      </div>
    );
  }

  // Calculate summary stats
  const totalSessions = sessions.length;
  const completedSessions = sessions.filter(s => s.status === 'Completed').length;
  const totalEnergy = sessions.reduce((sum, s) => sum + (s.energyDelivered || 0), 0);
  const totalCost = sessions.reduce((sum, s) => sum + (s.cost || 0), 0);
  const avgDuration = totalSessions > 0 
    ? sessions.reduce((sum, s) => sum + s.duration, 0) / totalSessions 
    : 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Meine Ladevorgänge</h1>
        <p className="text-gray-600 mt-1">Übersicht aller durchgeführten Ladevorgänge</p>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Gesamt</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalSessions}</div>
            <p className="text-xs text-gray-500">{completedSessions} abgeschlossen</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Energie</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalEnergy.toFixed(1)} kWh</div>
            <p className="text-xs text-gray-500">Gesamt geladen</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Kosten</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">€{totalCost.toFixed(2)}</div>
            <p className="text-xs text-gray-500">Gesamt</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Ø Dauer</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {Math.floor(avgDuration / 60)}h {Math.round(avgDuration % 60)}m
            </div>
            <p className="text-xs text-gray-500">Pro Ladevorgang</p>
          </CardContent>
        </Card>
      </div>

      {/* Sessions Table */}
      <Card>
        <CardHeader>
          <CardTitle>Alle Ladevorgänge</CardTitle>
          <CardDescription>Chronologische Übersicht Ihrer Ladeaktivitäten</CardDescription>
        </CardHeader>
        <CardContent>
          {sessions.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Datum & Zeit</TableHead>
                  <TableHead>Ladestation</TableHead>
                  <TableHead>Fahrzeug</TableHead>
                  <TableHead>Dauer</TableHead>
                  <TableHead>Energie</TableHead>
                  <TableHead>Kosten</TableHead>
                  <TableHead>Auth-Methode</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sessions.map((session) => (
                  <TableRow key={session.id}>
                    <TableCell>
                      <div className="text-sm">
                        <div className="font-medium">
                          {new Date(session.startedAt).toLocaleDateString('de-DE')}
                        </div>
                        <div className="text-gray-500 text-xs">
                          {new Date(session.startedAt).toLocaleTimeString('de-DE', {
                            hour: '2-digit',
                            minute: '2-digit'
                          })}
                          {session.endedAt && (
                            <> - {new Date(session.endedAt).toLocaleTimeString('de-DE', {
                              hour: '2-digit',
                              minute: '2-digit'
                            })}</>
                          )}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm">
                        <div className="font-medium flex items-center space-x-1">
                          <MapPin className="h-3 w-3 text-gray-400" />
                          <span>{session.station.name}</span>
                        </div>
                        <div className="text-gray-500 text-xs">
                          {session.station.chargingPark.name}
                        </div>
                        <div className="text-gray-400 text-xs">
                          {session.station.chargingPark.city}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      {session.vehicle ? (
                        <div className="text-sm">
                          <div className="font-medium flex items-center space-x-1">
                            <Car className="h-3 w-3 text-gray-400" />
                            <span>{session.vehicle.make} {session.vehicle.model}</span>
                          </div>
                          <div className="text-gray-500 text-xs">
                            {session.vehicle.licensePlate}
                          </div>
                        </div>
                      ) : (
                        <span className="text-gray-400 text-sm">-</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="text-sm flex items-center space-x-1">
                        <Clock className="h-3 w-3 text-gray-400" />
                        <span className="font-medium">
                          {Math.floor(session.duration / 60)}h {session.duration % 60}m
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm flex items-center space-x-1">
                        <Battery className="h-3 w-3 text-green-600" />
                        <span className="font-medium">
                          {session.energyDelivered ? `${session.energyDelivered.toFixed(1)} kWh` : '-'}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm font-medium">
                        €{session.cost ? session.cost.toFixed(2) : '0.00'}
                      </div>
                    </TableCell>
                    <TableCell>
                      {session.authorizationMethod ? (
                        <div className="text-xs">
                          <Badge variant="outline">
                            {session.authorizationMethod.type}
                          </Badge>
                          <div className="text-gray-500 mt-1">
                            {session.authorizationMethod.friendlyName || session.authorizationMethod.identifier}
                          </div>
                        </div>
                      ) : (
                        <span className="text-gray-400 text-sm">-</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge className={getStatusBadge(session.status)}>
                        {session.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => navigate(`/user/sessions/${session.id}`)}
                        className="flex items-center space-x-1"
                      >
                        <Eye className="h-4 w-4" />
                        <span>Details</span>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-12">
              <Zap className="h-16 w-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">Noch keine Ladevorgänge</h3>
              <p className="text-gray-600">
                Starten Sie Ihren ersten Ladevorgang an einer verfügbaren Ladestation
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};


