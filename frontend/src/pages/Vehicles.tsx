import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Car, Plus, Loader2, Edit, Trash2, QrCode } from 'lucide-react';
import { api, Vehicle } from '../services/api';
import { VehicleForm, VehicleFormData } from '../components/VehicleForm';
import { VehicleQRCodeDialog } from '../components/VehicleQRCodeDialog';

export const Vehicles: React.FC = () => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null);
  const [qrCodeVehicle, setQrCodeVehicle] = useState<Vehicle | null>(null);
  const [showQRDialog, setShowQRDialog] = useState(false);

  const loadVehicles = async () => {
    try {
      setLoading(true);
      const data = await api.getVehicles();
      setVehicles(data);
    } catch (error) {
      console.error('Failed to load vehicles:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadVehicles();
  }, []);

  const handleCreate = async (data: VehicleFormData) => {
    try {
      await api.createVehicle(data);
      setShowForm(false);
      loadVehicles();
    } catch (error) {
      console.error('Failed to create vehicle:', error);
      alert('Fehler beim Anlegen des Fahrzeugs');
    }
  };

  const handleUpdate = async (data: VehicleFormData) => {
    if (!editingVehicle) return;
    
    try {
      await api.updateVehicle(editingVehicle.id, { ...data, isActive: editingVehicle.isActive });
      setEditingVehicle(null);
      setShowForm(false);
      loadVehicles();
    } catch (error) {
      console.error('Failed to update vehicle:', error);
      alert('Fehler beim Aktualisieren des Fahrzeugs');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Möchten Sie dieses Fahrzeug wirklich löschen?')) return;
    
    try {
      await api.deleteVehicle(id);
      loadVehicles();
    } catch (error) {
      console.error('Failed to delete vehicle:', error);
      alert('Fehler beim Löschen des Fahrzeugs');
    }
  };

  const handleEdit = (vehicle: Vehicle) => {
    setEditingVehicle(vehicle);
    setShowForm(true);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingVehicle(null);
  };

  const handleShowQRCode = (vehicle: Vehicle) => {
    setQrCodeVehicle(vehicle);
    setShowQRDialog(true);
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={handleCancel}>
          ← Zurück
        </Button>
        <div className="flex justify-center">
          <VehicleForm
            vehicle={editingVehicle}
            onSubmit={editingVehicle ? handleUpdate : handleCreate}
            onCancel={handleCancel}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Fahrzeuge</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Verwalten Sie Poolfahrzeuge und Dienstwagen</p>
        </div>
        <Button onClick={() => setShowForm(true)} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neues Fahrzeug</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Lade Fahrzeuge...</span>
        </div>
      ) : vehicles.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {vehicles.map((vehicle) => (
            <Card key={vehicle.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">{vehicle.make} {vehicle.model}</CardTitle>
                  <div className={`w-3 h-3 rounded-full ${
                    vehicle.isActive ? 'bg-green-500' : 'bg-red-500'
                  }`} />
                </div>
                <CardDescription>{vehicle.licensePlate}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <span className="text-sm text-gray-600 dark:text-gray-400">Baujahr</span>
                    <div className="font-medium text-gray-900 dark:text-gray-100">{vehicle.year}</div>
                  </div>
                  <div>
                    <span className="text-sm text-gray-600 dark:text-gray-400">Farbe</span>
                    <div className="font-medium text-gray-900 dark:text-gray-100">{vehicle.color}</div>
                  </div>
                </div>

                <div>
                  <span className="text-sm text-gray-600 dark:text-gray-400">Typ</span>
                  <div className="font-medium text-gray-900 dark:text-gray-100">
                    {vehicle.type === 'PoolVehicle' ? 'Poolfahrzeug' : 'Dienstwagen'}
                  </div>
                </div>

                {vehicle.notes && (
                  <div>
                    <span className="text-sm text-gray-600 dark:text-gray-400">Notizen</span>
                    <div className="text-sm text-gray-700 dark:text-gray-300 mt-1">{vehicle.notes}</div>
                  </div>
                )}

                <div className="flex flex-col space-y-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="w-full bg-blue-50 hover:bg-blue-100 dark:bg-blue-950 dark:hover:bg-blue-900 border-blue-200 dark:border-blue-800"
                    onClick={() => handleShowQRCode(vehicle)}
                  >
                    <QrCode className="h-4 w-4 mr-2" />
                    QR-Code anzeigen
                  </Button>
                  <div className="flex space-x-2">
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="flex-1"
                      onClick={() => handleEdit(vehicle)}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Bearbeiten
                    </Button>
                    <Button 
                      variant="outline" 
                      size="sm"
                      onClick={() => handleDelete(vehicle.id)}
                      className="text-red-600 hover:text-red-700"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Car className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Fahrzeuge vorhanden</h3>
            <p className="text-gray-600 mb-4">Legen Sie Ihr erstes Fahrzeug an</p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Fahrzeug anlegen
            </Button>
          </CardContent>
        </Card>
      )}

      {/* QR-Code Dialog */}
      <VehicleQRCodeDialog
        open={showQRDialog}
        onOpenChange={setShowQRDialog}
        vehicle={qrCodeVehicle}
      />
    </div>
  );
};
