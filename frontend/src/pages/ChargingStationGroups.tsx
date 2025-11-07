import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Zap, Plus, Loader2, Edit, Trash2, Eye } from 'lucide-react';
import { api } from '../services/api';

interface ChargingStationGroup {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  stationCount: number;
}

export const ChargingStationGroups: React.FC = () => {
  const navigate = useNavigate();
  const [groups, setGroups] = useState<ChargingStationGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingGroup, setEditingGroup] = useState<ChargingStationGroup | null>(null);
  const [formData, setFormData] = useState({ name: '', description: '' });

  const loadGroups = async () => {
    try {
      setLoading(true);
      const data = await api.getChargingStationGroups();
      setGroups(data);
    } catch (error) {
      console.error('Failed to load charging station groups:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadGroups();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingGroup) {
        await api.updateChargingStationGroup(editingGroup.id, { ...formData, isActive: editingGroup.isActive });
      } else {
        await api.createChargingStationGroup(formData);
      }
      setShowForm(false);
      setEditingGroup(null);
      setFormData({ name: '', description: '' });
      loadGroups();
    } catch (error) {
      console.error('Failed to save charging station group:', error);
      alert('Fehler beim Speichern der Ladepunkt-Gruppe');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Möchten Sie diese Ladepunkt-Gruppe wirklich löschen?')) return;
    try {
      await api.deleteChargingStationGroup(id);
      loadGroups();
    } catch (error) {
      console.error('Failed to delete charging station group:', error);
      alert('Fehler beim Löschen der Ladepunkt-Gruppe');
    }
  };

  const handleEdit = (group: ChargingStationGroup) => {
    setEditingGroup(group);
    setFormData({ name: group.name, description: group.description || '' });
    setShowForm(true);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingGroup(null);
    setFormData({ name: '', description: '' });
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={handleCancel}>
          ← Zurück
        </Button>
        <div className="flex justify-center">
          <Card className="w-full max-w-2xl">
            <CardHeader>
              <CardTitle>{editingGroup ? 'Ladepunkt-Gruppe bearbeiten' : 'Neue Ladepunkt-Gruppe'}</CardTitle>
              <CardDescription>
                {editingGroup ? 'Aktualisieren Sie die Gruppendaten' : 'Erstellen Sie eine neue Ladepunkt-Gruppe'}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Name *</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    required
                    placeholder="z.B. Schnelllader Gruppe A"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Beschreibung</Label>
                  <textarea
                    id="description"
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
                    placeholder="Beschreibung der Gruppe..."
                  />
                </div>

                <div className="flex justify-end space-x-2 pt-4">
                  <Button type="button" variant="outline" onClick={handleCancel}>
                    Abbrechen
                  </Button>
                  <Button type="submit">
                    {editingGroup ? 'Speichern' : 'Erstellen'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Ladepunkt-Gruppen</h1>
          <p className="text-gray-600 mt-1">Organisieren Sie Ladestationen in Gruppen</p>
        </div>
        <Button onClick={() => setShowForm(true)} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neue Gruppe</span>
        </Button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Lade Ladepunkt-Gruppen...</span>
        </div>
      ) : groups.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {groups.map((group) => (
            <Card key={group.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg flex items-center">
                    <Zap className="h-5 w-5 mr-2 text-yellow-600" />
                    {group.name}
                  </CardTitle>
                  <div className={`w-3 h-3 rounded-full ${
                    group.isActive ? 'bg-green-500' : 'bg-red-500'
                  }`} />
                </div>
                {group.description && (
                  <CardDescription className="text-sm">{group.description}</CardDescription>
                )}
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <span className="text-sm text-gray-600">Ladestationen</span>
                  <div className="text-2xl font-bold text-blue-600">{group.stationCount}</div>
                </div>

                <div className="text-xs text-gray-500">
                  Erstellt: {new Date(group.createdAt).toLocaleDateString('de-DE')}
                </div>

                <div className="flex flex-col space-y-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    onClick={() => navigate(`/charging-station-groups/${group.id}`)}
                  >
                    <Eye className="h-4 w-4 mr-1" />
                    Details & Stationen
                  </Button>
                  <div className="flex space-x-2">
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="flex-1"
                      onClick={() => handleEdit(group)}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Bearbeiten
                    </Button>
                    <Button 
                      variant="outline" 
                      size="sm"
                      onClick={() => handleDelete(group.id)}
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
            <Zap className="h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Keine Ladepunkt-Gruppen vorhanden</h3>
            <p className="text-gray-600 mb-4">Erstellen Sie Ihre erste Ladepunkt-Gruppe</p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Gruppe erstellen
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

