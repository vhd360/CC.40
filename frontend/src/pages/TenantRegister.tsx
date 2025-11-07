import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Building2, Check, Loader2 } from 'lucide-react';

export const TenantRegister: React.FC = () => {
  const [formData, setFormData] = useState({
    companyName: '',
    subdomain: '',
    description: '',
    address: '',
    postalCode: '',
    city: '',
    country: '',
    phone: '',
    email: '',
    website: '',
    taxId: '',
    adminFirstName: '',
    adminLastName: '',
    adminEmail: '',
    adminPassword: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Validierung
    if (formData.adminPassword !== formData.confirmPassword) {
      setError('Passwörter stimmen nicht überein');
      return;
    }

    if (formData.adminPassword.length < 6) {
      setError('Passwort muss mindestens 6 Zeichen lang sein');
      return;
    }

    try {
      setLoading(true);
      const response = await fetch('http://localhost:5126/api/tenants/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          companyName: formData.companyName,
          subdomain: formData.subdomain,
          description: formData.description,
          address: formData.address,
          postalCode: formData.postalCode,
          city: formData.city,
          country: formData.country,
          phone: formData.phone,
          email: formData.email,
          website: formData.website,
          taxId: formData.taxId,
          adminFirstName: formData.adminFirstName,
          adminLastName: formData.adminLastName,
          adminEmail: formData.adminEmail,
          adminPassword: formData.adminPassword
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        setError(errorData.error || 'Fehler bei der Registrierung');
        return;
      }

      setSuccess(true);
      
      // Nach 3 Sekunden zur Login-Seite weiterleiten
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      setError('Netzwerkfehler. Bitte versuchen Sie es später erneut.');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6">
            <div className="text-center space-y-4">
              <div className="flex justify-center">
                <div className="rounded-full bg-green-100 p-3">
                  <Check className="h-8 w-8 text-green-600" />
                </div>
              </div>
              <h2 className="text-2xl font-bold text-gray-900">Registrierung erfolgreich!</h2>
              <p className="text-gray-600">
                Ihr Tenant wurde erfolgreich erstellt. Sie werden gleich zur Login-Seite weitergeleitet.
              </p>
              <p className="text-sm text-gray-500">
                Ihre URL: <span className="font-mono text-blue-600">{formData.subdomain}.chargingcontrol.com</span>
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <Card className="w-full max-w-2xl">
        <CardHeader>
          <div className="flex items-center justify-center mb-4">
            <Building2 className="h-12 w-12 text-blue-600" />
          </div>
          <CardTitle className="text-2xl text-center">Tenant Registrierung</CardTitle>
          <CardDescription className="text-center">
            Erstellen Sie Ihren eigenen Lade-Management Account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Firmeninformationen */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Firmeninformationen</h3>
              
              <div className="space-y-2">
                <Label htmlFor="companyName">Firmenname *</Label>
                <Input
                  id="companyName"
                  value={formData.companyName}
                  onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
                  placeholder="z.B. ACME Charging GmbH"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="subdomain">Subdomain *</Label>
                <div className="flex items-center space-x-2">
                  <Input
                    id="subdomain"
                    value={formData.subdomain}
                    onChange={(e) => setFormData({ 
                      ...formData, 
                      subdomain: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') 
                    })}
                    placeholder="acme"
                    pattern="[a-z0-9-]+"
                    required
                  />
                  <span className="text-sm text-gray-500 whitespace-nowrap">.chargingcontrol.com</span>
                </div>
                <p className="text-xs text-gray-500">
                  Dies wird Ihre persönliche URL. Nur Kleinbuchstaben, Zahlen und Bindestriche erlaubt.
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Beschreibung (optional)</Label>
                <textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full min-h-[60px] rounded-md border border-input bg-background px-3 py-2"
                  placeholder="Kurze Beschreibung Ihres Unternehmens..."
                />
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Adressinformationen */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Adresse (optional)</h3>
              
              <div className="space-y-2">
                <Label htmlFor="address">Straße und Hausnummer</Label>
                <Input
                  id="address"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  placeholder="z.B. Musterstraße 123"
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="postalCode">PLZ</Label>
                  <Input
                    id="postalCode"
                    value={formData.postalCode}
                    onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                    placeholder="12345"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="city">Stadt</Label>
                  <Input
                    id="city"
                    value={formData.city}
                    onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                    placeholder="z.B. Berlin"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="country">Land</Label>
                  <Input
                    id="country"
                    value={formData.country}
                    onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                    placeholder="Deutschland"
                  />
                </div>
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Kontaktinformationen */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Kontaktinformationen (optional)</h3>
              
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="phone">Telefon</Label>
                  <Input
                    id="phone"
                    type="tel"
                    value={formData.phone}
                    onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                    placeholder="+49 123 456789"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">Firmen-E-Mail</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    placeholder="info@firma.de"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="website">Website</Label>
                <Input
                  id="website"
                  type="url"
                  value={formData.website}
                  onChange={(e) => setFormData({ ...formData, website: e.target.value })}
                  placeholder="https://www.firma.de"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="taxId">Steuernummer / USt-IdNr. (optional)</Label>
                <Input
                  id="taxId"
                  value={formData.taxId}
                  onChange={(e) => setFormData({ ...formData, taxId: e.target.value })}
                  placeholder="DE123456789"
                />
              </div>
            </div>

            <div className="border-t pt-6" />

            {/* Admin-Benutzer */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Administrator Account</h3>
              
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="adminFirstName">Vorname *</Label>
                  <Input
                    id="adminFirstName"
                    value={formData.adminFirstName}
                    onChange={(e) => setFormData({ ...formData, adminFirstName: e.target.value })}
                    placeholder="Max"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="adminLastName">Nachname *</Label>
                  <Input
                    id="adminLastName"
                    value={formData.adminLastName}
                    onChange={(e) => setFormData({ ...formData, adminLastName: e.target.value })}
                    placeholder="Mustermann"
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="adminEmail">E-Mail *</Label>
                <Input
                  id="adminEmail"
                  type="email"
                  value={formData.adminEmail}
                  onChange={(e) => setFormData({ ...formData, adminEmail: e.target.value })}
                  placeholder="max.mustermann@acme.de"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="adminPassword">Passwort *</Label>
                <Input
                  id="adminPassword"
                  type="password"
                  value={formData.adminPassword}
                  onChange={(e) => setFormData({ ...formData, adminPassword: e.target.value })}
                  placeholder="Mindestens 6 Zeichen"
                  required
                  minLength={6}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Passwort bestätigen *</Label>
                <Input
                  id="confirmPassword"
                  type="password"
                  value={formData.confirmPassword}
                  onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                  placeholder="Passwort wiederholen"
                  required
                  minLength={6}
                />
              </div>
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-sm text-red-800">{error}</p>
              </div>
            )}

            <div className="flex flex-col space-y-2 pt-4">
              <Button type="submit" disabled={loading} className="w-full">
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Registrierung läuft...
                  </>
                ) : (
                  <>
                    <Building2 className="h-4 w-4 mr-2" />
                    Jetzt registrieren
                  </>
                )}
              </Button>
              
              <Button 
                type="button" 
                variant="outline" 
                onClick={() => navigate('/login')}
                className="w-full"
              >
                Bereits registriert? Zum Login
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};

