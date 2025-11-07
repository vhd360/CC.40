import React, { useState } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';

interface ChargingParkFormProps {
  park?: any;
  onSubmit: (data: ChargingParkFormData) => void;
  onCancel: () => void;
}

export interface ChargingParkFormData {
  name: string;
  description?: string;
  address: string;
  postalCode: string;
  city: string;
  country: string;
  latitude?: number;
  longitude?: number;
}

export const ChargingParkForm: React.FC<ChargingParkFormProps> = ({ park, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState<ChargingParkFormData>({
    name: park?.name || '',
    description: park?.description || '',
    address: park?.address || '',
    postalCode: park?.postalCode || '',
    city: park?.city || '',
    country: park?.country || 'Deutschland',
    latitude: park?.latitude || undefined,
    longitude: park?.longitude || undefined
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <Card className="w-full max-w-3xl">
      <CardHeader>
        <CardTitle>{park ? 'Ladepark bearbeiten' : 'Neuer Ladepark anlegen'}</CardTitle>
        <CardDescription>
          {park ? 'Aktualisieren Sie die Ladepark-Daten' : 'Geben Sie die Details für den neuen Ladepark ein'}
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
              placeholder="z.B. Ladepark München Süd"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Beschreibung</Label>
            <textarea
              id="description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
              placeholder="Zusätzliche Informationen zum Ladepark..."
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="col-span-2 space-y-2">
              <Label htmlFor="address">Adresse *</Label>
              <Input
                id="address"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                required
                placeholder="Straße und Hausnummer"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="postalCode">PLZ *</Label>
              <Input
                id="postalCode"
                value={formData.postalCode}
                onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                required
                placeholder="80331"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="city">Stadt *</Label>
              <Input
                id="city"
                value={formData.city}
                onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                required
                placeholder="München"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="country">Land *</Label>
            <Input
              id="country"
              value={formData.country}
              onChange={(e) => setFormData({ ...formData, country: e.target.value })}
              required
            />
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

          <div className="flex justify-end space-x-2 pt-4">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit">
              {park ? 'Speichern' : 'Ladepark anlegen'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

