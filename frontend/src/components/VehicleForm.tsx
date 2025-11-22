import React, { useState } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';

interface VehicleFormProps {
  vehicle?: any;
  onSubmit: (data: VehicleFormData) => void;
  onCancel: () => void;
}

export interface VehicleFormData {
  licensePlate: string;
  make: string;
  model: string;
  year: number;
  type: string;
  color: string;
  notes?: string;
  rfidTag?: string;
  qrCode?: string;
}

export const VehicleForm: React.FC<VehicleFormProps> = ({ vehicle, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState<VehicleFormData>({
    licensePlate: vehicle?.licensePlate || '',
    make: vehicle?.make || '',
    model: vehicle?.model || '',
    year: vehicle?.year || new Date().getFullYear(),
    type: vehicle?.type || 'PoolVehicle',
    color: vehicle?.color || '',
    notes: vehicle?.notes || '',
    rfidTag: vehicle?.rfidTag || '',
    qrCode: vehicle?.qrCode || ''
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader>
        <CardTitle>{vehicle ? 'Fahrzeug bearbeiten' : 'Neues Fahrzeug anlegen'}</CardTitle>
        <CardDescription>
          {vehicle ? 'Aktualisieren Sie die Fahrzeugdaten' : 'Geben Sie die Details für das neue Fahrzeug ein'}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="licensePlate">Kennzeichen *</Label>
              <Input
                id="licensePlate"
                value={formData.licensePlate}
                onChange={(e) => setFormData({ ...formData, licensePlate: e.target.value })}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="type">Fahrzeugtyp *</Label>
              <select
                id="type"
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                className="w-full h-10 rounded-md border border-input bg-background px-3 py-2"
                required
              >
                <option value="PoolVehicle">Poolfahrzeug</option>
                <option value="CompanyVehicle">Dienstwagen</option>
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="make">Hersteller *</Label>
              <Input
                id="make"
                value={formData.make}
                onChange={(e) => setFormData({ ...formData, make: e.target.value })}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="model">Modell *</Label>
              <Input
                id="model"
                value={formData.model}
                onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="year">Baujahr *</Label>
              <Input
                id="year"
                type="number"
                value={formData.year}
                onChange={(e) => setFormData({ ...formData, year: parseInt(e.target.value) })}
                min="1900"
                max={new Date().getFullYear() + 1}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="color">Farbe *</Label>
              <Input
                id="color"
                value={formData.color}
                onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="space-y-4 border-t pt-4 mt-4">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Identifikationsmethoden</h3>
            <p className="text-sm text-gray-600 dark:text-gray-400">
              Fügen Sie RFID-Tags oder QR-Codes hinzu, damit das Fahrzeug automatisch erkannt werden kann
            </p>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="rfidTag">RFID-Tag</Label>
                <Input
                  id="rfidTag"
                  value={formData.rfidTag}
                  onChange={(e) => setFormData({ ...formData, rfidTag: e.target.value })}
                  placeholder="z.B. VEHICLE-001"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  Für automatische Fahrzeugerkennung beim Laden
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="qrCode">QR-Code</Label>
                <Input
                  id="qrCode"
                  value={formData.qrCode}
                  onChange={(e) => setFormData({ ...formData, qrCode: e.target.value })}
                  placeholder="z.B. QR-VEHICLE-001"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  Alternative Identifikationsmethode
                </p>
              </div>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notizen</Label>
            <textarea
              id="notes"
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              className="w-full min-h-[100px] rounded-md border border-input bg-background px-3 py-2 dark:bg-gray-800 dark:text-gray-100"
              placeholder="Zusätzliche Informationen..."
            />
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit">
              {vehicle ? 'Speichern' : 'Fahrzeug anlegen'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

