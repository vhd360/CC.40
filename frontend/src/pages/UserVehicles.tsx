import React, { useState, useEffect } from 'react';
import { api, VehicleAssignment } from '../services/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Car, Calendar, FileText } from 'lucide-react';
import { Alert, AlertDescription } from '../components/ui/alert';

export function UserVehicles() {
  const [assignments, setAssignments] = useState<VehicleAssignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadMyVehicles();
  }, []);

  const loadMyVehicles = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.getMyVehicles();
      setAssignments(data);
    } catch (err) {
      setError('Fehler beim Laden Ihrer Fahrzeuge');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getAssignmentTypeInfo = (type: string) => {
    const variants: Record<string, { color: string; label: string; description: string }> = {
      Permanent: {
        color: 'bg-blue-500 dark:bg-blue-600',
        label: 'Dienstwagen',
        description: 'Ihnen dauerhaft zugewiesenes Fahrzeug'
      },
      Temporary: {
        color: 'bg-green-500 dark:bg-green-600',
        label: 'Poolfahrzeug',
        description: 'Temporär zugewiesenes Poolfahrzeug'
      },
      Reservation: {
        color: 'bg-yellow-500 dark:bg-yellow-600',
        label: 'Reservierung',
        description: 'Für Sie reserviertes Fahrzeug'
      }
    };
    return variants[type] || { color: 'bg-gray-500', label: type, description: '' };
  };

  const getVehicleTypeBadge = (type: string) => {
    const variants: Record<string, { color: string; label: string }> = {
      PoolVehicle: { color: 'bg-purple-500 dark:bg-purple-600', label: 'Pool' },
      CompanyVehicle: { color: 'bg-indigo-500 dark:bg-indigo-600', label: 'Firmenfahrzeug' }
    };
    const config = variants[type] || { color: 'bg-gray-500', label: type };
    return <Badge variant="outline" className={config.color}>{config.label}</Badge>;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-600 dark:text-gray-400">Lädt...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Meine Fahrzeuge</h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          Übersicht Ihrer zugewiesenen Fahrzeuge
        </p>
      </div>

      {error && (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {assignments.length === 0 ? (
        <Card>
          <CardContent className="py-12">
            <div className="text-center">
              <Car className="w-16 h-16 mx-auto text-gray-400 dark:text-gray-600 mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">
                Keine Fahrzeuge zugewiesen
              </h3>
              <p className="text-gray-600 dark:text-gray-400">
                Ihnen sind derzeit keine Fahrzeuge zugewiesen.
              </p>
            </div>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {assignments.map((assignment) => {
            const typeInfo = getAssignmentTypeInfo(assignment.assignmentType);
            return (
              <Card key={assignment.id} className="hover:shadow-lg transition-shadow">
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <CardTitle className="text-2xl text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.make} {assignment.vehicle.model}
                      </CardTitle>
                      <CardDescription className="text-base mt-1">
                        Kennzeichen:{' '}
                        <span className="font-mono font-bold text-gray-900 dark:text-gray-100">
                          {assignment.vehicle.licensePlate}
                        </span>
                      </CardDescription>
                    </div>
                    <Car className="w-12 h-12 text-primary" />
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Badges */}
                  <div className="flex gap-2 flex-wrap">
                    <Badge className={typeInfo.color}>{typeInfo.label}</Badge>
                    {getVehicleTypeBadge(assignment.vehicle.type)}
                    {assignment.isActive && (
                      <Badge variant="outline" className="bg-green-50 dark:bg-green-950 border-green-500">
                        Aktiv
                      </Badge>
                    )}
                  </div>

                  {/* Vehicle Details */}
                  <div className="grid grid-cols-2 gap-4 pt-2">
                    <div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">Baujahr</div>
                      <div className="font-semibold text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.year || 'N/A'}
                      </div>
                    </div>
                    <div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">Farbe</div>
                      <div className="font-semibold text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.color || 'N/A'}
                      </div>
                    </div>
                  </div>

                  {/* Assignment Info */}
                  <div className="border-t border-gray-200 dark:border-gray-700 pt-4 space-y-3">
                    <div className="flex items-start gap-3">
                      <Calendar className="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" />
                      <div className="flex-1">
                        <div className="text-sm text-gray-600 dark:text-gray-400">
                          Zugewiesen am
                        </div>
                        <div className="font-medium text-gray-900 dark:text-gray-100">
                          {new Date(assignment.assignedAt).toLocaleDateString('de-DE', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric'
                          })}
                        </div>
                      </div>
                    </div>

                    {assignment.notes && (
                      <div className="flex items-start gap-3">
                        <FileText className="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" />
                        <div className="flex-1">
                          <div className="text-sm text-gray-600 dark:text-gray-400">Notizen</div>
                          <div className="text-sm text-gray-900 dark:text-gray-100 whitespace-pre-wrap">
                            {assignment.notes}
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Type Description */}
                    <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-3">
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        <span className="font-semibold">{typeInfo.label}:</span>{' '}
                        {typeInfo.description}
                      </p>
                    </div>
                  </div>

                  {/* Charging Hint */}
                  <div className="bg-primary/10 border border-primary/20 rounded-lg p-3">
                    <p className="text-sm text-primary">
                      💡 Sie können dieses Fahrzeug an allen für Sie freigegebenen Ladestationen laden.
                    </p>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Info Box */}
      <Card className="bg-blue-50 dark:bg-blue-950 border-blue-200 dark:border-blue-800">
        <CardHeader>
          <CardTitle className="text-blue-900 dark:text-blue-100">
            Hinweise zur Fahrzeugnutzung
          </CardTitle>
        </CardHeader>
        <CardContent className="text-blue-800 dark:text-blue-200 space-y-2 text-sm">
          <p>
            <strong>Dienstwagen:</strong> Dauerhaft zugewiesene Fahrzeuge für Ihre berufliche Nutzung.
            Sie können diese Fahrzeuge auch zuhause laden.
          </p>
          <p>
            <strong>Poolfahrzeuge:</strong> Temporär zugewiesene Fahrzeuge aus dem Firmenpool.
            Bitte geben Sie diese nach Nutzung zurück.
          </p>
          <p>
            <strong>Laden:</strong> Wählen Sie beim Ladevorgang einfach Ihr zugewiesenes Fahrzeug aus,
            damit die Kosten korrekt zugeordnet werden können.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

