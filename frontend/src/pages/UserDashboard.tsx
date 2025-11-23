import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { 
  Zap, 
  Euro, 
  Battery, 
  MapPin, 
  Clock,
  TrendingUp,
  Loader2,
  CreditCard,
  Calendar,
  Eye,
  Car
} from 'lucide-react';
import { api, VehicleAssignment } from '../services/api';
import { Badge } from '../components/ui/badge';
import { useSignalRContext } from '../contexts/SignalRContext';
import { useToast } from '../components/ui/toast';
import { ConfirmDialog } from '../components/ConfirmDialog';

export const UserDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isConnected, onSessionUpdate, onStationStatusChanged } = useSignalRContext();
  const [loading, setLoading] = useState(true);
  const [dashboardData, setDashboardData] = useState<any>(null);
  const [recentSessions, setRecentSessions] = useState<any[]>([]);
  const [myVehicles, setMyVehicles] = useState<VehicleAssignment[]>([]);
  const [activeSessions, setActiveSessions] = useState<any[]>([]);
  const [stopping, setStopping] = useState<string | null>(null);
  const [stopConfirm, setStopConfirm] = useState<{ open: boolean; sessionId: string | null }>({
    open: false,
    sessionId: null
  });
  const { showToast } = useToast();

  useEffect(() => {
    loadDashboard();
    // Fallback: Reload active sessions every 30 seconds if SignalR is not connected
    const interval = setInterval(() => {
      if (!isConnected) {
        loadActiveSessions();
      }
    }, 30000);
    return () => clearInterval(interval);
  }, [isConnected]);

  // SignalR: Session Updates
  useEffect(() => {
    if (!isConnected) return;

    const handleSessionUpdate = (notification: any) => {
      console.log('📡 Session Update received:', notification);
      
      // Reload active sessions and dashboard data
      loadActiveSessions();
      loadDashboard();
    };

    const unsubscribe = onSessionUpdate(handleSessionUpdate);
    return () => unsubscribe();
  }, [isConnected, onSessionUpdate]);

  // SignalR: Station Status Updates (affects active sessions)
  useEffect(() => {
    if (!isConnected) return;

    const handleStationUpdate = (notification: any) => {
      console.log('📡 Station Status Update received:', notification);
      
      // Update active sessions if they involve this station
      const hasActiveSessionsOnStation = activeSessions.some(
        session => session.station.id === notification.StationId
      );
      
      if (hasActiveSessionsOnStation) {
        loadActiveSessions();
      }
    };

    const unsubscribe = onStationStatusChanged(handleStationUpdate);
    return () => unsubscribe();
  }, [isConnected, onStationStatusChanged, activeSessions]);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const [dashboard, sessions, vehicles, active] = await Promise.all([
        api.getUserDashboard(),
        api.getUserChargingSessions(10),
        api.getMyVehicles().catch(() => []), // Ignore errors if no vehicles
        api.getActiveSessions().catch(() => [])
      ]);
      setDashboardData(dashboard);
      setRecentSessions(sessions);
      setMyVehicles(vehicles);
      setActiveSessions(active);
    } catch (error) {
      console.error('Failed to load dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadActiveSessions = async () => {
    try {
      const active = await api.getActiveSessions();
      setActiveSessions(active);
    } catch (error) {
      console.error('Failed to load active sessions:', error);
    }
  };

  const handleStopCharging = (sessionId: string) => {
    setStopConfirm({ open: true, sessionId });
  };

  const handleStopConfirm = async () => {
    if (!stopConfirm.sessionId) return;

    try {
      setStopping(stopConfirm.sessionId);
      await api.stopChargingSession(stopConfirm.sessionId);
      // Reload data
      await Promise.all([loadDashboard(), loadActiveSessions()]);
      showToast('Ladevorgang erfolgreich beendet', 'success');
    } catch (error: any) {
      console.error('Failed to stop charging:', error);
      showToast(error.message || 'Fehler beim Stoppen des Ladevorgangs', 'error');
    } finally {
      setStopping(null);
      setStopConfirm({ open: false, sessionId: null });
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Dashboard...</span>
      </div>
    );
  }

  if (!dashboardData) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">Fehler beim Laden des Dashboards</p>
      </div>
    );
  }

  const stats = [
    {
      title: 'Verfügbare Ladestationen',
      value: dashboardData.availableStations,
      subtitle: 'Tenantübergreifend',
      icon: MapPin,
      color: 'text-blue-600',
      bgColor: 'bg-blue-100'
    },
    {
      title: 'Ladevorgänge gesamt',
      value: dashboardData.totalSessions,
      subtitle: `${dashboardData.sessionsThisMonth} diesen Monat`,
      icon: Zap,
      color: 'text-green-600',
      bgColor: 'bg-green-100'
    },
    {
      title: 'Energie geladen',
      value: `${dashboardData.totalEnergy.toFixed(1)} kWh`,
      subtitle: 'Gesamt',
      icon: Battery,
      color: 'text-purple-600',
      bgColor: 'bg-purple-100'
    },
    {
      title: 'Kosten gesamt',
      value: `€${dashboardData.totalCosts.toFixed(2)}`,
      subtitle: `€${dashboardData.costsThisMonth.toFixed(2)} diesen Monat`,
      icon: Euro,
      color: 'text-orange-600',
      bgColor: 'bg-orange-100'
    }
  ];

  const quickActions = [
    {
      title: 'Ladestationen finden',
      description: 'Zeige alle verfügbaren Ladestationen',
      icon: MapPin,
      onClick: () => navigate('/user/stations'),
      color: 'bg-blue-600 hover:bg-blue-700'
    },
    {
      title: 'Ladevorgänge',
      description: 'Meine Lade-Historie anzeigen',
      icon: Clock,
      onClick: () => navigate('/user/sessions'),
      color: 'bg-green-600 hover:bg-green-700'
    },
    {
      title: 'Kosten & Abrechnung',
      description: 'Kostenübersicht anzeigen',
      icon: Euro,
      onClick: () => navigate('/user/costs'),
      color: 'bg-orange-600 hover:bg-orange-700'
    },
    {
      title: 'Identifikationsmethoden',
      description: 'RFID-Karten & mehr verwalten',
      icon: CreditCard,
      onClick: () => navigate('/user/auth-methods'),
      color: 'bg-purple-600 hover:bg-purple-700'
    }
  ];

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      'Completed': 'bg-green-100 text-green-800',
      'Active': 'bg-blue-100 text-blue-800',
      'Failed': 'bg-red-100 text-red-800',
      'Cancelled': 'bg-gray-100 text-gray-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Willkommen zurück!</h1>
        <p className="text-gray-600 mt-1">Hier ist eine Übersicht Ihrer Ladeaktivitäten</p>
      </div>

      {/* Active Session Alert */}
      {dashboardData.activeSession > 0 && (
        <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded-lg">
          <div className="flex items-center">
            <Zap className="h-6 w-6 text-blue-600 mr-3 animate-pulse" />
            <div>
              <p className="text-blue-900 font-semibold">Aktiver Ladevorgang</p>
              <p className="text-blue-700 text-sm">Sie haben gerade einen aktiven Ladevorgang</p>
            </div>
          </div>
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat, index) => (
          <Card key={index} className="hover:shadow-lg transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
              <div className={`p-2 rounded-lg ${stat.bgColor}`}>
                <stat.icon className={`h-5 w-5 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p className="text-xs text-gray-600 mt-1">{stat.subtitle}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Schnellzugriff</CardTitle>
          <CardDescription>Häufig verwendete Funktionen</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {quickActions.map((action, index) => (
              <button
                key={index}
                onClick={action.onClick}
                className={`${action.color} text-white p-4 rounded-lg flex items-start space-x-4 transition-all text-left`}
              >
                <action.icon className="h-6 w-6 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="font-semibold">{action.title}</h3>
                  <p className="text-sm opacity-90 mt-1">{action.description}</p>
                </div>
              </button>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Active Charging Sessions */}
      {activeSessions.length > 0 && (
        <Card className="border-primary/20">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="flex items-center gap-2">
                  <Zap className="h-5 w-5 text-primary" />
                  Aktive Ladevorgänge
                </CardTitle>
                <CardDescription>
                  {activeSessions.length} Ladevorgang{activeSessions.length !== 1 ? 'e' : ''} läuft gerade
                </CardDescription>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {activeSessions.map((session) => (
                <div 
                  key={session.id}
                  className="border border-primary/30 bg-primary/5 rounded-lg p-4"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <Badge className="bg-green-500 hover:bg-green-600">
                          Läuft
                        </Badge>
                        <span className="text-sm text-gray-600 dark:text-gray-400">
                          seit {session.durationMinutes} Min
                        </span>
                      </div>
                      <h4 className="font-semibold text-gray-900 dark:text-gray-100 mb-1">
                        {session.station.name}
                      </h4>
                      <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                        {session.station.chargingPark.name}
                      </p>
                      {session.vehicle && (
                        <div className="flex items-center gap-2 text-sm">
                          <Car className="h-4 w-4 text-gray-500 dark:text-gray-400" />
                          <span className="text-gray-700 dark:text-gray-300">
                            {session.vehicle.make} {session.vehicle.model}
                          </span>
                          <span className="text-gray-500 dark:text-gray-400 font-mono">
                            {session.vehicle.licensePlate}
                          </span>
                        </div>
                      )}
                      <div className="grid grid-cols-2 gap-2 mt-3 text-sm">
                        <div>
                          <span className="text-gray-600 dark:text-gray-400">Energie:</span>
                          <span className="ml-2 font-semibold text-gray-900 dark:text-gray-100">
                            {session.energyDelivered ? `${session.energyDelivered.toFixed(1)} kWh` : '0.0 kWh'}
                          </span>
                        </div>
                        <div>
                          <span className="text-gray-600 dark:text-gray-400">Kosten:</span>
                          <span className="ml-2 font-semibold text-gray-900 dark:text-gray-100">
                            €{session.cost ? session.cost.toFixed(2) : '0.00'}
                          </span>
                        </div>
                      </div>
                    </div>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleStopCharging(session.id)}
                      disabled={stopping === session.id}
                    >
                      {stopping === session.id ? (
                        <>
                          <Loader2 className="h-4 w-4 animate-spin mr-2" />
                          Stoppe...
                        </>
                      ) : (
                        'Beenden'
                      )}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* My Vehicles */}
      {myVehicles.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Meine Fahrzeuge</CardTitle>
                <CardDescription>Ihnen zugewiesene Fahrzeuge</CardDescription>
              </div>
              <Button variant="outline" onClick={() => navigate('/user/vehicles')}>
                Alle anzeigen
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {myVehicles.slice(0, 4).map((assignment) => (
                <div 
                  key={assignment.id}
                  className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors cursor-pointer"
                  onClick={() => navigate('/user/vehicles')}
                >
                  <div className="flex items-start space-x-3">
                    <div className="bg-primary/10 p-2 rounded-lg">
                      <Car className="h-5 w-5 text-primary" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="font-semibold text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.make} {assignment.vehicle.model}
                      </div>
                      <div className="text-sm text-gray-600 dark:text-gray-400 font-mono">
                        {assignment.vehicle.licensePlate}
                      </div>
                      <div className="flex gap-2 mt-2">
                        <Badge 
                          variant="outline" 
                          className={
                            assignment.assignmentType === 'Permanent' 
                              ? 'bg-blue-50 dark:bg-blue-950 border-blue-500 text-blue-700 dark:text-blue-300' 
                              : 'bg-green-50 dark:bg-green-950 border-green-500 text-green-700 dark:text-green-300'
                          }
                        >
                          {assignment.assignmentType === 'Permanent' ? 'Dienstwagen' : 'Poolfahrzeug'}
                        </Badge>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
            {myVehicles.length > 4 && (
              <div className="mt-4 text-center">
                <Button 
                  variant="link" 
                  onClick={() => navigate('/user/vehicles')}
                  className="text-primary"
                >
                  {myVehicles.length - 4} weitere Fahrzeuge anzeigen
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Recent Sessions */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Letzte Ladevorgänge</CardTitle>
              <CardDescription>Ihre neuesten Ladeaktivitäten</CardDescription>
            </div>
            <Button variant="outline" onClick={() => navigate('/user/sessions')}>
              Alle anzeigen
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {recentSessions.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Datum</TableHead>
                  <TableHead>Ladestation</TableHead>
                  <TableHead>Fahrzeug</TableHead>
                  <TableHead>Dauer</TableHead>
                  <TableHead>Energie</TableHead>
                  <TableHead>Kosten</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Aktionen</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recentSessions.map((session) => (
                  <TableRow key={session.id}>
                    <TableCell className="text-sm">
                      {new Date(session.startedAt).toLocaleDateString('de-DE', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </TableCell>
                    <TableCell>
                      <div className="text-sm">
                        <div className="font-medium">{session.station.name}</div>
                        <div className="text-gray-500 text-xs">
                          {session.station.chargingPark.name}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell className="text-sm">
                      {session.vehicle ? (
                        <div>
                          <div className="font-medium">
                            {session.vehicle.make} {session.vehicle.model}
                          </div>
                          <div className="text-gray-500 text-xs">
                            {session.vehicle.licensePlate}
                          </div>
                        </div>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </TableCell>
                    <TableCell className="text-sm">
                      {Math.floor(session.duration / 60)}h {session.duration % 60}m
                    </TableCell>
                    <TableCell className="text-sm font-medium">
                      {session.energyDelivered ? `${session.energyDelivered.toFixed(1)} kWh` : '-'}
                    </TableCell>
                    <TableCell className="text-sm font-medium">
                      €{session.cost ? session.cost.toFixed(2) : '0.00'}
                    </TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 text-xs rounded-full ${getStatusBadge(session.status)}`}>
                        {session.status}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Button 
                        variant="ghost" 
                        size="sm"
                        onClick={() => navigate(`/user/sessions/${session.id}`)}
                        className="flex items-center space-x-1"
                      >
                        <Eye className="h-4 w-4" />
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
              <p className="text-gray-600 mb-4">
                Starten Sie Ihren ersten Ladevorgang an einer verfügbaren Ladestation
              </p>
              <Button onClick={() => navigate('/user/stations')}>
                <MapPin className="h-4 w-4 mr-2" />
                Ladestationen finden
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Stop Charging Confirmation Dialog */}
      <ConfirmDialog
        open={stopConfirm.open}
        onOpenChange={(open) => setStopConfirm({ ...stopConfirm, open })}
        title="Ladevorgang beenden"
        message="Möchten Sie den Ladevorgang wirklich beenden? Diese Aktion kann nicht rückgängig gemacht werden."
        confirmText="Beenden"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleStopConfirm}
      />
    </div>
  );
};


