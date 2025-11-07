import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Loader2, CheckCircle, XCircle, Users, CreditCard, ArrowRight, Home } from 'lucide-react';
import { api } from '../services/api';

export const JoinGroup: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [token, setToken] = useState(searchParams.get('token') || '');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [groupName, setGroupName] = useState<string>('');
  const [hasAuthMethods, setHasAuthMethods] = useState(false);
  const [checkingAuthMethods, setCheckingAuthMethods] = useState(false);

  // Check if user is authenticated
  useEffect(() => {
    const authToken = localStorage.getItem('token');
    const urlToken = searchParams.get('token');
    
    if (!authToken) {
      // User is not logged in - redirect to register with invite token
      if (urlToken) {
        navigate(`/user-register?inviteToken=${urlToken}`);
      } else {
        navigate('/login');
      }
      return;
    }

    // User is authenticated - auto-join if token is in URL
    if (urlToken) {
      handleJoin(urlToken);
    }
  }, []);

  const checkAuthorizationMethods = async () => {
    try {
      setCheckingAuthMethods(true);
      const userStr = localStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const methods = await api.getAuthorizationMethodsByUser(user.id);
      setHasAuthMethods(methods.length > 0);
    } catch (err) {
      console.error('Failed to check authorization methods:', err);
    } finally {
      setCheckingAuthMethods(false);
    }
  };

  const handleJoin = async (tokenToUse?: string) => {
    const joinToken = tokenToUse || token;
    if (!joinToken.trim()) {
      setError('Bitte geben Sie einen Einladungs-Token ein');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const result = await api.joinGroupWithToken(joinToken);
      setSuccess(true);
      setGroupName(result.groupName);
      
      // Check if user has authorization methods
      await checkAuthorizationMethods();
    } catch (err: any) {
      setError(err.message || 'Fehler beim Beitreten zur Gruppe');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleJoin();
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <Card className="w-full max-w-2xl">
        <CardHeader>
          <div className="flex items-center justify-center mb-4">
            <Users className="h-12 w-12 text-blue-600" />
          </div>
          <CardTitle className="text-center">Nutzergruppe beitreten</CardTitle>
          <CardDescription className="text-center">
            Verwenden Sie den Einladungs-Token, um einer Gruppe beizutreten
          </CardDescription>
        </CardHeader>
        <CardContent>
          {success ? (
            <div className="space-y-6">
              <div className="text-center">
                <CheckCircle className="h-16 w-16 text-green-600 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900">Erfolgreich beigetreten!</h3>
                <p className="text-gray-600 mt-2">
                  Sie sind jetzt Mitglied der Gruppe <strong>{groupName}</strong>
                </p>
              </div>

              {/* Warning if no authorization methods */}
              {!checkingAuthMethods && !hasAuthMethods && (
                <div className="bg-amber-50 border-2 border-amber-200 rounded-lg p-4">
                  <div className="flex items-start space-x-3">
                    <CreditCard className="h-6 w-6 text-amber-600 flex-shrink-0 mt-0.5" />
                    <div className="text-left flex-1">
                      <h4 className="font-semibold text-amber-900 mb-1">
                        ⚠️ Wichtiger nächster Schritt
                      </h4>
                      <p className="text-sm text-amber-800 mb-3">
                        Um an den Ladestationen dieser Gruppe laden zu können, müssen Sie noch eine 
                        <strong> Identifikationsmethode hinzufügen</strong> (z.B. RFID-Karte, Autocharge, App).
                      </p>
                      <p className="text-xs text-amber-700 mb-3">
                        💡 Ohne Identifikationsmethode können Sie sich nicht an den Ladestationen authentifizieren!
                      </p>
                      <Button 
                        onClick={() => navigate('/authorization-methods')} 
                        className="w-full bg-amber-600 hover:bg-amber-700"
                        size="sm"
                      >
                        <CreditCard className="h-4 w-4 mr-2" />
                        Jetzt Identifikationsmethode hinzufügen
                        <ArrowRight className="h-4 w-4 ml-2" />
                      </Button>
                    </div>
                  </div>
                </div>
              )}

              {/* Success message if already has auth methods */}
              {!checkingAuthMethods && hasAuthMethods && (
                <div className="bg-green-50 border-2 border-green-200 rounded-lg p-4">
                  <div className="flex items-start space-x-3">
                    <CheckCircle className="h-6 w-6 text-green-600 flex-shrink-0 mt-0.5" />
                    <div className="text-left flex-1">
                      <h4 className="font-semibold text-green-900 mb-1">
                        ✅ Sie sind startklar!
                      </h4>
                      <p className="text-sm text-green-800">
                        Sie haben bereits Identifikationsmethoden registriert und können jetzt an den 
                        Ladestationen dieser Gruppe laden.
                      </p>
                    </div>
                  </div>
                </div>
              )}

              {/* Action buttons */}
              <div className="flex flex-col gap-2">
                {hasAuthMethods && (
                  <Button 
                    onClick={() => navigate('/authorization-methods')} 
                    variant="outline"
                    className="w-full"
                  >
                    <CreditCard className="h-4 w-4 mr-2" />
                    Identifikationsmethoden verwalten
                  </Button>
                )}
                <Button 
                  onClick={() => navigate('/')} 
                  className="w-full"
                  variant={hasAuthMethods ? "default" : "outline"}
                >
                  <Home className="h-4 w-4 mr-2" />
                  Zum Dashboard
                </Button>
              </div>
            </div>
          ) : error ? (
            <div className="text-center space-y-4">
              <XCircle className="h-16 w-16 text-red-600 mx-auto" />
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Fehler</h3>
                <p className="text-red-600 mt-2">{error}</p>
              </div>
              <Button 
                onClick={() => {
                  setError(null);
                  setToken('');
                }} 
                variant="outline"
                className="w-full"
              >
                Erneut versuchen
              </Button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="token">Einladungs-Token</Label>
                <Input
                  id="token"
                  type="text"
                  value={token}
                  onChange={(e) => setToken(e.target.value)}
                  placeholder="Token hier einfügen"
                  required
                  disabled={loading}
                />
                <p className="text-xs text-gray-500">
                  Der Token wurde Ihnen von einem Gruppenadministrator zur Verfügung gestellt
                </p>
              </div>

              <Button 
                type="submit" 
                className="w-full" 
                disabled={loading || !token.trim()}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Beitreten...
                  </>
                ) : (
                  'Gruppe beitreten'
                )}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

