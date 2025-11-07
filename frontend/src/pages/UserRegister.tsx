import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Loader2, UserPlus, CheckCircle, Users } from 'lucide-react';

export const UserRegister: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const inviteToken = searchParams.get('inviteToken');
  
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [joinedGroupName, setJoinedGroupName] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Passwörter stimmen nicht überein');
      return;
    }

    if (formData.password.length < 6) {
      setError('Passwort muss mindestens 6 Zeichen lang sein');
      return;
    }

    try {
      setLoading(true);

      const response = await fetch('http://localhost:5126/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          FirstName: formData.firstName,
          LastName: formData.lastName,
          Email: formData.email,
          PhoneNumber: formData.phoneNumber || null,
          Password: formData.password,
          InviteToken: inviteToken || null
        })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || errorData.message || 'Registrierung fehlgeschlagen');
      }

      const data = await response.json();
      
      // Check if user was automatically added to a group
      if (data.joinedGroupName) {
        setJoinedGroupName(data.joinedGroupName);
      }
      
      setSuccess(true);
      
      // Redirect to login after 3 seconds
      setTimeout(() => {
        navigate('/login');
      }, 3000);

    } catch (err: any) {
      console.error('Registration error:', err);
      setError(err.message || 'Fehler bei der Registrierung');
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
              <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
                <CheckCircle className="h-10 w-10 text-green-600" />
              </div>
              <div>
                <h2 className="text-2xl font-bold text-gray-900">Erfolgreich registriert!</h2>
                <p className="text-gray-600 mt-2">
                  Ihr Konto wurde erstellt. Sie werden in Kürze zur Anmeldeseite weitergeleitet...
                </p>
                {joinedGroupName && (
                  <div className="mt-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                    <div className="flex items-center justify-center space-x-2 mb-2">
                      <Users className="h-5 w-5 text-blue-600" />
                      <h3 className="font-semibold text-blue-900">Gruppe beigetreten!</h3>
                    </div>
                    <p className="text-sm text-blue-800">
                      Sie wurden automatisch zur Gruppe <strong>{joinedGroupName}</strong> hinzugefügt.
                    </p>
                    <p className="text-xs text-blue-700 mt-2">
                      💡 Vergessen Sie nicht, nach der Anmeldung eine Identifikationsmethode hinzuzufügen!
                    </p>
                  </div>
                )}
              </div>
              <Button 
                onClick={() => navigate('/login')}
                className="w-full"
              >
                Jetzt anmelden
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <div className="flex items-center justify-center mb-4">
            <div className="p-3 bg-blue-100 rounded-full">
              <UserPlus className="h-8 w-8 text-blue-600" />
            </div>
          </div>
          <CardTitle className="text-2xl text-center">Konto erstellen</CardTitle>
          <CardDescription className="text-center">
            Registrieren Sie sich, um Zugang zum Lade-Netzwerk zu erhalten
          </CardDescription>
          {inviteToken && (
            <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg">
              <div className="flex items-center space-x-2 mb-1">
                <Users className="h-5 w-5 text-green-600" />
                <h3 className="font-semibold text-green-900">Sie wurden eingeladen!</h3>
              </div>
              <p className="text-sm text-green-800">
                Nach der Registrierung werden Sie automatisch einer Nutzergruppe hinzugefügt und erhalten 
                Zugriff auf Ladestationen.
              </p>
            </div>
          )}
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
                {error}
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="firstName">Vorname *</Label>
                <Input
                  id="firstName"
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  placeholder="Max"
                  required
                  disabled={loading}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Nachname *</Label>
                <Input
                  id="lastName"
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  placeholder="Mustermann"
                  required
                  disabled={loading}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">E-Mail-Adresse *</Label>
              <Input
                id="email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                placeholder="max@beispiel.de"
                required
                disabled={loading}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="phoneNumber">Telefonnummer (optional)</Label>
              <Input
                id="phoneNumber"
                type="tel"
                value={formData.phoneNumber}
                onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                placeholder="+49 123 456789"
                disabled={loading}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Passwort *</Label>
              <Input
                id="password"
                type="password"
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                placeholder="Mindestens 6 Zeichen"
                required
                minLength={6}
                disabled={loading}
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
                disabled={loading}
              />
            </div>

            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <p className="text-sm text-blue-800">
                <strong>Hinweis:</strong> Nach der Registrierung können Sie sich anmelden und 
                über QR-Codes Nutzergruppen beitreten, um Zugriff auf Ladestationen zu erhalten.
              </p>
            </div>

            <Button 
              type="submit" 
              className="w-full" 
              disabled={loading}
            >
              {loading ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Registrierung läuft...
                </>
              ) : (
                <>
                  <UserPlus className="h-4 w-4 mr-2" />
                  Jetzt registrieren
                </>
              )}
            </Button>

            <div className="text-center text-sm">
              <span className="text-gray-600">Bereits registriert?</span>{' '}
              <button
                type="button"
                onClick={() => navigate('/login')}
                className="text-blue-600 hover:underline font-medium"
              >
                Jetzt anmelden
              </button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};

