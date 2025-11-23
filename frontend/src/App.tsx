import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { SignalRProvider } from './contexts/SignalRContext';
import { ToastProvider } from './components/ui/toast';
import { AdminLayout } from './components/AdminLayout';
import { UserLayout } from './components/UserLayout';
import { Login } from './pages/Login';
import { UserRegister } from './pages/UserRegister';
import { Dashboard } from './pages/Dashboard';
import { Tenants } from './pages/Tenants';
import { TenantRegister } from './pages/TenantRegister';
import { TenantDetail } from './pages/TenantDetail';
import { TenantSettings } from './pages/TenantSettings';
import { ChargingParks } from './pages/ChargingParks';
import { ChargingStations } from './pages/ChargingStations';
import { ChargingStationDetail } from './pages/ChargingStationDetail';
import { ChargingStationGroups } from './pages/ChargingStationGroups';
import { ChargingStationGroupDetail } from './pages/ChargingStationGroupDetail';
import { Users } from './pages/Users';
import { UserGroups } from './pages/UserGroups';
import { UserGroupDetail } from './pages/UserGroupDetail';
import { JoinGroup } from './pages/JoinGroup';
import { AuthorizationMethods } from './pages/AuthorizationMethods';
import { Vehicles } from './pages/Vehicles';
import { VehicleAssignments } from './pages/VehicleAssignments';
import { Billing } from './pages/Billing';
import { QrCodes } from './pages/QrCodes';
import { Tariffs } from './pages/Tariffs';

// User Portal Pages
import { UserDashboard } from './pages/UserDashboard';
import { UserStations } from './pages/UserStations';
import { UserSessions } from './pages/UserSessions';
import { SessionDetail } from './pages/SessionDetail';
import { UserCosts } from './pages/UserCosts';
import { UserBilling } from './pages/UserBilling';
import { UserAuthMethods } from './pages/UserAuthMethods';
import { UserVehicles } from './pages/UserVehicles';
import { UserDebug } from './pages/UserDebug';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userRole, setUserRole] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Prüfe Token und Rolle im localStorage
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        setUserRole(user.role);
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Failed to parse user data:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  const handleLogin = () => {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        setUserRole(user.role);
      } catch (error) {
        console.error('Failed to parse user data:', error);
      }
    }
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setIsAuthenticated(false);
    setUserRole(null);
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  const isAdmin = userRole === 'SuperAdmin' || userRole === 'TenantAdmin';
  const isUser = userRole === 'User';

  // Redirect based on role
  const getDefaultRoute = () => {
    if (isUser) return '/user/dashboard';
    return '/';
  };

  return (
    <Router>
      <SignalRProvider>
        <ToastProvider>
          <div className="min-h-screen bg-gray-50 dark:bg-gray-950">
          <Routes>
            {/* Public routes */}
          <Route path="/register" element={<TenantRegister />} />
          <Route path="/user-register" element={<UserRegister />} />
          <Route path="/join-group" element={<JoinGroup />} />
          <Route
            path="/login"
            element={
              isAuthenticated ? (
                <Navigate to={getDefaultRoute()} replace />
              ) : (
                <Login onLogin={handleLogin} />
              )
            }
          />

          {/* User Portal Routes (role: User) */}
          {isAuthenticated && isUser && (
            <Route path="/user/*" element={<UserLayout onLogout={handleLogout} />}>
              <Route path="dashboard" element={<UserDashboard />} />
              <Route path="stations" element={<UserStations />} />
              <Route path="sessions" element={<UserSessions />} />
              <Route path="sessions/:id" element={<SessionDetail />} />
              <Route path="costs" element={<UserCosts />} />
              <Route path="billing" element={<UserBilling />} />
              <Route path="vehicles" element={<UserVehicles />} />
              <Route path="auth-methods" element={<UserAuthMethods />} />
              <Route path="debug" element={<UserDebug />} />
            </Route>
          )}

          {/* Admin Routes (role: SuperAdmin or TenantAdmin) */}
          {isAuthenticated && isAdmin && (
            <Route path="/*" element={<AdminLayout onLogout={handleLogout} />}>
              <Route index element={<Dashboard />} />
              <Route path="settings" element={<TenantSettings />} />
              <Route path="tenants" element={<Tenants />} />
              <Route path="tenants/:id" element={<TenantDetail />} />
              <Route path="charging-parks" element={<ChargingParks />} />
              <Route path="charging-stations" element={<ChargingStations />} />
              <Route path="charging-stations/:id" element={<ChargingStationDetail />} />
              <Route path="charging-station-groups" element={<ChargingStationGroups />} />
              <Route path="charging-station-groups/:id" element={<ChargingStationGroupDetail />} />
              <Route path="users" element={<Users />} />
              <Route path="user-groups" element={<UserGroups />} />
              <Route path="user-groups/:id" element={<UserGroupDetail />} />
              <Route path="authorization-methods" element={<AuthorizationMethods />} />
              <Route path="vehicles" element={<Vehicles />} />
              <Route path="vehicle-assignments" element={<VehicleAssignments />} />
              <Route path="billing" element={<Billing />} />
              <Route path="qrcodes" element={<QrCodes />} />
              <Route path="tariffs" element={<Tariffs />} />
            </Route>
          )}

          {/* Fallback routes */}
          <Route 
            path="*" 
            element={
              isAuthenticated ? (
                <Navigate to={getDefaultRoute()} replace />
              ) : (
                <Navigate to="/login" replace />
              )
            } 
          />
        </Routes>
          </div>
        </ToastProvider>
      </SignalRProvider>
    </Router>
  );
}

export default App;
