import React, { useState, useEffect } from 'react';
import { api, VehicleAssignment, CreateVehicleAssignmentRequest, Vehicle } from '../services/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import { Badge } from '../components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '../components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Textarea } from '../components/ui/textarea';
import { Plus, ArrowLeftRight, Trash2, CheckCircle, AlertCircle } from 'lucide-react';
import { Alert, AlertDescription } from '../components/ui/alert';

export function VehicleAssignments() {
  const [assignments, setAssignments] = useState<VehicleAssignment[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [includeReturned, setIncludeReturned] = useState(false);
  const [filterType, setFilterType] = useState<string>('all');

  const [newAssignment, setNewAssignment] = useState<CreateVehicleAssignmentRequest>({
    vehicleId: '',
    userId: '',
    assignmentType: 'Permanent',
    notes: ''
  });

  useEffect(() => {
    loadData();
  }, [includeReturned]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [assignmentsData, vehiclesData, usersData] = await Promise.all([
        api.getVehicleAssignments(includeReturned),
        api.getVehicles(),
        api.getUsers()
      ]);
      setAssignments(assignmentsData);
      setVehicles(vehiclesData.filter(v => v.isActive));
      setUsers(usersData);
    } catch (err) {
      setError('Fehler beim Laden der Daten');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateAssignment = async () => {
    try {
      setError(null);
      await api.createVehicleAssignment(newAssignment);
      setSuccess('Fahrzeug erfolgreich zugewiesen');
      setShowCreateDialog(false);
      setNewAssignment({
        vehicleId: '',
        userId: '',
        assignmentType: 'Permanent',
        notes: ''
      });
      loadData();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Zuweisen des Fahrzeugs');
    }
  };

  const handleReturnVehicle = async (assignmentId: string) => {
    if (!window.confirm('Möchten Sie dieses Fahrzeug wirklich zurückgeben?')) return;

    try {
      setError(null);
      await api.returnVehicle(assignmentId);
      setSuccess('Fahrzeug erfolgreich zurückgegeben');
      loadData();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Zurückgeben des Fahrzeugs');
    }
  };

  const handleDeleteAssignment = async (assignmentId: string) => {
    if (!window.confirm('Möchten Sie diese Zuweisung wirklich löschen?')) return;

    try {
      setError(null);
      await api.deleteVehicleAssignment(assignmentId);
      setSuccess('Zuweisung erfolgreich gelöscht');
      loadData();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Löschen der Zuweisung');
    }
  };

  const getAssignmentTypeBadge = (type: string) => {
    const variants: Record<string, { color: string; label: string }> = {
      Permanent: { color: 'bg-blue-500 dark:bg-blue-600', label: 'Dienstwagen' },
      Temporary: { color: 'bg-green-500 dark:bg-green-600', label: 'Poolfahrzeug' },
      Reservation: { color: 'bg-yellow-500 dark:bg-yellow-600', label: 'Reservierung' }
    };
    const config = variants[type] || { color: 'bg-gray-500', label: type };
    return <Badge className={config.color}>{config.label}</Badge>;
  };

  const getVehicleTypeBadge = (type: string) => {
    const variants: Record<string, { color: string; label: string }> = {
      PoolVehicle: { color: 'bg-purple-500 dark:bg-purple-600', label: 'Pool' },
      CompanyVehicle: { color: 'bg-indigo-500 dark:bg-indigo-600', label: 'Firmen' }
    };
    const config = variants[type] || { color: 'bg-gray-500', label: type };
    return <Badge variant="outline" className={config.color}>{config.label}</Badge>;
  };

  const filteredAssignments = assignments.filter(assignment => {
    if (filterType === 'all') return true;
    return assignment.assignmentType === filterType;
  });

  // Get available vehicles (not currently assigned)
  const availableVehicles = vehicles.filter(vehicle => {
    return !assignments.some(a => a.vehicleId === vehicle.id && a.isActive);
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-600 dark:text-gray-400">Lädt...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Fahrzeugzuweisungen</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Verwalten Sie Dienstwagen und Poolfahrzeuge
          </p>
        </div>
        <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="w-4 h-4 mr-2" />
              Fahrzeug zuweisen
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[500px]">
            <DialogHeader>
              <DialogTitle>Fahrzeug zuweisen</DialogTitle>
              <DialogDescription>
                Weisen Sie einem Benutzer ein Fahrzeug zu
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="vehicle">Fahrzeug</Label>
                <Select
                  value={newAssignment.vehicleId}
                  onValueChange={(value: string) => setNewAssignment({ ...newAssignment, vehicleId: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Fahrzeug auswählen" />
                  </SelectTrigger>
                  <SelectContent>
                    {availableVehicles.length === 0 ? (
                      <SelectItem value="none" disabled>Keine verfügbaren Fahrzeuge</SelectItem>
                    ) : (
                      availableVehicles.map((vehicle) => (
                        <SelectItem key={vehicle.id} value={vehicle.id}>
                          {vehicle.licensePlate} - {vehicle.make} {vehicle.model} ({vehicle.type})
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="user">Benutzer</Label>
                <Select
                  value={newAssignment.userId}
                  onValueChange={(value: string) => setNewAssignment({ ...newAssignment, userId: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Benutzer auswählen" />
                  </SelectTrigger>
                  <SelectContent>
                    {users.map((user) => (
                      <SelectItem key={user.id} value={user.id}>
                        {user.firstName} {user.lastName} ({user.email})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="assignmentType">Zuweisungstyp</Label>
                <Select
                  value={newAssignment.assignmentType}
                  onValueChange={(value: string) => setNewAssignment({ ...newAssignment, assignmentType: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Permanent">Permanent (Dienstwagen)</SelectItem>
                    <SelectItem value="Temporary">Temporär (Poolfahrzeug)</SelectItem>
                    <SelectItem value="Reservation">Reservierung</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="notes">Notizen (optional)</Label>
                <Textarea
                  id="notes"
                  placeholder="Zusätzliche Informationen..."
                  value={newAssignment.notes || ''}
                  onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNewAssignment({ ...newAssignment, notes: e.target.value })}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setShowCreateDialog(false)}>
                Abbrechen
              </Button>
              <Button
                onClick={handleCreateAssignment}
                disabled={!newAssignment.vehicleId || !newAssignment.userId}
              >
                Zuweisen
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {error && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {success && (
        <Alert className="border-green-500 bg-green-50 dark:bg-green-950">
          <CheckCircle className="h-4 w-4 text-green-600 dark:text-green-400" />
          <AlertDescription className="text-green-600 dark:text-green-400">{success}</AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle>Aktive Zuweisungen</CardTitle>
              <CardDescription>
                {filteredAssignments.filter(a => a.isActive).length} aktive Zuweisungen
              </CardDescription>
            </div>
            <div className="flex gap-2">
              <Select value={filterType} onValueChange={setFilterType}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Alle Typen</SelectItem>
                  <SelectItem value="Permanent">Dienstwagen</SelectItem>
                  <SelectItem value="Temporary">Poolfahrzeuge</SelectItem>
                  <SelectItem value="Reservation">Reservierungen</SelectItem>
                </SelectContent>
              </Select>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setIncludeReturned(!includeReturned)}
              >
                {includeReturned ? 'Nur aktive' : 'Inkl. zurückgegeben'}
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fahrzeug</TableHead>
                <TableHead>Kennzeichen</TableHead>
                <TableHead>Typ</TableHead>
                <TableHead>Benutzer</TableHead>
                <TableHead>Zugewiesen am</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Aktionen</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredAssignments.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center text-gray-500 dark:text-gray-400">
                    Keine Zuweisungen gefunden
                  </TableCell>
                </TableRow>
              ) : (
                filteredAssignments.map((assignment) => (
                  <TableRow key={assignment.id}>
                    <TableCell>
                      <div className="font-medium text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.make} {assignment.vehicle.model}
                      </div>
                      <div className="text-sm text-gray-500 dark:text-gray-400">
                        {assignment.vehicle.year}
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="font-mono font-semibold text-gray-900 dark:text-gray-100">
                        {assignment.vehicle.licensePlate}
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        {getVehicleTypeBadge(assignment.vehicle.type)}
                        {getAssignmentTypeBadge(assignment.assignmentType)}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="font-medium text-gray-900 dark:text-gray-100">
                        {assignment.user.firstName} {assignment.user.lastName}
                      </div>
                      <div className="text-sm text-gray-500 dark:text-gray-400">
                        {assignment.user.email}
                      </div>
                    </TableCell>
                    <TableCell>
                      {new Date(assignment.assignedAt).toLocaleDateString('de-DE')}
                    </TableCell>
                    <TableCell>
                      {assignment.isActive ? (
                        <Badge variant="outline" className="bg-green-50 dark:bg-green-950 border-green-500">
                          Aktiv
                        </Badge>
                      ) : (
                        <Badge variant="outline" className="bg-gray-50 dark:bg-gray-900 border-gray-500">
                          Zurückgegeben
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        {assignment.isActive && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleReturnVehicle(assignment.id)}
                          >
                            <ArrowLeftRight className="w-4 h-4 mr-1" />
                            Zurückgeben
                          </Button>
                        )}
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteAssignment(assignment.id)}
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}

