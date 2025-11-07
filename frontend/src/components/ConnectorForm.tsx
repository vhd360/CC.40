import React, { useState } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';

interface ConnectorFormProps {
  chargingPointId: string;
  connector?: any;
  onSubmit: (data: ConnectorFormData) => void;
  onCancel: () => void;
}

export interface ConnectorFormData {
  chargingPointId: string;
  connectorId: number;
  connectorType: string;
  connectorFormat?: string;
  powerType?: string;
  maxPower: number;
  maxCurrent: number;
  maxVoltage: number;
  status: number;
  physicalReference?: string;
  notes?: string;
}

export const ConnectorForm: React.FC<ConnectorFormProps> = ({ 
  chargingPointId, 
  connector, 
  onSubmit, 
  onCancel 
}) => {
  const [formData, setFormData] = useState<ConnectorFormData>({
    chargingPointId: chargingPointId,
    connectorId: connector?.connectorId || 1,
    connectorType: connector?.connectorType || 'Type2',
    connectorFormat: connector?.connectorFormat || 'SOCKET',
    powerType: connector?.powerType || 'AC_3_PHASE',
    maxPower: connector?.maxPower || 22,
    maxCurrent: connector?.maxCurrent || 32,
    maxVoltage: connector?.maxVoltage || 230,
    status: connector?.status || 0,
    physicalReference: connector?.physicalReference || '',
    notes: connector?.notes || ''
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  // Vordefinierte Connector-Typen
  const connectorTypes = [
    'Type1',
    'Type2',
    'CCS',
    'CHAdeMO',
    'Tesla',
    'Schuko',
    'CEE',
    'GB/T'
  ];

  const connectorFormats = [
    { value: 'SOCKET', label: 'Steckdose' },
    { value: 'CABLE', label: 'Fest montiertes Kabel' }
  ];

  const powerTypes = [
    { value: 'AC_1_PHASE', label: 'AC 1-phasig' },
    { value: 'AC_3_PHASE', label: 'AC 3-phasig' },
    { value: 'DC', label: 'DC Gleichstrom' }
  ];

  return (
    <Card className="w-full max-w-2xl">
      <CardHeader>
        <CardTitle>
          {connector ? 'Stecker bearbeiten' : 'Neuen Stecker anlegen'}
        </CardTitle>
        <CardDescription>
          Physischer Stecker/Anschluss am Ladepunkt
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="connectorId">Stecker-Nummer *</Label>
              <Input
                id="connectorId"
                type="number"
                min="1"
                value={formData.connectorId}
                onChange={(e) => setFormData({ ...formData, connectorId: parseInt(e.target.value) })}
                required
                placeholder="1"
              />
              <p className="text-xs text-gray-500">
                Nummer des Steckers am Ladepunkt (1-basiert)
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="status">Status</Label>
              <select
                id="status"
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: parseInt(e.target.value) })}
                className="w-full rounded-md border border-input bg-background px-3 py-2"
              >
                <option value="0">Verfügbar</option>
                <option value="1">Belegt</option>
                <option value="2">Defekt</option>
                <option value="3">Nicht verfügbar</option>
                <option value="4">Reserviert</option>
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="connectorType">Stecker-Typ *</Label>
              <select
                id="connectorType"
                value={formData.connectorType}
                onChange={(e) => setFormData({ ...formData, connectorType: e.target.value })}
                className="w-full rounded-md border border-input bg-background px-3 py-2"
                required
              >
                {connectorTypes.map(type => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="connectorFormat">Format</Label>
              <select
                id="connectorFormat"
                value={formData.connectorFormat}
                onChange={(e) => setFormData({ ...formData, connectorFormat: e.target.value })}
                className="w-full rounded-md border border-input bg-background px-3 py-2"
              >
                {connectorFormats.map(format => (
                  <option key={format.value} value={format.value}>{format.label}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="powerType">Stromart *</Label>
            <select
              id="powerType"
              value={formData.powerType}
              onChange={(e) => setFormData({ ...formData, powerType: e.target.value })}
              className="w-full rounded-md border border-input bg-background px-3 py-2"
              required
            >
              {powerTypes.map(type => (
                <option key={type.value} value={type.value}>{type.label}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="maxPower">Max. Leistung (kW) *</Label>
              <Input
                id="maxPower"
                type="number"
                step="0.1"
                min="0"
                value={formData.maxPower}
                onChange={(e) => setFormData({ ...formData, maxPower: parseFloat(e.target.value) })}
                required
                placeholder="22"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="maxCurrent">Max. Strom (A) *</Label>
              <Input
                id="maxCurrent"
                type="number"
                min="0"
                value={formData.maxCurrent}
                onChange={(e) => setFormData({ ...formData, maxCurrent: parseInt(e.target.value) })}
                required
                placeholder="32"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="maxVoltage">Max. Spannung (V) *</Label>
              <Input
                id="maxVoltage"
                type="number"
                min="0"
                value={formData.maxVoltage}
                onChange={(e) => setFormData({ ...formData, maxVoltage: parseInt(e.target.value) })}
                required
                placeholder="230"
              />
            </div>
          </div>

          <div className="p-3 bg-blue-50 rounded-lg text-sm">
            <div className="font-medium text-blue-900">Berechnete Leistung</div>
            <div className="text-blue-700 mt-1">
              {formData.powerType === 'AC_3_PHASE' 
                ? `≈ ${Math.round(Math.sqrt(3) * formData.maxVoltage * formData.maxCurrent / 1000)} kW (3-phasig)`
                : `≈ ${Math.round(formData.maxVoltage * formData.maxCurrent / 1000)} kW (1-phasig/DC)`
              }
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="physicalReference">Physische Referenz</Label>
            <Input
              id="physicalReference"
              value={formData.physicalReference}
              onChange={(e) => setFormData({ ...formData, physicalReference: e.target.value })}
              placeholder="z.B. Links, Rechts, A, B"
            />
            <p className="text-xs text-gray-500">
              Beschriftung am Ladepunkt zur Identifikation
            </p>
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

          <div className="flex justify-end space-x-2 pt-4 border-t">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit">
              {connector ? 'Speichern' : 'Stecker anlegen'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

