import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Plus, Trash2, Save, Users } from 'lucide-react';
import { api, Tariff, TariffComponentType, CreateTariffRequest, TariffComponentRequest } from '../services/api';

interface UserGroup {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

interface TariffFormProps {
  tariff?: Tariff | null;
  onSuccess: () => void;
  onCancel: () => void;
}

interface ComponentFormData extends TariffComponentRequest {
  tempId: string;
}

export const TariffForm: React.FC<TariffFormProps> = ({ tariff, onSuccess, onCancel }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    currency: 'EUR',
    isDefault: false,
    isActive: true,
    validFrom: '',
    validUntil: ''
  });

  const [components, setComponents] = useState<ComponentFormData[]>([]);
  const [saving, setSaving] = useState(false);
  const [userGroups, setUserGroups] = useState<UserGroup[]>([]);
  const [selectedUserGroup, setSelectedUserGroup] = useState<string>('');
  const [groupPriority, setGroupPriority] = useState<number>(10);
  const [assignedGroups, setAssignedGroups] = useState<Array<{ userGroupId: string; userGroupName: string; priority: number }>>([]);

  useEffect(() => {
    // Load user groups
    const loadUserGroups = async () => {
      try {
        const groups = await api.getUserGroups();
        setUserGroups(groups);
      } catch (error) {
        console.error('Failed to load user groups:', error);
      }
    };
    loadUserGroups();

    if (tariff) {
      setFormData({
        name: tariff.name,
        description: tariff.description || '',
        currency: tariff.currency,
        isDefault: tariff.isDefault,
        isActive: tariff.isActive,
        validFrom: tariff.validFrom ? tariff.validFrom.split('T')[0] : '',
        validUntil: tariff.validUntil ? tariff.validUntil.split('T')[0] : ''
      });

      setComponents(
        tariff.components.map((c) => ({
          tempId: Math.random().toString(),
          type: c.type,
          price: c.price,
          stepSize: c.stepSize,
          timeStart: c.timeStart,
          timeEnd: c.timeEnd,
          daysOfWeek: c.daysOfWeek,
          minimumCharge: c.minimumCharge,
          maximumCharge: c.maximumCharge,
          gracePeriodMinutes: c.gracePeriodMinutes,
          displayOrder: c.displayOrder
        }))
      );

      setAssignedGroups(tariff.userGroups || []);
    } else {
      // Default: Add one energy component
      addComponent();
    }
  }, [tariff]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (components.length === 0) {
      alert('Bitte fügen Sie mindestens eine Tarifkomponente hinzu');
      return;
    }

    try {
      setSaving(true);

      const requestData: CreateTariffRequest = {
        name: formData.name,
        description: formData.description || undefined,
        currency: formData.currency,
        isDefault: formData.isDefault,
        isActive: formData.isActive,
        validFrom: formData.validFrom || undefined,
        validUntil: formData.validUntil || undefined,
        components: components.map((c, index) => ({
          type: c.type,
          price: c.price,
          stepSize: c.stepSize,
          timeStart: c.timeStart,
          timeEnd: c.timeEnd,
          daysOfWeek: c.daysOfWeek,
          minimumCharge: c.minimumCharge,
          maximumCharge: c.maximumCharge,
          gracePeriodMinutes: c.gracePeriodMinutes,
          displayOrder: index
        }))
      };

      let savedTariffId: string;
      
      if (tariff) {
        // UPDATE: Remove old assignments and create new ones
        savedTariffId = tariff.id;
        
        // Remove all existing user group assignments
        const existingGroups = tariff.userGroups || [];
        for (const group of existingGroups) {
          try {
            await api.removeTariffFromUserGroup(savedTariffId, group.userGroupId);
          } catch (error) {
            console.error('Failed to remove user group:', error);
          }
        }
        
        // Update the tariff
        await api.updateTariff(tariff.id, requestData);
      } else {
        // CREATE: Just create the tariff
        const created = await api.createTariff(requestData);
        savedTariffId = created.id;
      }

      // Assign user groups (for both create and update)
      for (const group of assignedGroups) {
        try {
          await api.assignTariffToUserGroup(savedTariffId, group.userGroupId, group.priority);
        } catch (error) {
          console.error('Failed to assign user group:', error);
        }
      }

      onSuccess();
    } catch (error) {
      console.error('Failed to save tariff:', error);
      alert('Fehler beim Speichern des Tarifs');
    } finally {
      setSaving(false);
    }
  };

  const addComponent = () => {
    setComponents([
      ...components,
      {
        tempId: Math.random().toString(),
        type: TariffComponentType.Energy,
        price: 0.30,
        displayOrder: components.length
      }
    ]);
  };

  const removeComponent = (tempId: string) => {
    setComponents(components.filter((c) => c.tempId !== tempId));
  };

  const updateComponent = (tempId: string, updates: Partial<ComponentFormData>) => {
    setComponents(
      components.map((c) => (c.tempId === tempId ? { ...c, ...updates } : c))
    );
  };

  const getComponentTypeLabel = (type: TariffComponentType): string => {
    switch (type) {
      case TariffComponentType.Energy: return 'Energie (€/kWh)';
      case TariffComponentType.ChargingTime: return 'Ladezeit (€/Minute)';
      case TariffComponentType.ParkingTime: return 'Parkzeit (€/Minute)';
      case TariffComponentType.SessionFee: return 'Sitzungsgebühr (€)';
      case TariffComponentType.IdleTime: return 'Standzeit (€/Minute)';
      case TariffComponentType.TimeOfDay: return 'Zeittarif (€/kWh)';
      default: return 'Unbekannt';
    }
  };

  const handleAddUserGroup = () => {
    if (!selectedUserGroup) {
      alert('Bitte wählen Sie eine Benutzergruppe aus');
      return;
    }

    const group = userGroups.find(g => g.id === selectedUserGroup);
    if (!group) return;

    // Check if already assigned
    if (assignedGroups.some(ag => ag.userGroupId === selectedUserGroup)) {
      alert('Diese Benutzergruppe ist bereits zugewiesen');
      return;
    }

    setAssignedGroups([
      ...assignedGroups,
      {
        userGroupId: group.id,
        userGroupName: group.name,
        priority: groupPriority
      }
    ]);

    setSelectedUserGroup('');
    setGroupPriority(10);
  };

  const handleRemoveUserGroup = (userGroupId: string) => {
    setAssignedGroups(assignedGroups.filter(ag => ag.userGroupId !== userGroupId));
  };

  return (
    <Card className="w-full max-w-4xl mx-auto">
      <CardHeader>
        <CardTitle>{tariff ? 'Tarif bearbeiten' : 'Neuer Tarif'}</CardTitle>
        <CardDescription>
          {tariff ? 'Aktualisieren Sie die Tarifdaten' : 'Erstellen Sie einen neuen Tarif'}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Basic Information */}
          <div className="space-y-4">
            <h3 className="font-semibold">Grundinformationen</h3>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="currency">Währung *</Label>
                <Input
                  id="currency"
                  value={formData.currency}
                  onChange={(e) => setFormData({ ...formData, currency: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Beschreibung</Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="validFrom">Gültig ab</Label>
                <Input
                  id="validFrom"
                  type="date"
                  value={formData.validFrom}
                  onChange={(e) => setFormData({ ...formData, validFrom: e.target.value })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="validUntil">Gültig bis</Label>
                <Input
                  id="validUntil"
                  type="date"
                  value={formData.validUntil}
                  onChange={(e) => setFormData({ ...formData, validUntil: e.target.value })}
                />
              </div>
            </div>

            <div className="flex gap-4">
              <div className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  id="isDefault"
                  checked={formData.isDefault}
                  onChange={(e) => setFormData({ ...formData, isDefault: e.target.checked })}
                  className="w-4 h-4"
                />
                <Label htmlFor="isDefault">Standard-Tarif</Label>
              </div>

              <div className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  className="w-4 h-4"
                />
                <Label htmlFor="isActive">Aktiv</Label>
              </div>
            </div>
          </div>

          {/* User Groups Assignment */}
          <div className="space-y-4">
            <h3 className="font-semibold flex items-center gap-2">
              <Users className="h-5 w-5" />
              Benutzergruppen zuweisen
            </h3>
            
            <div className="flex gap-2">
              <div className="flex-1">
                <select
                  value={selectedUserGroup}
                  onChange={(e) => setSelectedUserGroup(e.target.value)}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                >
                  <option value="">Benutzergruppe auswählen...</option>
                  {userGroups.map((group) => (
                    <option key={group.id} value={group.id}>
                      {group.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="w-32">
                <Input
                  type="number"
                  value={groupPriority}
                  onChange={(e) => setGroupPriority(parseInt(e.target.value))}
                  placeholder="Priorität"
                  min="0"
                />
              </div>
              <Button type="button" onClick={handleAddUserGroup} variant="outline">
                <Plus className="h-4 w-4" />
              </Button>
            </div>

            {assignedGroups.length > 0 && (
              <div className="space-y-2">
                <div className="text-sm text-muted-foreground">
                  Zugewiesene Benutzergruppen:
                </div>
                {assignedGroups.map((group) => (
                  <div
                    key={group.userGroupId}
                    className="flex items-center justify-between bg-secondary/20 rounded px-3 py-2"
                  >
                    <div className="flex items-center gap-2">
                      <Badge variant="secondary">{group.userGroupName}</Badge>
                      <span className="text-sm text-muted-foreground">
                        Priorität: {group.priority}
                      </span>
                    </div>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      onClick={() => handleRemoveUserGroup(group.userGroupId)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Components */}
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <h3 className="font-semibold">Tarifkomponenten</h3>
              <Button type="button" variant="outline" size="sm" onClick={addComponent}>
                <Plus className="mr-2 h-4 w-4" />
                Komponente hinzufügen
              </Button>
            </div>

            <div className="space-y-4">
              {components.map((component, index) => (
                <Card key={component.tempId} className="bg-secondary/20">
                  <CardContent className="pt-6">
                    <div className="space-y-4">
                      <div className="flex justify-between items-start">
                        <h4 className="font-medium">Komponente {index + 1}</h4>
                        {components.length > 1 && (
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            onClick={() => removeComponent(component.tempId)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label>Typ *</Label>
                          <select
                            value={component.type}
                            onChange={(e) =>
                              updateComponent(component.tempId, {
                                type: parseInt(e.target.value) as TariffComponentType
                              })
                            }
                            className="w-full rounded-md border border-input bg-background px-3 py-2"
                            required
                          >
                            {Object.entries(TariffComponentType)
                              .filter(([key]) => !isNaN(Number(key)))
                              .map(([key, value]) => (
                                <option key={key} value={key}>
                                  {getComponentTypeLabel(parseInt(key))}
                                </option>
                              ))}
                          </select>
                        </div>

                        <div className="space-y-2">
                          <Label>Preis * ({formData.currency})</Label>
                          <Input
                            type="number"
                            step="0.0001"
                            value={component.price}
                            onChange={(e) =>
                              updateComponent(component.tempId, {
                                price: parseFloat(e.target.value)
                              })
                            }
                            required
                          />
                        </div>
                      </div>

                      {/* Advanced options */}
                      <details className="text-sm">
                        <summary className="cursor-pointer text-muted-foreground hover:text-foreground">
                          Erweiterte Optionen
                        </summary>
                        <div className="grid grid-cols-2 gap-4 mt-4">
                          <div className="space-y-2">
                            <Label>Schrittgröße</Label>
                            <Input
                              type="number"
                              value={component.stepSize || ''}
                              onChange={(e) =>
                                updateComponent(component.tempId, {
                                  stepSize: e.target.value ? parseInt(e.target.value) : undefined
                                })
                              }
                              placeholder="Optional"
                            />
                          </div>

                          <div className="space-y-2">
                            <Label>Kulanzzeit (Minuten)</Label>
                            <Input
                              type="number"
                              value={component.gracePeriodMinutes || ''}
                              onChange={(e) =>
                                updateComponent(component.tempId, {
                                  gracePeriodMinutes: e.target.value ? parseInt(e.target.value) : undefined
                                })
                              }
                              placeholder="Optional"
                            />
                          </div>

                          <div className="space-y-2">
                            <Label>Mindestbetrag ({formData.currency})</Label>
                            <Input
                              type="number"
                              step="0.01"
                              value={component.minimumCharge || ''}
                              onChange={(e) =>
                                updateComponent(component.tempId, {
                                  minimumCharge: e.target.value ? parseFloat(e.target.value) : undefined
                                })
                              }
                              placeholder="Optional"
                            />
                          </div>

                          <div className="space-y-2">
                            <Label>Höchstbetrag ({formData.currency})</Label>
                            <Input
                              type="number"
                              step="0.01"
                              value={component.maximumCharge || ''}
                              onChange={(e) =>
                                updateComponent(component.tempId, {
                                  maximumCharge: e.target.value ? parseFloat(e.target.value) : undefined
                                })
                              }
                              placeholder="Optional"
                            />
                          </div>

                          {component.type === TariffComponentType.TimeOfDay && (
                            <>
                              <div className="space-y-2">
                                <Label>Startzeit (HH:mm)</Label>
                                <Input
                                  type="time"
                                  value={component.timeStart || ''}
                                  onChange={(e) =>
                                    updateComponent(component.tempId, {
                                      timeStart: e.target.value
                                    })
                                  }
                                />
                              </div>

                              <div className="space-y-2">
                                <Label>Endzeit (HH:mm)</Label>
                                <Input
                                  type="time"
                                  value={component.timeEnd || ''}
                                  onChange={(e) =>
                                    updateComponent(component.tempId, {
                                      timeEnd: e.target.value
                                    })
                                  }
                                />
                              </div>

                              <div className="space-y-2 col-span-2">
                                <Label>Wochentage (0=Sonntag, kommagetrennt)</Label>
                                <Input
                                  value={component.daysOfWeek || ''}
                                  onChange={(e) =>
                                    updateComponent(component.tempId, {
                                      daysOfWeek: e.target.value
                                    })
                                  }
                                  placeholder="z.B. 1,2,3,4,5 für Werktage"
                                />
                              </div>
                            </>
                          )}
                        </div>
                      </details>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-2 pt-4 border-t">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? (
                <>
                  <span className="animate-spin mr-2">⏳</span>
                  Speichern...
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Speichern
                </>
              )}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

