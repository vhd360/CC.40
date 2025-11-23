import React, { useState, useEffect, useRef } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Building2, Save, Loader2, CheckCircle, Upload, Trash2, Image as ImageIcon } from 'lucide-react';
import { themes, TenantTheme } from '../themes/tenantThemes';
import { api } from '../services/api';

interface TenantSettings {
  id: string;
  name: string;
  subdomain: string;
  description?: string;
  address?: string;
  postalCode?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  website?: string;
  taxId?: string;
  logoUrl?: string;
  theme: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  userCount: number;
  chargingParkCount: number;
  vehicleCount: number;
}

export const TenantSettings: React.FC = () => {
  const [settings, setSettings] = useState<TenantSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [error, setError] = useState('');
  const [uploadingLogo, setUploadingLogo] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      
      // Get current user from localStorage
      const userStr = localStorage.getItem('user');
      
      if (!userStr) {
        throw new Error('Not authenticated');
      }
      
      const user = JSON.parse(userStr);
      const tenantId = user.tenantId;
      
      // Fetch tenant details
      const data = await api.getTenantById(tenantId);
      setSettings(data as any);
    } catch (err) {
      setError('Fehler beim Laden der Einstellungen');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;

    try {
      setSaving(true);
      setError('');
      setSaveSuccess(false);

      await api.updateTenant(settings.id, {
        name: settings.name,
        subdomain: settings.subdomain,
        description: settings.description,
        address: settings.address,
        postalCode: settings.postalCode,
        city: settings.city,
        country: settings.country,
        phone: settings.phone,
        email: settings.email,
        website: settings.website,
        taxId: settings.taxId,
        theme: settings.theme,
        isActive: settings.isActive
      });

      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
      
      // Update localStorage with new theme to apply immediately
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        user.tenantTheme = settings.theme;
        localStorage.setItem('user', JSON.stringify(user));
        // Reload page to apply theme
        window.location.reload();
      }
    } catch (err: any) {
      setError(err.message || 'Fehler beim Speichern der Einstellungen');
    } finally {
      setSaving(false);
    }
  };

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    try {
      setUploadingLogo(true);
      setError('');

      const data = await api.uploadTenantLogo(file);
      
      if (settings) {
        setSettings({ ...settings, logoUrl: data.logoUrl });
      }

      // Update localStorage
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        user.tenantLogoUrl = data.logoUrl;
        localStorage.setItem('user', JSON.stringify(user));
      }

      setSaveSuccess(true);
      setTimeout(() => {
        setSaveSuccess(false);
        window.location.reload();
      }, 1000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Hochladen des Logos');
    } finally {
      setUploadingLogo(false);
    }
  };

  const handleDeleteLogo = async () => {
    if (!window.confirm('Möchten Sie das Logo wirklich löschen?')) return;

    try {
      setUploadingLogo(true);
      setError('');

      await api.deleteTenantLogo();

      if (settings) {
        setSettings({ ...settings, logoUrl: undefined });
      }

      // Update localStorage
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        user.tenantLogoUrl = null;
        localStorage.setItem('user', JSON.stringify(user));
      }

      setSaveSuccess(true);
      setTimeout(() => {
        setSaveSuccess(false);
        window.location.reload();
      }, 1000);
    } catch (err: any) {
      setError(err.message || 'Fehler beim Löschen des Logos');
    } finally {
      setUploadingLogo(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!settings) {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-800">Keine Tenant-Einstellungen gefunden.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Mandanten-Einstellungen</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Verwalten Sie die Einstellungen Ihres Unternehmens</p>
        </div>
        <Building2 className="h-10 w-10 text-blue-600" />
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-sm text-gray-600 dark:text-gray-400">Benutzer</p>
              <p className="text-3xl font-bold text-gray-900 dark:text-gray-100 mt-2">{settings.userCount}</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-sm text-gray-600 dark:text-gray-400">Ladeparks</p>
              <p className="text-3xl font-bold text-gray-900 dark:text-gray-100 mt-2">{settings.chargingParkCount}</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-sm text-gray-600 dark:text-gray-400">Fahrzeuge</p>
              <p className="text-3xl font-bold text-gray-900 dark:text-gray-100 mt-2">{settings.vehicleCount}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Settings Form */}
      <Card>
        <CardHeader>
          <CardTitle>Unternehmenseinstellungen</CardTitle>
          <CardDescription>
            Aktualisieren Sie Ihre Firmeninformationen und Kontaktdaten
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Basic Information */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Basisinformationen</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Firmenname *</Label>
                  <Input
                    id="name"
                    value={settings.name}
                    onChange={(e) => setSettings({ ...settings, name: e.target.value })}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="subdomain">Subdomain (schreibgeschützt)</Label>
                  <Input
                    id="subdomain"
                    value={settings.subdomain}
                    disabled
                    className="bg-gray-100"
                  />
                  <p className="text-xs text-gray-500 dark:text-gray-400">{settings.subdomain}.chargingcontrol.com</p>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Beschreibung</Label>
                <textarea
                  id="description"
                  value={settings.description || ''}
                  onChange={(e) => setSettings({ ...settings, description: e.target.value })}
                  className="w-full min-h-[80px] rounded-md border border-input bg-background px-3 py-2"
                  placeholder="Kurze Beschreibung Ihres Unternehmens..."
                />
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Branding */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Branding</h3>
              
              {/* Logo Upload */}
              <div className="space-y-2">
                <Label>Firmenlogo</Label>
                <div className="flex items-center space-x-4">
                  {settings.logoUrl ? (
                    <div className="flex items-center space-x-4">
                      <div className="w-32 h-32 border-2 border-gray-200 rounded-lg p-4 flex items-center justify-center bg-gray-50">
                        <img
                          src={`http://localhost:5126${settings.logoUrl}`}
                          alt="Company Logo"
                          className="max-w-full max-h-full object-contain"
                        />
                      </div>
                      <div className="flex flex-col space-y-2">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => fileInputRef.current?.click()}
                          disabled={uploadingLogo}
                        >
                          {uploadingLogo ? (
                            <>
                              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                              Hochladen...
                            </>
                          ) : (
                            <>
                              <Upload className="h-4 w-4 mr-2" />
                              Neues Logo
                            </>
                          )}
                        </Button>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={handleDeleteLogo}
                          disabled={uploadingLogo}
                        >
                          <Trash2 className="h-4 w-4 mr-2" />
                          Löschen
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <div className="flex items-center space-x-4">
                      <div className="w-32 h-32 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center bg-gray-50">
                        <ImageIcon className="h-12 w-12 text-gray-400" />
                      </div>
                      <Button
                        type="button"
                        variant="outline"
                        onClick={() => fileInputRef.current?.click()}
                        disabled={uploadingLogo}
                      >
                        {uploadingLogo ? (
                          <>
                            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                            Hochladen...
                          </>
                        ) : (
                          <>
                            <Upload className="h-4 w-4 mr-2" />
                            Logo hochladen
                          </>
                        )}
                      </Button>
                    </div>
                  )}
                </div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png,image/svg+xml,image/webp"
                  onChange={handleLogoUpload}
                  className="hidden"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  Empfohlen: Transparentes PNG oder SVG, max. 5 MB
                </p>
              </div>

              {/* Theme Selection */}
              <div className="space-y-2">
                <Label htmlFor="theme">Farbschema</Label>
                <select
                  id="theme"
                  value={settings.theme}
                  onChange={(e) => setSettings({ ...settings, theme: parseInt(e.target.value) })}
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                >
                  {Object.values(themes).map((theme) => (
                    <option key={theme.id} value={theme.id}>
                      {theme.name} - {theme.description}
                    </option>
                  ))}
                </select>
                <div className="flex space-x-2 mt-2">
                  {Object.values(themes).map((theme) => (
                    <button
                      key={theme.id}
                      type="button"
                      onClick={() => setSettings({ ...settings, theme: theme.id })}
                      className={`w-10 h-10 rounded-full border-2 transition-all ${
                        settings.theme === theme.id ? 'border-gray-900 scale-110' : 'border-gray-300'
                      }`}
                      style={{ backgroundColor: theme.colors.primary }}
                      title={theme.name}
                    />
                  ))}
                </div>
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Address Information */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Adresse</h3>
              
              <div className="space-y-2">
                <Label htmlFor="address">Straße und Hausnummer</Label>
                <Input
                  id="address"
                  value={settings.address || ''}
                  onChange={(e) => setSettings({ ...settings, address: e.target.value })}
                  placeholder="z.B. Musterstraße 123"
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="postalCode">PLZ</Label>
                  <Input
                    id="postalCode"
                    value={settings.postalCode || ''}
                    onChange={(e) => setSettings({ ...settings, postalCode: e.target.value })}
                    placeholder="12345"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="city">Stadt</Label>
                  <Input
                    id="city"
                    value={settings.city || ''}
                    onChange={(e) => setSettings({ ...settings, city: e.target.value })}
                    placeholder="z.B. Berlin"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="country">Land</Label>
                  <Input
                    id="country"
                    value={settings.country || ''}
                    onChange={(e) => setSettings({ ...settings, country: e.target.value })}
                    placeholder="z.B. Deutschland"
                  />
                </div>
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Contact Information */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Kontaktinformationen</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="phone">Telefon</Label>
                  <Input
                    id="phone"
                    type="tel"
                    value={settings.phone || ''}
                    onChange={(e) => setSettings({ ...settings, phone: e.target.value })}
                    placeholder="+49 123 456789"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">E-Mail</Label>
                  <Input
                    id="email"
                    type="email"
                    value={settings.email || ''}
                    onChange={(e) => setSettings({ ...settings, email: e.target.value })}
                    placeholder="info@firma.de"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="website">Website</Label>
                <Input
                  id="website"
                  type="url"
                  value={settings.website || ''}
                  onChange={(e) => setSettings({ ...settings, website: e.target.value })}
                  placeholder="https://www.firma.de"
                />
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Tax Information */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Steuerinformationen</h3>
              
              <div className="space-y-2">
                <Label htmlFor="taxId">Steuernummer / USt-IdNr.</Label>
                <Input
                  id="taxId"
                  value={settings.taxId || ''}
                  onChange={(e) => setSettings({ ...settings, taxId: e.target.value })}
                  placeholder="DE123456789"
                />
              </div>
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-sm text-red-800">{error}</p>
              </div>
            )}

            {saveSuccess && (
              <div className="bg-green-50 border border-green-200 rounded-lg p-4 flex items-center">
                <CheckCircle className="h-5 w-5 text-green-600 mr-2" />
                <p className="text-sm text-green-800">Einstellungen erfolgreich gespeichert!</p>
              </div>
            )}

            <div className="flex justify-end space-x-4">
              <Button type="button" variant="outline" onClick={loadSettings}>
                Zurücksetzen
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Speichern...
                  </>
                ) : (
                  <>
                    <Save className="h-4 w-4 mr-2" />
                    Speichern
                  </>
                )}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <div className="text-sm text-gray-500 dark:text-gray-400">
        <p>Erstellt am: {new Date(settings.createdAt).toLocaleString('de-DE')}</p>
        {settings.updatedAt && (
          <p>Zuletzt aktualisiert: {new Date(settings.updatedAt).toLocaleString('de-DE')}</p>
        )}
      </div>
    </div>
  );
};

