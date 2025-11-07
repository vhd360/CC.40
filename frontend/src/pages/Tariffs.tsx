import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
import { DollarSign, Plus, Loader2, Edit, Trash2, Users } from 'lucide-react';
import { api, Tariff, TariffComponentType } from '../services/api';
import { TariffForm } from '../components/TariffForm';

export const Tariffs: React.FC = () => {
  const [tariffs, setTariffs] = useState<Tariff[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingTariff, setEditingTariff] = useState<Tariff | null>(null);

  const loadTariffs = async () => {
    try {
      setLoading(true);
      const data = await api.getTariffs();
      setTariffs(data);
    } catch (error) {
      console.error('Failed to load tariffs:', error);
      alert('Fehler beim Laden der Tarife');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTariffs();
  }, []);

  const handleDelete = async (id: string) => {
    if (!window.confirm('Möchten Sie diesen Tarif wirklich löschen?')) return;
    try {
      await api.deleteTariff(id);
      loadTariffs();
    } catch (error) {
      console.error('Failed to delete tariff:', error);
      alert('Fehler beim Löschen des Tarifs');
    }
  };

  const handleEdit = (tariff: Tariff) => {
    setEditingTariff(tariff);
    setShowForm(true);
  };

  const handleFormSuccess = () => {
    setShowForm(false);
    setEditingTariff(null);
    loadTariffs();
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingTariff(null);
  };

  const getComponentTypeLabel = (type: TariffComponentType): string => {
    switch (type) {
      case TariffComponentType.Energy: return 'Energie';
      case TariffComponentType.ChargingTime: return 'Ladezeit';
      case TariffComponentType.ParkingTime: return 'Parkzeit';
      case TariffComponentType.SessionFee: return 'Sitzungsgebühr';
      case TariffComponentType.IdleTime: return 'Standzeit';
      case TariffComponentType.TimeOfDay: return 'Zeittarif';
      default: return 'Unbekannt';
    }
  };

  if (showForm) {
    return (
      <div className="space-y-6">
        <Button variant="outline" onClick={handleFormCancel}>
          ← Zurück
        </Button>
        <TariffForm
          tariff={editingTariff}
          onSuccess={handleFormSuccess}
          onCancel={handleFormCancel}
        />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Tarife</h1>
          <p className="text-muted-foreground">
            Verwalten Sie Ihre Preismodelle und Tarife
          </p>
        </div>
        <Button onClick={() => setShowForm(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Neuer Tarif
        </Button>
      </div>

      {loading ? (
        <div className="flex justify-center items-center py-8">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : tariffs.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <DollarSign className="h-16 w-16 text-muted-foreground mb-4" />
            <h3 className="text-xl font-semibold mb-2">Keine Tarife vorhanden</h3>
            <p className="text-muted-foreground mb-4">
              Erstellen Sie Ihren ersten Tarif, um mit der Preisgestaltung zu beginnen.
            </p>
            <Button onClick={() => setShowForm(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Ersten Tarif erstellen
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {tariffs.map((tariff) => (
            <Card key={tariff.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="flex items-center gap-2">
                      {tariff.name}
                      {tariff.isDefault && (
                        <Badge variant="default">Standard</Badge>
                      )}
                    </CardTitle>
                    <CardDescription className="mt-1">
                      {tariff.description || 'Keine Beschreibung'}
                    </CardDescription>
                  </div>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleEdit(tariff)}
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleDelete(tariff.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Status */}
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Status</span>
                  <Badge variant={tariff.isActive ? 'default' : 'secondary'}>
                    {tariff.isActive ? 'Aktiv' : 'Inaktiv'}
                  </Badge>
                </div>

                {/* Currency */}
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Währung</span>
                  <span className="font-medium">{tariff.currency}</span>
                </div>

                {/* User Groups */}
                {tariff.userGroups.length > 0 && (
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground flex items-center gap-1">
                      <Users className="h-3 w-3" />
                      Benutzergruppen
                    </span>
                    <span className="font-medium">{tariff.userGroups.length}</span>
                  </div>
                )}

                {/* Components */}
                <div className="space-y-2">
                  <div className="text-sm font-medium">Komponenten</div>
                  <div className="space-y-1">
                    {tariff.components.map((component) => (
                      <div
                        key={component.id}
                        className="flex items-center justify-between text-sm bg-secondary/50 rounded px-2 py-1"
                      >
                        <span className="text-muted-foreground">
                          {getComponentTypeLabel(component.type)}
                        </span>
                        <span className="font-mono">
                          {component.price.toFixed(4)} {tariff.currency}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Validity */}
                {(tariff.validFrom || tariff.validUntil) && (
                  <div className="text-xs text-muted-foreground pt-2 border-t">
                    {tariff.validFrom && (
                      <div>Gültig ab: {new Date(tariff.validFrom).toLocaleDateString('de-DE')}</div>
                    )}
                    {tariff.validUntil && (
                      <div>Gültig bis: {new Date(tariff.validUntil).toLocaleDateString('de-DE')}</div>
                    )}
                  </div>
                )}

                {/* User Group List */}
                {tariff.userGroups.length > 0 && (
                  <div className="text-xs text-muted-foreground pt-2 border-t">
                    <div className="font-medium mb-1">Zugeordnete Gruppen:</div>
                    {tariff.userGroups.map((ug) => (
                      <div key={ug.userGroupId} className="flex justify-between">
                        <span>{ug.userGroupName}</span>
                        <span className="text-xs">Priorität: {ug.priority}</span>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
};

