import React, { useState, useEffect } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';

interface ChargingStationFormProps {
  station?: any;
  onSubmit: (data: ChargingStationFormData) => void;
  onCancel: () => void;
}

export interface ChargingStationFormData {
  chargingParkId: string;
  stationId: string;
  name: string;
  vendor?: string;
  model?: string;
  maxPower: number;
  numberOfConnectors: number;
  type: string;
  latitude?: number;
  longitude?: number;
  notes?: string;
}

export const ChargingStationForm: React.FC<ChargingStationFormProps> = ({ station, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState<ChargingStationFormData>({
    chargingParkId: station?.chargingParkId || '',
    stationId: station?.stationId || '',
    name: station?.name || '',
    vendor: station?.vendor || '',
    model: station?.model || '',
    maxPower: station?.maxPower || 22,
    numberOfConnectors: station?.numberOfConnectors || 1,
    type: station?.type || 'AC',
    latitude: station?.latitude || undefined,
    longitude: station?.longitude || undefined,
    notes: station?.notes || ''
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <Card className="w-full max-w-3xl">
      <CardHeader>
        <CardTitle>{station ? 'Ladestation bearbeiten' : 'Neue Ladestation anlegen'}</CardTitle>
        <CardDescription>
          {station ? 'Aktualisieren Sie die Ladestationsdaten' : 'Geben Sie die Details für die neue Ladestation ein'}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="stationId">Stations-ID *</Label>
              <Input
                id="stationId"
                value={formData.stationId}
                onChange={(e) => setFormData({ ...formData, stationId: e.target.value })}
                required
                placeholder="z.B. CS-001"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                placeholder="z.B. Haupteingang Station 1"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="vendor">Hersteller</Label>
              <Input
                id="vendor"
                value={formData.vendor}
                onChange={(e) => setFormData({ ...formData, vendor: e.target.value })}
                placeholder="z.B. ABB, Siemens"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="model">Modell</Label>
              <Input
                id="model"
                value={formData.model}
                onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                placeholder="z.B. Terra AC Wallbox"
              />
            </div>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="type">Typ *</Label>
              <select
                id="type"
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                className="w-full rounded-md border border-input bg-background px-3 py-2"
                required
              >
                <option value="AC">AC</option>
                <option value="DC">DC</option>
                <option value="Hybrid">Hybrid</option>
              </select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="maxPower">Max. Leistung (kW) *</Label>
              <Input
                id="maxPower"
                type="number"
                step="0.1"
                value={formData.maxPower}
                onChange={(e) => setFormData({ ...formData, maxPower: parseFloat(e.target.value) })}
                required
                placeholder="22"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="numberOfConnectors">Anzahl Stecker *</Label>
              <Input
                id="numberOfConnectors"
                type="number"
                min="1"
                value={formData.numberOfConnectors}
                onChange={(e) => setFormData({ ...formData, numberOfConnectors: parseInt(e.target.value) })}
                required
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="latitude">Breitengrad</Label>
              <Input
                id="latitude"
                type="number"
                step="0.000001"
                value={formData.latitude || ''}
                onChange={(e) => setFormData({ ...formData, latitude: e.target.value ? parseFloat(e.target.value) : undefined })}
                placeholder="48.1351"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="longitude">Längengrad</Label>
              <Input
                id="longitude"
                type="number"
                step="0.000001"
                value={formData.longitude || ''}
                onChange={(e) => setFormData({ ...formData, longitude: e.target.value ? parseFloat(e.target.value) : undefined })}
                placeholder="11.5820"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notizen</Label>
            <textarea
              id="notes"
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
              placeholder="Zusätzliche Informationen..."
            />
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit">
              {station ? 'Speichern' : 'Ladestation anlegen'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

