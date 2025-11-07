import React, { useState } from 'react';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Upload, X } from 'lucide-react';

interface ChargingPointFormProps {
  chargingStationId: string;
  chargingPoint?: any;
  onSubmit: (data: ChargingPointFormData) => void;
  onCancel: () => void;
}

export interface ChargingPointFormData {
  chargingStationId: string;
  evseId: number;
  evseIdExternal?: string;
  name: string;
  description?: string;
  maxPower: number;
  status: number;
  supportsSmartCharging: boolean;
  supportsRemoteStartStop: boolean;
  supportsReservation: boolean;
  publicKey?: string;
  certificateChain?: string;
  tariffInfo?: string;
  notes?: string;
}

export const ChargingPointForm: React.FC<ChargingPointFormProps> = ({ 
  chargingStationId, 
  chargingPoint, 
  onSubmit, 
  onCancel 
}) => {
  const [formData, setFormData] = useState<ChargingPointFormData>({
    chargingStationId: chargingStationId,
    evseId: chargingPoint?.evseId || 1,
    evseIdExternal: chargingPoint?.evseIdExternal || '',
    name: chargingPoint?.name || '',
    description: chargingPoint?.description || '',
    maxPower: chargingPoint?.maxPower || 22,
    status: chargingPoint?.status || 0,
    supportsSmartCharging: chargingPoint?.supportsSmartCharging || false,
    supportsRemoteStartStop: chargingPoint?.supportsRemoteStartStop || true,
    supportsReservation: chargingPoint?.supportsReservation || false,
    publicKey: chargingPoint?.publicKey || '',
    certificateChain: chargingPoint?.certificateChain || '',
    tariffInfo: chargingPoint?.tariffInfo || '',
    notes: chargingPoint?.notes || ''
  });

  const [certificateFile, setCertificateFile] = useState<File | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const handleCertificateUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setCertificateFile(file);
      const reader = new FileReader();
      reader.onload = (event) => {
        const content = event.target?.result as string;
        setFormData({ ...formData, publicKey: content });
      };
      reader.readAsText(file);
    }
  };

  return (
    <Card className="w-full max-w-4xl">
      <CardHeader>
        <CardTitle>
          {chargingPoint ? 'Ladepunkt bearbeiten' : 'Neuen Ladepunkt anlegen'}
        </CardTitle>
        <CardDescription>
          Ein Ladepunkt (EVSE) entspricht einem OCPP ConnectorId und kann mehrere physische Stecker haben
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Grunddaten */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium">Grunddaten</h3>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="evseId">EVSE-ID (OCPP ConnectorId) *</Label>
                <Input
                  id="evseId"
                  type="number"
                  min="1"
                  value={formData.evseId}
                  onChange={(e) => setFormData({ ...formData, evseId: parseInt(e.target.value) })}
                  required
                  placeholder="1"
                />
                <p className="text-xs text-gray-500">
                  Interne ID für OCPP-Kommunikation (1-basiert)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="evseIdExternal">Externe EVSE-ID (für eRoaming)</Label>
                <Input
                  id="evseIdExternal"
                  value={formData.evseIdExternal}
                  onChange={(e) => setFormData({ ...formData, evseIdExternal: e.target.value })}
                  placeholder="DE*ABC*E1234*5678"
                />
                <p className="text-xs text-gray-500">
                  Format: Country*OperatorId*E[PowerOutletId]*[StationId]
                </p>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                placeholder="z.B. Ladepunkt 1, Linke Säule"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Beschreibung</Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Optionale Beschreibung"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="maxPower">Maximale Leistung (kW) *</Label>
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
                <Label htmlFor="status">Status</Label>
                <select
                  id="status"
                  value={formData.status}
                  onChange={(e) => setFormData({ ...formData, status: parseInt(e.target.value) })}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                >
                  <option value="0">Verfügbar</option>
                  <option value="1">Belegt</option>
                  <option value="2">Lädt</option>
                  <option value="3">Reserviert</option>
                  <option value="4">Defekt</option>
                  <option value="5">Nicht verfügbar</option>
                  <option value="6">Vorbereitung</option>
                  <option value="7">Abschluss</option>
                </select>
              </div>
            </div>
          </div>

          {/* Funktionen */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium">Unterstützte Funktionen</h3>
            <div className="space-y-3">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={formData.supportsSmartCharging}
                  onChange={(e) => setFormData({ ...formData, supportsSmartCharging: e.target.checked })}
                  className="rounded border-gray-300"
                />
                <div>
                  <span className="font-medium">Smart Charging</span>
                  <p className="text-xs text-gray-500">Dynamische Laststeuerung</p>
                </div>
              </label>

              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={formData.supportsRemoteStartStop}
                  onChange={(e) => setFormData({ ...formData, supportsRemoteStartStop: e.target.checked })}
                  className="rounded border-gray-300"
                />
                <div>
                  <span className="font-medium">Remote Start/Stop</span>
                  <p className="text-xs text-gray-500">Ferngesteuertes Starten/Stoppen via OCPP</p>
                </div>
              </label>

              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={formData.supportsReservation}
                  onChange={(e) => setFormData({ ...formData, supportsReservation: e.target.checked })}
                  className="rounded border-gray-300"
                />
                <div>
                  <span className="font-medium">Reservierung</span>
                  <p className="text-xs text-gray-500">Ladepunkt kann reserviert werden</p>
                </div>
              </label>
            </div>
          </div>

          {/* Plug & Charge (ISO 15118) */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium">Plug & Charge (ISO 15118)</h3>
            <p className="text-sm text-gray-600">
              Laden Sie ein X.509-Zertifikat für automatische Authentifizierung hoch
            </p>

            <div className="space-y-2">
              <Label htmlFor="certificateUpload">Zertifikat (PEM-Format)</Label>
              <div className="flex items-center space-x-2">
                <Input
                  id="certificateUpload"
                  type="file"
                  accept=".pem,.crt,.cer"
                  onChange={handleCertificateUpload}
                  className="flex-1"
                />
                {certificateFile && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setCertificateFile(null);
                      setFormData({ ...formData, publicKey: '', certificateChain: '' });
                    }}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                )}
              </div>
              {certificateFile && (
                <div className="text-xs text-green-600 mt-1">
                  ✓ {certificateFile.name} hochgeladen
                </div>
              )}
              {formData.publicKey && (
                <div className="mt-2 p-2 bg-gray-100 rounded text-xs font-mono break-all max-h-32 overflow-y-auto">
                  {formData.publicKey.substring(0, 200)}...
                </div>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="certificateChain">Zertifikatskette (optional)</Label>
              <textarea
                id="certificateChain"
                value={formData.certificateChain}
                onChange={(e) => setFormData({ ...formData, certificateChain: e.target.value })}
                className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2 font-mono text-xs"
                placeholder="-----BEGIN CERTIFICATE-----..."
              />
            </div>
          </div>

          {/* Tarif und Notizen */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="tariffInfo">Tarifinfo (JSON)</Label>
              <textarea
                id="tariffInfo"
                value={formData.tariffInfo}
                onChange={(e) => setFormData({ ...formData, tariffInfo: e.target.value })}
                className="w-full min-h-[60px] rounded-md border border-input bg-background px-3 py-2 font-mono text-xs"
                placeholder='{"pricePerKwh": 0.30, "currency": "EUR"}'
              />
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
          </div>

          <div className="flex justify-end space-x-2 pt-4 border-t">
            <Button type="button" variant="outline" onClick={onCancel}>
              Abbrechen
            </Button>
            <Button type="submit">
              {chargingPoint ? 'Speichern' : 'Ladepunkt anlegen'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

