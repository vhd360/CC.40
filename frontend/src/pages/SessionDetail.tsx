import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Loader2, ArrowLeft, Zap, Clock, MapPin, Car, CreditCard, FileText, Receipt } from 'lucide-react';
import { api, SessionCostBreakdown, TariffComponentType } from '../services/api';

export const SessionDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [session, setSession] = useState<SessionCostBreakdown | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadSessionDetails();
    }
  }, [id]);

  const loadSessionDetails = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.getSessionCostBreakdown(id!);
      setSession(data);
    } catch (error: any) {
      console.error('Failed to load session details:', error);
      setError(error.message || 'Fehler beim Laden der Session-Details');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      'Completed': 'bg-green-100 text-green-800',
      'Charging': 'bg-blue-100 text-blue-800',
      'Stopped': 'bg-gray-100 text-gray-800',
      'Faulted': 'bg-red-100 text-red-800',
      'Reserved': 'bg-yellow-100 text-yellow-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  const getComponentTypeLabel = (componentName: string): string => {
    if (componentName.includes('Energy')) return 'Energie';
    if (componentName.includes('ChargingTime')) return 'Ladezeit';
    if (componentName.includes('ParkingTime')) return 'Parkzeit';
    if (componentName.includes('SessionFee')) return 'Sitzungsgebühr';
    if (componentName.includes('IdleTime')) return 'Standzeit';
    if (componentName.includes('TimeOfDay')) return 'Tageszeit';
    if (componentName.includes('default')) return 'Standard-Tarif';
    return componentName;
  };

  const getComponentIcon = (componentName: string) => {
    if (componentName.includes('Energy') || componentName.includes('default')) return <Zap className="h-4 w-4" />;
    if (componentName.includes('Time') || componentName.includes('Standzeit')) return <Clock className="h-4 w-4" />;
    if (componentName.includes('SessionFee') || componentName.includes('Sitzung')) return <CreditCard className="h-4 w-4" />;
    return <Receipt className="h-4 w-4" />;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Session-Details...</span>
      </div>
    );
  }

  if (error || !session) {
    return (
      <div className="py-12">
        <div className="text-center">
          <p className="text-red-600 mb-4">{error || 'Session nicht gefunden'}</p>
          <Button onClick={() => navigate('/user/sessions')} variant="outline">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Zurück zur Übersicht
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button onClick={() => navigate('/user/sessions')} variant="outline" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Zurück
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Ladevorgang Details</h1>
            <p className="text-gray-600 text-sm">Session #{session.sessionNumber}</p>
          </div>
        </div>
        <Badge className={getStatusBadge(session.status)}>
          {session.status}
        </Badge>
      </div>

      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center space-x-2">
              <Zap className="h-5 w-5 text-blue-600" />
              <CardTitle className="text-lg">Energie</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{session.energyDelivered.toFixed(2)}</div>
            <p className="text-gray-600 text-sm">kWh geladen</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center space-x-2">
              <Clock className="h-5 w-5 text-green-600" />
              <CardTitle className="text-lg">Dauer</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{session.durationFormatted}</div>
            <p className="text-gray-600 text-sm">{session.durationMinutes} Minuten</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center space-x-2">
              <CreditCard className="h-5 w-5 text-purple-600" />
              <CardTitle className="text-lg">Gesamtkosten</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{session.totalCost.toFixed(2)} {session.currency}</div>
            <p className="text-gray-600 text-sm">
              {session.appliedTariff ? `Tarif: ${session.appliedTariff.name}` : 'Standard-Tarif'}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Cost Breakdown */}
      <Card>
        <CardHeader>
          <div className="flex items-center space-x-2">
            <Receipt className="h-5 w-5 text-gray-600" />
            <CardTitle>Kostenaufschlüsselung</CardTitle>
          </div>
          <CardDescription>
            Detaillierte Aufschlüsselung der Kosten nach Tarifkomponenten
          </CardDescription>
        </CardHeader>
        <CardContent>
          {session.costBreakdown.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Komponente</TableHead>
                  <TableHead>Details</TableHead>
                  <TableHead className="text-right">Kosten</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {session.costBreakdown.map((item, index) => (
                  <TableRow key={index}>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        {getComponentIcon(item.component)}
                        <span className="font-medium">
                          {getComponentTypeLabel(item.component)}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {item.component.includes('Energy') && `${session.energyDelivered.toFixed(2)} kWh`}
                      {item.component.includes('Time') && `${session.durationMinutes} min`}
                      {item.component.includes('SessionFee') && 'Einmalige Gebühr'}
                      {item.component.includes('default') && `${session.energyDelivered.toFixed(2)} kWh`}
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {item.cost.toFixed(2)} {session.currency}
                    </TableCell>
                  </TableRow>
                ))}
                <TableRow className="bg-gray-50 font-bold">
                  <TableCell colSpan={2}>Gesamtsumme</TableCell>
                  <TableCell className="text-right">
                    {session.totalCost.toFixed(2)} {session.currency}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>Keine Kostenaufschlüsselung verfügbar</p>
              <p className="text-sm mt-2">Gesamtkosten: {session.totalCostFormatted}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Session Details */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Station Information */}
        <Card>
          <CardHeader>
            <div className="flex items-center space-x-2">
              <MapPin className="h-5 w-5 text-gray-600" />
              <CardTitle>Ladestation</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <p className="text-sm text-gray-600">Name</p>
              <p className="font-medium">{session.station.name}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Station ID</p>
              <p className="font-mono text-sm">{session.station.stationId}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Standort</p>
              <p className="font-medium">{session.station.chargingPark.name}</p>
              {session.station.chargingPark.address && (
                <p className="text-sm text-gray-600">{session.station.chargingPark.address}</p>
              )}
              <p className="text-sm text-gray-600">
                {session.station.chargingPark.postalCode && `${session.station.chargingPark.postalCode} `}
                {session.station.chargingPark.city}
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Konnektor</p>
              <p className="font-medium">#{session.connector.connectorId} - {session.connector.type}</p>
            </div>
          </CardContent>
        </Card>

        {/* Vehicle & Time Information */}
        <Card>
          <CardHeader>
            <div className="flex items-center space-x-2">
              <FileText className="h-5 w-5 text-gray-600" />
              <CardTitle>Session-Informationen</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <p className="text-sm text-gray-600">Startzeitpunkt</p>
              <p className="font-medium">
                {new Date(session.startedAt).toLocaleString('de-DE', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                  second: '2-digit'
                })}
              </p>
            </div>
            {session.endedAt && (
              <div>
                <p className="text-sm text-gray-600">Endzeitpunkt</p>
                <p className="font-medium">
                  {new Date(session.endedAt).toLocaleString('de-DE', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit'
                  })}
                </p>
              </div>
            )}
            {session.vehicle && (
              <div>
                <p className="text-sm text-gray-600">Fahrzeug</p>
                <p className="font-medium">
                  {session.vehicle.make} {session.vehicle.model}
                </p>
                <p className="text-sm text-gray-600">{session.vehicle.licensePlate}</p>
              </div>
            )}
            {session.authorizationMethod && (
              <div>
                <p className="text-sm text-gray-600">Autorisierung</p>
                <p className="font-medium">{session.authorizationMethod.friendlyName}</p>
                <p className="text-sm text-gray-600 font-mono">{session.authorizationMethod.identifier}</p>
              </div>
            )}
            {session.ocppTransactionId && (
              <div>
                <p className="text-sm text-gray-600">OCPP Transaction ID</p>
                <p className="font-mono text-sm">{session.ocppTransactionId}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Tariff Information */}
      {session.appliedTariff && (
        <Card>
          <CardHeader>
            <div className="flex items-center space-x-2">
              <Receipt className="h-5 w-5 text-gray-600" />
              <CardTitle>Angewendeter Tarif</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <p className="text-sm text-gray-600">Tarifname</p>
              <p className="font-medium text-lg">{session.appliedTariff.name}</p>
            </div>
            {session.appliedTariff.description && (
              <div>
                <p className="text-sm text-gray-600">Beschreibung</p>
                <p className="text-sm">{session.appliedTariff.description}</p>
              </div>
            )}
            <div>
              <p className="text-sm text-gray-600">Währung</p>
              <p className="font-medium">{session.appliedTariff.currency}</p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

