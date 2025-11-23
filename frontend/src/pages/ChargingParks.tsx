import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { MapPin, Plus, Loader2, Edit, Trash2, Building2 } from 'lucide-react';
import { api } from '../services/api';
import { ChargingParkForm, ChargingParkFormData } from '../components/ChargingParkForm';
import { useToast } from '../components/ui/toast';
import { ConfirmDialog } from '../components/ConfirmDialog';

interface ChargingPark {
  id: string;
  name: string;
  description?: string;
  address: string;
  postalCode: string;
  city: string;
  country: string;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
  createdAt: string;
  stationCount: number;
}

export const ChargingParks: React.FC = () => {
  const [parks, setParks] = useState<ChargingPark[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingPark, setEditingPark] = useState<ChargingPark | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<{ open: boolean; parkId: string | null; parkName?: string }>({
    open: false,
    parkId: null
  });
  const { showToast } = useToast();

  const loadParks = async () => {
    try {
      setLoading(true);
      const data = await api.getChargingParks();
      setParks(data);
    } catch (error) {
      console.error('Failed to load charging parks:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadParks();
  }, []);

  const handleCreate = async (data: ChargingParkFormData) => {
    try {
      await api.createChargingPark(data);
      setShowForm(false);
      loadParks();
      showToast('Ladepark erfolgreich erstellt', 'success');
    } catch (error) {
      console.error('Failed to create charging park:', error);
      showToast('Fehler beim Anlegen des Ladeparks', 'error');
    }
  };

  const handleUpdate = async (data: ChargingParkFormData) => {
    if (!editingPark) return;
    
    try {
      await api.updateChargingPark(editingPark.id, { ...data, isActive: editingPark.isActive });
      setEditingPark(null);
      setShowForm(false);
      loadParks();
      showToast('Ladepark erfolgreich aktualisiert', 'success');
    } catch (error) {
      console.error('Failed to update charging park:', error);
      showToast('Fehler beim Aktualisieren des Ladeparks', 'error');
    }
  };

  const handleDelete = (id: string) => {
    const park = parks.find(p => p.id === id);
    setDeleteConfirm({
      open: true,
      parkId: id,
      parkName: park?.name
    });
  };

  const handleDeleteConfirm = async () => {
    if (!deleteConfirm.parkId) return;
    
    try {
      await api.deleteChargingPark(deleteConfirm.parkId);
      loadParks();
      showToast('Ladepark erfolgreich gelöscht', 'success');
    } catch (error) {
      console.error('Failed to delete charging park:', error);
      showToast('Fehler beim Löschen des Ladeparks', 'error');
    } finally {
      setDeleteConfirm({ open: false, parkId: null });
    }
  };

  const handleEdit = (park: ChargingPark) => {
    setEditingPark(park);
    setShowForm(true);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingPark(null);
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={handleCancel}>
          ← Zurück
        </Button>
        <div className="flex justify-center">
          <ChargingParkForm
            park={editingPark}
            onSubmit={editingPark ? handleUpdate : handleCreate}
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
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Ladeparks</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Verwalten Sie Ihre Ladepark-Standorte</p>
        </div>
        <Button onClick={() => setShowForm(true)} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neuer Ladepark</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Lade Ladeparks...</span>
        </div>
      ) : parks.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {parks.map((park) => (
            <Card key={park.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">{park.name}</CardTitle>
                  <div className={`w-3 h-3 rounded-full ${
                    park.isActive ? 'bg-green-500' : 'bg-red-500'
                  }`} />
                </div>
                <CardDescription className="flex items-center text-sm">
                  <MapPin className="h-3 w-3 mr-1" />
                  {park.city}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <span className="text-sm text-gray-600">Adresse</span>
                  <div className="font-medium text-sm">
                    {park.address}<br />
                    {park.postalCode} {park.city}
                  </div>
                </div>

                {park.description && (
                  <div>
                    <span className="text-sm text-gray-600">Beschreibung</span>
                    <div className="text-sm text-gray-700 mt-1">{park.description}</div>
                  </div>
                )}

                <div>
                  <span className="text-sm text-gray-600">Ladestationen</span>
                  <div className="font-medium">{park.stationCount} Stationen</div>
                </div>

                <div className="flex space-x-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => handleEdit(park)}
                  >
                    <Edit className="h-4 w-4 mr-1" />
                    Bearbeiten
                  </Button>
                  <Button 
                    variant="outline" 
                    size="sm"
                    onClick={() => handleDelete(park.id)}
                    className="text-red-600 hover:text-red-700"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Building2 className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Ladeparks vorhanden</h3>
            <p className="text-gray-600 mb-4">Legen Sie Ihren ersten Ladepark an</p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Ladepark anlegen
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={deleteConfirm.open}
        onOpenChange={(open) => setDeleteConfirm({ ...deleteConfirm, open })}
        title="Ladepark löschen"
        message={`Möchten Sie den Ladepark "${deleteConfirm.parkName}" wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteConfirm}
      />
    </div>
  );
};
